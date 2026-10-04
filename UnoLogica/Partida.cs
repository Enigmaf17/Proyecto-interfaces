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
            return Descarte[Descarte.Count - 1];
        }

        // Pone una carta en la pila de descarte y actualiza el color actual
        public void PonerCartaEnMesa(Carta carta, ColorCarta? colorElegido = null)
        {
            Descarte.Add(carta);
            ColorActual = colorElegido ?? carta.Color;
        }

        // ¿Se puede jugar esta carta sobre la de arriba?
        public bool PuedeJugar(Carta carta)
        {
            // Los comodines siempre se pueden jugar
            if (carta.Tipo == TipoCarta.Comodin || carta.Tipo == TipoCarta.ComodinMasCuatro)
                return true;

            // Mismo color que el color actual
            if (carta.Color == ColorActual)
                return true;

            Carta arriba = CartaArriba();

            // Dos cartas de número: deben tener el mismo número
            if (carta.Tipo == TipoCarta.Numero && arriba.Tipo == TipoCarta.Numero)
                return carta.Numero == arriba.Numero;

            // Cartas especiales: mismo tipo (Salta sobre Salta, +2 sobre +2...)
            return carta.Tipo == arriba.Tipo;
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
            int paso = SentidoHorario ? 1 : -1;
            int total = Jugadores.Count;
            TurnoActual = (TurnoActual + paso + total) % total;
        }
    }
}