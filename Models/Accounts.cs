using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework.Models
{
    public class Accounts
    {
        public ObjectId Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Password { get; set; }
        [DataType(DataType.Date)]
        public string DateOfBirth { get; set; }
        public bool IsLoggedIn { get; set; } = true;
        public int LogInAttempts { get; set; } = 0;
        public string? RefreshToken { get; set; }
        public string? RefreshTokenExpiryTime { get; set; }

    }
}
