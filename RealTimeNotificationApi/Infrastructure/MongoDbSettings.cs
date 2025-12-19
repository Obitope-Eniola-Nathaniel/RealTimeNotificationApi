namespace RealTimeNotificationApi.Infrastructure
{
    // This matches the "MongoDb" section in appsettings.json
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string TasksCollectionName { get; set; } = null!;
    }
}
