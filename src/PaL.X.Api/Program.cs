using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaL.X.Api.Services;
using PaL.X.API.Services;
using PaL.X.Data;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter le ServiceManager comme singleton
builder.Services.AddSingleton<ServiceManager>();

// Ajouter le service de nettoyage des messages
builder.Services.AddHostedService<MessageCleanupService>();

// Ajouter le service de géolocalisation
builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>();

// Ajouter SignalR
builder.Services.AddSignalR();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireClaim("isAdmin", "true"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
var allowedOrigins = builder.Environment.IsDevelopment() 
    ? new[] { "http://localhost:5000", "https://localhost:7109" }
    : new[] { "https://votre-domaine.com" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        builder =>
        {
            builder.WithOrigins(allowedOrigins)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowedOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PaL.X.Api.Hubs.PaLHub>("/hubs/pal");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Appliquer les migrations automatiquement
    dbContext.Database.Migrate();
    
    // Nettoyer les sessions fantômes (sessions actives des exécutions précédentes)
    var orphanedSessions = await dbContext.Sessions
        .Where(s => s.IsActive)
        .ToListAsync();
    
    if (orphanedSessions.Any())
    {
        foreach (var session in orphanedSessions)
        {
            session.IsActive = false;
            session.DisconnectedAt = DateTime.UtcNow;
            session.DisplayedStatus = PaL.X.Shared.Enums.UserStatus.Offline;
            session.RealStatus = PaL.X.Shared.Enums.UserStatus.Offline;
        }
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"Nettoyage: {orphanedSessions.Count} sessions fantômes désactivées.");
    }
    
    // Initialiser les données de test (Admin et User)
    // await SeedData.Initialize(dbContext);
}

app.Run();
