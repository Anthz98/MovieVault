using System.ComponentModel.DataAnnotations;

namespace EntityFramework.Models
{
    public class LogIn
    {
        [Required]
        public string? username { get; set; }
        [Required]
        public string? password { get; set; }
    }
}
