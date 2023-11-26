//Models/UserModel.cs
using System.ComponentModel.DataAnnotations;

namespace netbusters.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers.")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 30 characters.")]
        public string ?Username { get; set; }

        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "First name can only contain letters and numbers.")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "First name must be between 4 and 30 characters.")]
        public string FirstName { get; set; }

        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Last name can only contain letters and numbers.")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Last name must be between 8 and 30 characters.")]
        public string LastName { get; set; }

        [StringLength(100, MinimumLength = 4, ErrorMessage = "Email must be between 4 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid email format.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(60, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Password name can only contain letters and numbers.")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
        public string ?Password { get; set; }
    }
}