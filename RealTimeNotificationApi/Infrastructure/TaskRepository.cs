using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RealTimeNotificationApi.Infrastructure
{
    public class TaskItem
    {
        // Id is optional in the request (nullable string)
        public string? Id { get; set; }

        // These can stay required / non-nullable
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(string id);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<bool> UpdateAsync(string id, TaskItem task);
        Task<bool> DeleteAsync(string id);
    }

    public class MongoTaskRepository : ITaskRepository
    {
        private readonly IMongoCollection<TaskItem> _collection;

        public MongoTaskRepository(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<TaskItem>(settings.TasksCollectionName);
        }

        public async Task<List<TaskItem>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

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
