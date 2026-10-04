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

        // Devuelve el nombre de la imagen, por ejemplo "rojo_5.png" o "comodin_mas4.png"
        public string NombreImagen()
        {
            throw new NotImplementedException();
        }
    }
}