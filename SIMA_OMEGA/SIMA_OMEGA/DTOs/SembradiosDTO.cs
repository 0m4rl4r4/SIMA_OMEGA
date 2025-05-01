namespace SIMA_OMEGA.DTOs
{
    public class SembradiosDTO
    {
        public int Id { get; set; }
        public string TipoPlanta { get; set; }
        public double ExtensionMts2 { get; set; }
        public string FotoSembradio { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
