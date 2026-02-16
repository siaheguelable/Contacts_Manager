using ContactManager.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ContactManager.Utilities;
using System.Threading.Tasks;

namespace ContactManager.Data
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<User> GetByUserNameAsync(string userName) =>
            await _users.Find(u => u.UserName == userName).FirstOrDefaultAsync();

        public async Task CreateAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task<User> ValidateCredentialsAsync(string userOrEmail, string password)
        {
            User user = null;
            if (userOrEmail.Contains("@"))
                user = await GetByEmailAsync(userOrEmail);
            else
                user = await GetByUserNameAsync(userOrEmail);

            if (user == null) return null;

            if (Utilities.PasswordHasher.Verify(password, user.PasswordHash))
                return user;

            return null;
        }
    }
}
