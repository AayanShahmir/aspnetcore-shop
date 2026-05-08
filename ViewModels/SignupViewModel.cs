using System.ComponentModel.DataAnnotations;

namespace BIsm2.ViewModels
{
    public class SignupViewModel
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords donot match")]
        public string? ConfirmPassword { get; set; }
    }
}
