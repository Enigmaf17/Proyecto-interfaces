namespace UnoRed
{
    public class VistaJugador
    {
        public int MiId { get; set; }
        public List<string> MiMano { get; set; } = new List<string>();
        public List<JugadorVisible> Jugadores { get; set; } = new List<JugadorVisible>();
        public string CartaArriba { get; set; } = string.Empty;
        public string ColorActual { get; set; } = string.Empty;
        public int TurnoId { get; set; }
        public bool SentidoHorario { get; set; }
        public int CartasEnMazo { get; set; }
        public string? CartaRobada { get; set; }
        public string? UltimoMovimiento { get; set; }
    }
}