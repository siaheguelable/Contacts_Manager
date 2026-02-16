namespace ContactManager.Data
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string ContactsCollectionName { get; set; } = null!;
    }
}
