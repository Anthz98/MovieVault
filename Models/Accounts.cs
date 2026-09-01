using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MovieVault.Models
{
    public class Accounts
    {
        public ObjectId Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Fullname { get; set; } = string.Empty;

        // Holds a BCrypt hash once CreateAccount has run, never the raw password.
        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public string DateOfBirth { get; set; } = string.Empty;

        public bool IsLoggedIn { get; set; } = false;
        public int LogInAttempts { get; set; } = 0;
        public string? RefreshToken { get; set; }
        public string? RefreshTokenExpiryTime { get; set; }
    }
}
