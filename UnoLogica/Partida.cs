namespace UnoLogica
{
    public class Partida
    {
        public List<Jugador> Jugadores { get; }
        public Mazo Mazo { get; } = new Mazo();
        public List<Carta> Descarte { get; } = new List<Carta>();
        public int TurnoActual { get; private set; }            // índice del jugador en turno
        public bool SentidoHorario { get; private set; } = true;
        public ColorCarta ColorActual { get; private set; }
        public Jugador? Ganador { get; private set; }

        public Jugador JugadorEnTurno => Jugadores[TurnoActual];

        public Partida(List<Jugador> jugadores)
        {
            Jugadores = jugadores;
        }

        // Crea y baraja el mazo, reparte 7 cartas a cada jugador y voltea la primera carta
        public void Iniciar()
        {
            throw new NotImplementedException();
        }

        // Devuelve la carta de arriba de la pila de descarte
        public Carta CartaArriba()
        {
            throw new NotImplementedException();
        }

        // ¿Se puede jugar esta carta sobre la de arriba?
        public bool PuedeJugar(Carta carta)
        {
            throw new NotImplementedException();
        }

        // Juega una carta y aplica su efecto. colorElegido solo se usa en los comodines
        public void JugarCarta(Carta carta, ColorCarta? colorElegido = null)
        {
            throw new NotImplementedException();
        }

        // El jugador en turno roba una carta del mazo
        public Carta RobarCarta()
        {
            throw new NotImplementedException();
        }

        // El jugador en turno dice "UNO"
        public void DecirUno()
        {
            throw new NotImplementedException();
        }

        // Pasa al siguiente jugador según el sentido del juego
        public void PasarTurno()
        {
            throw new NotImplementedException();
        }
    }
}