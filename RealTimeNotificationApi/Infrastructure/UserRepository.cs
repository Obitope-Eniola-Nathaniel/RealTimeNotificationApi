using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RealTimeNotificationApi.Infrastructure
{
    // User document model
    public class User
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; // DEMO: plain text or simple hash
    }

    // User repository contract
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
    }

    // Mongo implementation
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public MongoUserRepository(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<User>("Users"); // collection name
        }

        public async Task<User?> GetByEmailAsync(string email) =>
            await _collection.Find(x => x.Email == email).FirstOrDefaultAsync();

        public async Task<User> CreateAsync(User user)
        {
            await _collection.InsertOneAsync(user);
            return user;
        }
    }
}
