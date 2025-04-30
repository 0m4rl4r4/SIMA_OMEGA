using System.ComponentModel.DataAnnotations;

namespace SIMA_OMEGA.DTOs
{
    public class EventoDTO
    {
        [Required]
        public int Id { get; set; }

        public string EventoNombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    }
    }



