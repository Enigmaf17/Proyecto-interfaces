namespace UnoLogica
{
    // Une las reglas (Partida) con el registro en la base de datos (ApiCliente).
    // La interfaz solo necesita usar esta clase: cada acción aplica la regla
    // y guarda el movimiento en el log automáticamente.
    public class ControladorPartida
    {
        private readonly ApiCliente api;
        private int numeroJugada = 0;

        public Partida Partida { get; private set; } = null!;
        public int PartidaId { get; private set; }

        public ControladorPartida(ApiCliente api)
        {
            this.api = api;
        }

        // Crea la partida en la base de datos, reparte y voltea la primera carta
        public async Task IniciarAsync(List<Jugador> jugadores)
        {
            PartidaId = await api.CrearPartidaAsync(jugadores.Select(j => j.Id).ToList());

            Partida = new Partida(jugadores);
            Partida.Iniciar();
            numeroJugada = 0;
        }

        // Juega una carta. Si la jugada no es válida, Partida lanza error y no se registra nada.
        public async Task JugarCartaAsync(Carta carta, ColorCarta? colorElegido = null)
        {
            Jugador jugador = Partida.JugadorEnTurno;
            int cartasAntes = jugador.Mano.Count;
            bool dijoUno = jugador.DijoUno;

            Partida.JugarCarta(carta, colorElegido);
            await Registrar(jugador, "jugar", carta, colorElegido);

            // Si tenía 2 cartas y no dijo UNO, Partida ya le dio 2 de castigo
            if (cartasAntes == 2 && !dijoUno)
                await Registrar(jugador, "castigo_uno");

            if (Partida.Ganador != null)
                await api.FinalizarPartidaAsync(PartidaId, Partida.Ganador.Id);
        }

        // El jugador en turno roba una carta (no pasa el turno)
        public async Task<Carta> RobarCartaAsync()
        {
            Jugador jugador = Partida.JugadorEnTurno;
            Carta carta = Partida.RobarCarta();
            await Registrar(jugador, "robar", carta);
            return carta;
        }

        // El jugador en turno dice UNO (antes de tirar su penúltima carta)
        public async Task DecirUnoAsync()
        {
            Jugador jugador = Partida.JugadorEnTurno;
            Partida.DecirUno();
            await Registrar(jugador, "uno");
        }

        // El jugador en turno pasa (por ejemplo, después de robar una carta que no puede jugar)
        public async Task PasarTurnoAsync()
        {
            Jugador jugador = Partida.JugadorEnTurno;
            Partida.PasarTurno();
            await Registrar(jugador, "pasar");
        }

        // Guarda un movimiento en el log de la base de datos
        private async Task Registrar(Jugador jugador, string accion,
            Carta? carta = null, ColorCarta? color = null)
        {
            numeroJugada++;
            string? nombreCarta = carta?.NombreImagen().Replace(".png", "");   // "rojo_5"
            await api.RegistrarMovimientoAsync(PartidaId, jugador.Id, numeroJugada,
                accion, nombreCarta, color?.ToString());
        }
    }
}