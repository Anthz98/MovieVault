namespace EntityFramework.Configuration
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string MoviesCollection { get; set; }
        public string AccountsCollection { get; set; }

    }
}
