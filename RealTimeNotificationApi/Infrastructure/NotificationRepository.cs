using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RealTimeNotificationApi.Infrastructure
{
    // Notification document model
    public class Notification
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;   // which user this belongs to
        public string Message { get; set; } = null!;  // text of notification
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Delivered { get; set; } = false;  // has it been sent to user?
    }

    // Notification repository contract
    public interface INotificationRepository
    {
        Task CreateAsync(Notification notification);
        Task<List<Notification>> GetUndeliveredForUserAsync(string userId);
        Task MarkAsDeliveredAsync(IEnumerable<string> ids);
    }

    // Mongo implementation
    public class MongoNotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _collection;

        public MongoNotificationRepository(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<Notification>("Notifications");
        }

        public async Task CreateAsync(Notification notification)
        {
            await _collection.InsertOneAsync(notification);
        }

        public async Task<List<Notification>> GetUndeliveredForUserAsync(string userId)
        {
            return await _collection
                .Find(n => n.UserId == userId && !n.Delivered)
                .SortBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsDeliveredAsync(IEnumerable<string> ids)
        {
            var filter = Builders<Notification>.Filter.In(n => n.Id, ids);
            var update = Builders<Notification>.Update.Set(n => n.Delivered, true);
            await _collection.UpdateManyAsync(filter, update);
        }
    }
}
