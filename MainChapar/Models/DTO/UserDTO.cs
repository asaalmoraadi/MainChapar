using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models.DTOs
{
    public class UserDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره تلفن باید 11 رقمی و با 09 شروع شود.")]
        public string PhoneNumber { get; set; }
        [Required] public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfPassword { get; set; }
    }
}
