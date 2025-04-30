using System.ComponentModel.DataAnnotations;

namespace SIMA_OMEGA.Entities
{
    public class CredencialesUsuario
    {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public IFormFile? ProfileImage { get; set; }
        
    }
}
