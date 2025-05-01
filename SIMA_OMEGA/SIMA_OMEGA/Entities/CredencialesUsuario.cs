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

        
    }
}
