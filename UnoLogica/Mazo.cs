namespace UnoLogica
{
    public class Mazo
    {
        private List<Carta> cartas = new List<Carta>();
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

        public void Barajar() { throw new NotImplementedException(); }

        public Carta Robar() { throw new NotImplementedException(); }

        public void Rellenar(List<Carta> descarte) { throw new NotImplementedException(); }
    }
}