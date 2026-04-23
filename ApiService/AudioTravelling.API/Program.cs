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
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// ── Database ───────────────────────────────────────────────
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=host.docker.internal\\SQLEXPRESS;Database=audiotravelling;User Id=sa;Password=Sa@123456;TrustServerCertificate=True;Encrypt=False;";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));
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
builder.Services.AddSingleton<VnPayService>();
builder.Services.AddHostedService<AudioTravelling.API.Services.ActiveSessionBroadcaster>();

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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new() { Title = "AudioTravelling API", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token (không cần prefix 'Bearer ')",
    });
    opt.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseSwagger();
app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/swagger/v1/swagger.json", "AudioTravelling API v1"));

// Serve audio files from shared volume
var audioPath = builder.Configuration["AUDIO_STORAGE_PATH"] ?? "/storage/audio";
Directory.CreateDirectory(audioPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(audioPath)),
    RequestPath = "/audio"
});

// Serve QR code images
var qrPath = builder.Configuration["QR_STORAGE_PATH"] ?? "/storage/qr";
Directory.CreateDirectory(qrPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(qrPath)),
    RequestPath = "/qr"
});

app.MapControllers().RequireRateLimiting("api");
app.MapHub<AdminHub>("/hubs/admin");

app.Run();
