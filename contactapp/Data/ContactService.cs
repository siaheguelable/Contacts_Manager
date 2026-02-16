using ContactManager.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactManager.Data
{
    public class ContactService
    {
        private readonly IMongoCollection<Contact> _contacts;

        public ContactService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _contacts = database.GetCollection<Contact>("Contacts");
        }

        public async Task<List<Contact>> GetAsync() =>
            await _contacts.Find(contact => true).ToListAsync();

        public async Task<Contact> GetByIdAsync(string id) =>
            await _contacts.Find(contact => contact.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Contact contact) =>
            await _contacts.InsertOneAsync(contact);

        public async Task UpdateAsync(string id, Contact contact) =>
            await _contacts.ReplaceOneAsync(c => c.Id == id, contact);

        public async Task DeleteAsync(string id) =>
            await _contacts.DeleteOneAsync(c => c.Id == id);
    }
}
