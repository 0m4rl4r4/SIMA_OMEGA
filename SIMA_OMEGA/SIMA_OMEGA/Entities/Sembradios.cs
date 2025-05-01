namespace SIMA_OMEGA.Entities
{
    public class Sembradio
    {
        public int Id { get; set; }
        public string TipoPlanta { get; set; }
        public double ExtensionMts2 { get; set; }
        public string FotoSembradio { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
