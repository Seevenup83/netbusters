//Models/ClubModel.cs
using System.ComponentModel.DataAnnotations;

namespace netbusters.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Club name must be between 4 and 50 characters long.")]
        [RegularExpression("^[a-zA-Z0-9 ]+$", ErrorMessage = "Club name must contain only letters, numbers, and spaces.")]
        public string Name { get; set; }

        [StringLength(2083, ErrorMessage = "HTTP link must be under 2083 characters.")]
        [RegularExpression(@"^(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-.,@?^=%&:/~+#]*[\w\-@?^=%&:/~+#])?$", ErrorMessage = "Invalid HTTP link format.")]
        public string HttpLink { get; set; }

        public int UserId { get; set; } // Foreign key to User
    }
}
