using System.ComponentModel.DataAnnotations;

namespace netbusters.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 15 characters long.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Username must contain only letters.")]
        public string Username { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 30 characters long.")]
        [RegularExpression(".*[A-Z].*", ErrorMessage = "Password must contain at least one uppercase letter.")]
        public string Password { get; set; }
    }
}

