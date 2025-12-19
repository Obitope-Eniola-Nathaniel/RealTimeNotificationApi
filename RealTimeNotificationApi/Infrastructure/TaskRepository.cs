using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RealTimeNotificationApi.Infrastructure
{
    // C# model for a task document in MongoDB
    public class TaskItem
    {
        public string? Id { get; set; }           // string ID (we set GUID in code)
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Interface (contract) for our task repository
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(string id);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<bool> UpdateAsync(string id, TaskItem task);
        Task<bool> DeleteAsync(string id);
    }

    // Implementation using MongoDB
    public class MongoTaskRepository : ITaskRepository
    {
        private readonly IMongoCollection<TaskItem> _collection;

        // We inject MongoDbSettings (via IOptions) to get connection info
        public MongoTaskRepository(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;

            // Connect to Mongo using connection string
            var client = new MongoClient(settings.ConnectionString);

            // Get database
            var database = client.GetDatabase(settings.DatabaseName);

            // Get Tasks collection
            _collection = database.GetCollection<TaskItem>(settings.TasksCollectionName);
        }

        public async Task<List<TaskItem>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync(); // {} filter = all

        public async Task<TaskItem?> GetByIdAsync(string id) =>
            await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            await _collection.InsertOneAsync(task);
            return task;
        }

        public async Task<bool> UpdateAsync(string id, TaskItem task)
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == id, task);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
