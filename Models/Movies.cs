using MongoDB.Bson;

namespace MovieVault.Models
{
    public class Movies
    {
        public ObjectId Id { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public double Rating { get; set; } = 5.0;
    }
}
