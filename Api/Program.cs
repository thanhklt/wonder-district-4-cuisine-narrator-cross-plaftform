using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Identity;
using Api.Models.Entities;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Tu dong map PascalCase sang camelCase
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Jwt
void ConfigureOptions(AuthenticationOptions options)
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}

void BearerOptions(JwtBearerOptions options)
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
        )
    };
}

builder.Services
    .AddAuthentication(ConfigureOptions)
    .AddJwtBearer(BearerOptions);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhap JWT token vao day. Vi du: Bearer eyJhbGciOi..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

// DI
builder.Services.AddScoped<PoiRepository>();
builder.Services.AddScoped<PoiService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<LocalizeService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<LocalizationPipeline>();

// Cors — cho phep moi origin (bao gom mobile emulator)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebAdmin", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Startup tasks: seed missing POI images + ensure QR code exists
{
    using var scope = app.Services.CreateScope();
    var ctx  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var env  = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var http = scope.ServiceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient();

    // 0. Di chuyển POI có tọa độ ngoài Quận 4 về đường Vĩnh Khánh
    await FixOutOfBoundsPoisAsync(ctx);

    // 1. Download & attach images to POIs that have < 4 images
    await ImageAutoSeeder.SeedMissingImagesAsync(ctx, env, http);

    // 2. Ensure at least 1 active QR code exists (mobile dev-bypass requires it)
    if (!ctx.QrCodes.Any(q => q.IsActive))
    {
        ctx.QrCodes.Add(new QrCode
        {
            QrCodeValue = "QR-DEMO-001",
            IsActive    = true,
            CreatedDate = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
        Console.WriteLine("[Startup] Created QR-DEMO-001 for development.");
    }
}

// Tao thu muc audio neu chua co
var webRoot = app.Environment.WebRootPath
    ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "audio"));

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();

// app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowWebAdmin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ── Local helpers ─────────────────────────────────────────────────────────────

static async Task FixOutOfBoundsPoisAsync(Api.Repositories.AppDbContext ctx)
{
    // Ranh giới xấp xỉ của Quận 4, TP.HCM
    const double MinLat = 10.748, MaxLat = 10.775;
    const double MinLon = 106.690, MaxLon = 106.715;

    // Các vị trí dọc đường Vĩnh Khánh để gán lại cho POI lạc
    var fallbackPositions = new (double Lat, double Lon)[]
    {
        (10.7564, 106.7006),
        (10.7577, 106.7029),
        (10.7545, 106.6994),
        (10.7558, 106.7040),
        (10.7570, 106.6980),
    };

    var outOfBounds = ctx.Pois.Where(p =>
        p.Latitude  < MinLat || p.Latitude  > MaxLat ||
        p.Longitude < MinLon || p.Longitude > MaxLon)
        .ToList();

    if (outOfBounds.Count == 0) return;

    for (int i = 0; i < outOfBounds.Count; i++)
    {
        var poi = outOfBounds[i];
        var pos = fallbackPositions[i % fallbackPositions.Length];
        Console.WriteLine(
            $"[Startup] POI #{poi.PoiID} '{poi.PoiName}': " +
            $"({poi.Latitude:F4}, {poi.Longitude:F4}) → ({pos.Lat}, {pos.Lon})");
        poi.Latitude    = pos.Lat;
        poi.Longitude   = pos.Lon;
        poi.UpdatedDate = DateTime.UtcNow;
    }

    await ctx.SaveChangesAsync();
}
