namespace SIMA_OMEGA.DTOs
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NuevaPassword { get; set; }
    }

}
