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
            Mazo.Crear();
            Mazo.Barajar();

            Descarte.Clear();
            TurnoActual = 0;
            SentidoHorario = true;
            Ganador = null;

            foreach (Jugador jugador in Jugadores)
            {
                jugador.Mano.Clear();
                jugador.DijoUno = false;
            }

            // Repartir de una en una, como en la vida real
            for (int i = 0; i < 7; i++)
            {
                foreach (Jugador jugador in Jugadores)
                    jugador.RecibirCarta(Mazo.Robar());
            }

            // La primera carta de la mesa siempre es de número.
            // Si sale una especial, se queda abajo en la pila y se voltea otra.
            Carta primera = Mazo.Robar();
            while (primera.Tipo != TipoCarta.Numero)
            {
                Descarte.Add(primera);
                primera = Mazo.Robar();
            }
            PonerCartaEnMesa(primera);
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
            Jugador jugador = JugadorEnTurno;
            bool esComodin = carta.Tipo == TipoCarta.Comodin || carta.Tipo == TipoCarta.ComodinMasCuatro;

            // 1. Validaciones
            if (Ganador != null)
                throw new InvalidOperationException("La partida ya terminó.");
            if (!jugador.Mano.Contains(carta))
                throw new InvalidOperationException("El jugador no tiene esa carta.");
            if (!PuedeJugar(carta))
                throw new InvalidOperationException("Esa carta no se puede jugar.");
            if (esComodin && (colorElegido == null || colorElegido == ColorCarta.Negro))
                throw new InvalidOperationException("Hay que elegir un color para el comodín.");

            // 2. Mover la carta de la mano a la mesa
            jugador.Mano.Remove(carta);
            PonerCartaEnMesa(carta, esComodin ? colorElegido : null);

            // 3. ¿Ganó?
            if (jugador.Mano.Count == 0)
            {
                Ganador = jugador;
                return;
            }

            // 4. Castigo por no decir UNO: si le queda 1 carta y no lo dijo, roba 2
            if (jugador.Mano.Count == 1 && !jugador.DijoUno)
                RobarPara(jugador, 2);
            jugador.DijoUno = false;

            // 5. Efecto de la carta
            switch (carta.Tipo)
            {
                case TipoCarta.Salta:
                    PasarTurno(); // brinca al siguiente jugador
                    break;

                case TipoCarta.Reversa:
                    SentidoHorario = !SentidoHorario;
                    if (Jugadores.Count == 2)
                        PasarTurno(); // con 2 jugadores, la reversa funciona como salta
                    break;

                case TipoCarta.MasDos:
                    PasarTurno();
                    RobarPara(JugadorEnTurno, 2); // el siguiente roba 2 y pierde su turno
                    break;

                case TipoCarta.ComodinMasCuatro:
                    PasarTurno();
                    RobarPara(JugadorEnTurno, 4); // el siguiente roba 4 y pierde su turno
                    break;
            }

            // 6. Turno del siguiente jugador
            PasarTurno();
        }

        // El jugador en turno roba una carta del mazo
        public Carta RobarCarta()
        {
            throw new NotImplementedException();
        }

        // El jugador indicado roba una cantidad de cartas del mazo
        private void RobarPara(Jugador jugador, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                if (Mazo.CantidadCartas == 0)
                    Mazo.Rellenar(Descarte);

                jugador.Mano.Add(Mazo.Robar());
            }
        }

        // El jugador en turno dice "UNO" (antes de tirar su penúltima carta)
        public void DecirUno()
        {
            JugadorEnTurno.DijoUno = true;
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