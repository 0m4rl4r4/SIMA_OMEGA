using System.ComponentModel.DataAnnotations;

namespace SIMA_OMEGA.DTOs
{
    public class SensorDataDTO
    {
        [Required]
        public int Id { get; set; }
        public float Temperatura { get; set; }
        public float Humedad { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
