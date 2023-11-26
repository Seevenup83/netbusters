//Models/TeamModel.cs
using System.ComponentModel.DataAnnotations;

namespace netbusters.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Teamname must be between 4 and 30 characters long.")]
        [RegularExpression("^[a-zA-Z0-9 ]+$", ErrorMessage = "Teamname must contain only letters, numbers, and spaces.")]
        public string ?Name { get; set; }

        public int UserId { get; set; }
    }
}