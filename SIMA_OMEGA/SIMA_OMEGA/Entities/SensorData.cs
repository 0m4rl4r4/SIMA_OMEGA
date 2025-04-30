namespace SIMA_OMEGA.Entities
{
    public class SensorData
    {
        public int Id { get; set; }
        public float Temperatura { get; set; }
        public float Humedad { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
