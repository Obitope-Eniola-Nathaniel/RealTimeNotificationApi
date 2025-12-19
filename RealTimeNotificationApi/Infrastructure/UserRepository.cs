using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RealTimeNotificationApi.Infrastructure
{
    public class User
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; // demo: we could use plain text, but better to hash
    }

    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
    }

    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public MongoUserRepository(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<User>("Users");
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
