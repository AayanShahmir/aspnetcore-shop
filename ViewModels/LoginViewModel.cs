using System.ComponentModel.DataAnnotations;

namespace BIsm2.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Required.")]
        //EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? UserIdentifier { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
