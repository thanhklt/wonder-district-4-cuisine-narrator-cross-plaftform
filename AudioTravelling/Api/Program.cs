using Api.Infrastructure.Services;
using Api.Modules.Admin.Interfaces;
using Api.Modules.Admin.Services;
using Api.Modules.Audio.Interfaces;
using Api.Modules.Audio.Services;
using Api.Modules.Auth.Interfaces;
using Api.Modules.Auth.Services;
using Api.Modules.Localization.Interfaces;
using Api.Modules.Localization.Services;
using Api.Modules.Owner.Interfaces;
using Api.Modules.Owner.Services;
using Api.Modules.Poi.Interfaces;
using Api.Modules.Poi.Services;
using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();

// Thêm PoiService
builder.Services.AddScoped<IPoiService, PoiService>();

// Theme OwnerService
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IOwnerPoiService, OwnerPoiService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<AudioFileService>();
builder.Services.AddScoped<ITtsProvider, FakeTtsProvider>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Them AdminPoiService
builder.Services.AddScoped<IAdminPoiService, AdminPoiService>();

// Deep translate client configuration  
builder.Services.AddHttpClient<IDeepTranslateClient, DeepTranslateClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Localization services
builder.Services.AddScoped<ILocalizationService, LocalizationService>();


// Add CORS - Allow requests from Mobile apps
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhap token theo dang: Bearer {your token}"
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
builder.Services.AddEndpointsApiExplorer();

// 
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();