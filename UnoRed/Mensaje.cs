using UnoLogica;

namespace UnoRed
{
    public class Mensaje
    {
        public string Tipo { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public int? Indice { get; set; }
        public string? Color { get; set; }
        public int? JugadorId { get; set; }
        public List<string>? Conectados { get; set; }
        public VistaJugador? Vista { get; set; }
        public string? Texto { get; set; }
        public string? Ganador { get; set; }
        public List<EstadisticasDto>? Estadisticas { get; set; }
    }
}