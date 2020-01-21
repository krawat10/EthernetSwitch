using System.ComponentModel.DataAnnotations;

namespace EthernetSwitch.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(255, ErrorMessage = "Username be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Text)]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        public string Message { get; set; } = "";
    }
}