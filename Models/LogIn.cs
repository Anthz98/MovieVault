using System.ComponentModel.DataAnnotations;

namespace MovieVault.Models
{
    public class LogIn
    {
        [Required]
        public string? username { get; set; }
        [Required]
        public string? password { get; set; }
    }
}
