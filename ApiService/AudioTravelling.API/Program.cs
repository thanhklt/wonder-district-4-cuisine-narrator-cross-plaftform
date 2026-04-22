using System.Text;
using System.Threading.RateLimiting;
using AudioTravelling.API.Hubs;
using AudioTravelling.Core.Interfaces;
using AudioTravelling.Infrastructure.Persistence;
using AudioTravelling.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// ── Database ───────────────────────────────────────────────
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? "Host=localhost;Database=audiotravelling;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// ── JWT Auth ───────────────────────────────────────────────
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "default_secret_change_me_32_chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ── HTTP Clients ───────────────────────────────────────────
builder.Services.AddHttpClient("DeepTranslate", c =>
    c.BaseAddress = new Uri(builder.Configuration["DEEP_TRANSLATE_URL"] ?? "http://localhost:8000"));
builder.Services.AddHttpClient("TTS", c =>
    c.BaseAddress = new Uri(builder.Configuration["TTS_URL"] ?? "http://localhost:8001"));

// ── Services ───────────────────────────────────────────────
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IOnlineTracker, OnlineTracker>();
builder.Services.AddSingleton<VnPayService>();

// ── Rate Limiting ──────────────────────────────────────────
builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("api", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 10;
    });
    opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── SignalR + CORS + Controllers ───────────────────────────
builder.Services.AddSignalR();
builder.Services.AddCors(opt => opt.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Auto-migrate on startup ────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.Migrate();
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve audio files from shared volume
var audioPath = builder.Configuration["AUDIO_STORAGE_PATH"] ?? "/storage/audio";
Directory.CreateDirectory(audioPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(audioPath)),
    RequestPath = "/audio"
});

app.MapControllers().RequireRateLimiting("api");
app.MapHub<AdminHub>("/hubs/admin");

app.Run();
