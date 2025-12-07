using RealTimeNotificationApi.Filters;
using RealTimeNotificationApi.Hubs;
using RealTimeNotificationApi.Infrastructure;
using RealTimeNotificationApi.Security;

var builder = WebApplication.CreateBuilder(args);

// MongoDB configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddScoped<ITaskRepository, MongoTaskRepository>();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Global logging filter
    options.Filters.Add<LogActionFilter>();
});

builder.Services.AddSignalR();

builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<EncryptResponseFilter>();
builder.Services.AddScoped<EncryptionService>();

// Swagger (simple setup, no OpenApiInfo needed)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: allow any origin for demo purposes
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// Serve static files (for index.html SignalR client)
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
