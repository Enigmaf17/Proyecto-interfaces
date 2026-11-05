using UnoLogica;

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

        public static VistaJugador Crear(Partida partida, int jugadorId, string? cartaRobada = null, string? ultimoMovimiento = null)
        {
            var vista = new VistaJugador
            {
                MiId = jugadorId,
                CartaArriba = partida.CartaArriba().NombreImagen().Replace(".png", ""),
                ColorActual = partida.ColorActual.ToString(),
                TurnoId = partida.JugadorEnTurno.Id,
                SentidoHorario = partida.SentidoHorario,
                CartasEnMazo = partida.Mazo.CantidadCartas,
                CartaRobada = cartaRobada?.Replace(".png", ""),
                UltimoMovimiento = ultimoMovimiento
            };

            var jugadorActual = partida.Jugadores.FirstOrDefault(j => j.Id == jugadorId);
            if (jugadorActual != null)
            {
                vista.MiMano = jugadorActual.Mano
                    .Select(c => c.NombreImagen().Replace(".png", ""))
                    .ToList();
            }

            foreach (var j in partida.Jugadores)
            {
                vista.Jugadores.Add(new JugadorVisible
                {
                    Id = j.Id,
                    Nombre = j.Nombre,
                    Cartas = j.Mano.Count
                });
            }

            return vista;
        }
    }
}