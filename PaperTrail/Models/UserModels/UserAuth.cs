namespace PaperTrail.Models.UserModels
{
    namespace PaperTrail.Models.UserModels
    {
        using System.ComponentModel.DataAnnotations;

        public class AuthModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters long.")]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }
    }
}
