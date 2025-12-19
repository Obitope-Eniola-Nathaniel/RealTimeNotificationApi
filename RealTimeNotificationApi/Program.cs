// Bring in namespaces (libraries) we need.
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using RealTimeNotificationApi.Filters;
using RealTimeNotificationApi.Hubs;
using RealTimeNotificationApi.Infrastructure;
using RealTimeNotificationApi.Security;

var builder = WebApplication.CreateBuilder(args); // Create app builder

// =======================
// 1. CONFIGURE SERVICES
// =======================

// Bind MongoDb section from appsettings.json to MongoDbSettings class
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

// Register repositories so we can inject them via interfaces
builder.Services.AddScoped<ITaskRepository, MongoTaskRepository>();
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<INotificationRepository, MongoNotificationRepository>();

// Add controllers (so [ApiController] classes work)
builder.Services.AddControllers(options =>
{
    // Add global logging filter (runs on every request)
    options.Filters.Add<LogActionFilter>();
});

// Add SignalR for real-time
builder.Services.AddSignalR();

// Register filters and encryption service for DI
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<EncryptResponseFilter>();
builder.Services.AddScoped<EncryptionService>();

// Swagger for API docs & testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt"); // read Jwt section
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);     // convert key string to bytes

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // use Bearer auth
    .AddJwtBearer(options =>
    {
        // How to validate incoming tokens
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };

        // Allow SignalR to pass token via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to read token from "access_token" query string
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // If this is a hub request, use that token
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// Configure CORS (allow any origin for demo)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true)); // allow all origins
});

var app = builder.Build(); // Build the app

// =======================
// 2. CONFIGURE PIPELINE
// =======================

if (app.Environment.IsDevelopment())
{
    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors();

// IMPORTANT: authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers (REST API routes)
app.MapControllers();

// Map SignalR hub at /hubs/notifications
app.MapHub<NotificationHub>("/hubs/notifications");

// Serve index.html and static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Run the app
app.Run();
