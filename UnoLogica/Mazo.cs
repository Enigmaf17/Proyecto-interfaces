namespace UnoLogica
{
    public class Mazo
    {
        private List<Carta> cartas = new List<Carta>();
        private static readonly Random aleatorio = new Random();

        public int CantidadCartas => cartas.Count;

        public void Crear()
        {
            cartas.Clear(); // por si se llama dos veces, que no se dupliquen las cartas

            ColorCarta[] colores = { ColorCarta.Rojo, ColorCarta.Amarillo, ColorCarta.Verde, ColorCarta.Azul };

            foreach (ColorCarta color in colores)
            {
                // Un solo 0 por color
                cartas.Add(new Carta(color, TipoCarta.Numero, 0));

                // Dos cartas de cada número del 1 al 9
                for (int numero = 1; numero <= 9; numero++)
                {
                    cartas.Add(new Carta(color, TipoCarta.Numero, numero));
                    cartas.Add(new Carta(color, TipoCarta.Numero, numero));
                }

                // Dos Salta, dos Reversa y dos MasDos
                for (int i = 0; i < 2; i++)
                {
                    cartas.Add(new Carta(color, TipoCarta.Salta));
                    cartas.Add(new Carta(color, TipoCarta.Reversa));
                    cartas.Add(new Carta(color, TipoCarta.MasDos));
                }
            }

            // 4 Comodín y 4 Comodín +4 (color Negro)
            for (int i = 0; i < 4; i++)
            {
                cartas.Add(new Carta(ColorCarta.Negro, TipoCarta.Comodin));
                cartas.Add(new Carta(ColorCarta.Negro, TipoCarta.ComodinMasCuatro));
            }
        }

        public void Barajar()
        {
            // Fisher-Yates: de la última carta hacia la segunda
            for (int i = cartas.Count - 1; i > 0; i--)
            {
                int j = aleatorio.Next(i + 1); // número al azar entre 0 e i

                // Intercambiar la carta en i con la carta en j
                Carta temporal = cartas[i];
                cartas[i] = cartas[j];
                cartas[j] = temporal;
            }
        }

        public Carta Robar()
        {
            if (cartas.Count == 0)
            {
                throw new InvalidOperationException("El mazo está vacío.");
            }

            // La carta de arriba es la última de la lista
            Carta carta = cartas[cartas.Count - 1];
            cartas.RemoveAt(cartas.Count - 1);
            return carta;
        }

        public void Rellenar(List<Carta> descarte)
        {
            // Si hay 0 o 1 carta en el descarte, no hay nada que pasar al mazo
            if (descarte.Count <= 1)
            {
                return;
            }

            // Todas menos la última (la de arriba, que se queda en la mesa)
            int cantidad = descarte.Count - 1;
            List<Carta> paraElMazo = descarte.GetRange(0, cantidad);

            // QUITARLAS del descarte para que no queden repetidas
            descarte.RemoveRange(0, cantidad);

            // Pasarlas al mazo y barajar
            cartas.AddRange(paraElMazo);
            Barajar();
        }
    }
}