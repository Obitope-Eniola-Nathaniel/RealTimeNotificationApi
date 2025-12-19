using RealTimeNotificationApi.Filters;
using RealTimeNotificationApi.Hubs;
using RealTimeNotificationApi.Infrastructure;
using RealTimeNotificationApi.Security;

var builder = WebApplication.CreateBuilder(args);

// ---- CONFIGURE SERVICES (like app.use(...) & DI registrations) ----

builder.Services.AddScoped<IUserRepository, MongoUserRepository>();


// Bind MongoDB settings from appsettings.json to MongoDbSettings class
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

// Register our task repository so controllers can ask for ITaskRepository
builder.Services.AddScoped<ITaskRepository, MongoTaskRepository>();

// Add controller support (attribute routing, model binding, etc.)
builder.Services.AddControllers(options =>
{
    // Register a global logging filter that runs on every request
    options.Filters.Add<LogActionFilter>();
});

// Add SignalR for real-time communication (websockets etc.)
builder.Services.AddSignalR();

// Register filters and encryption service so they can be injected
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<EncryptResponseFilter>();
builder.Services.AddScoped<EncryptionService>();

// Swagger for API documentation & testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Very open CORS policy for demo (allow any origin/method/header)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

// ---- CONFIGURE MIDDLEWARE PIPELINE (like app.use(...) order) ----

if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI at /swagger in Development
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Force HTTPS when possible
app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors();

// (Later we'll add app.UseAuthentication(); before UseAuthorization())
app.UseAuthorization();

// Map attribute-routed controllers (e.g. TasksController)
app.MapControllers();

// Map SignalR hub at /hubs/notifications
app.MapHub<NotificationHub>("/hubs/notifications");

// Serve index.html and other static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Start the web app
app.Run();
