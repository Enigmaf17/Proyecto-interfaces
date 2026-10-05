namespace UnoLogica
{
    public enum ColorCarta { Rojo, Amarillo, Verde, Azul, Negro } // Negro = comodines
    public enum TipoCarta { Numero, Salta, Reversa, MasDos, Comodin, ComodinMasCuatro }

    public class Carta
    {
        public ColorCarta Color { get; }
        public TipoCarta Tipo { get; }
        public int Numero { get; } // 0 a 9 si es carta de número; -1 en las demás

        public Carta(ColorCarta color, TipoCarta tipo, int numero = -1)
        {
            Color = color;
            Tipo = tipo;
            Numero = numero;
        }

        public string NombreImagen()
        {
            switch (Tipo)
            {
                case TipoCarta.Comodin:
                    return "comodin.png";

                case TipoCarta.ComodinMasCuatro:
                    return "comodin_mas4.png";

                case TipoCarta.Numero:
                    return $"{NombreColor()}_{Numero}.png";   // ej: rojo_5.png

                case TipoCarta.Salta:
                    return $"{NombreColor()}_salta.png";      // ej: verde_salta.png

                case TipoCarta.Reversa:
                    return $"{NombreColor()}_reversa.png";    // ej: azul_reversa.png

                case TipoCarta.MasDos:
                    return $"{NombreColor()}_mas2.png";       // ej: amarillo_mas2.png

                default:
                    throw new InvalidOperationException("Tipo de carta desconocido.");
            }
        }

        // Método privado auxiliar: convierte el color a minúsculas ("Rojo" -> "rojo")
        private string NombreColor()
        {
            return Color.ToString().ToLowerInvariant();
        }
    }
}