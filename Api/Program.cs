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
builder.Services.AddSwaggerGen();

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

// Seed DB neu chua co du lieu
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher  = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    DbSeeder.Seed(context, hasher);
}
catch (Exception ex)
{
    Console.WriteLine($"[DbSeeder] Bỏ qua seed do lỗi kết nối DB: {ex.Message}");
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