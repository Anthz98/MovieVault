using System.ComponentModel.DataAnnotations;

namespace MovieVault.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
