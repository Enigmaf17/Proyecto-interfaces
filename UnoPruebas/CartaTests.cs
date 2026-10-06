using System.Collections.Generic;
using System.Linq;
using UnoLogica;
using Xunit;

namespace UnoPruebas
{
    public class CartaTests
    {
        // Método auxiliar: crea un mazo y saca las 108 cartas para poder contarlas
        private List<Carta> SacarTodasLasCartas()
        {
            Mazo mazo = new Mazo();
            mazo.Crear();

            List<Carta> todas = new List<Carta>();
            while (mazo.CantidadCartas > 0)
            {
                todas.Add(mazo.Robar());
            }
            return todas;
        }

        [Fact]
        public void Crear_GeneraLas108Cartas()
        {
            Mazo mazo = new Mazo();
            mazo.Crear();

            Assert.Equal(108, mazo.CantidadCartas);
        }

        [Fact]
        public void Crear_TieneCuatroComodines()
        {
            List<Carta> todas = SacarTodasLasCartas();

            int comodines = todas.Count(c => c.Tipo == TipoCarta.Comodin);

            Assert.Equal(4, comodines);
        }

        [Fact]
        public void Crear_TieneCuatroMasCuatro()
        {
            List<Carta> todas = SacarTodasLasCartas();

            int masCuatro = todas.Count(c => c.Tipo == TipoCarta.ComodinMasCuatro);

            Assert.Equal(4, masCuatro);
        }

        [Theory]
        [InlineData(ColorCarta.Rojo, TipoCarta.Numero, 5, "rojo_5.png")]
        [InlineData(ColorCarta.Amarillo, TipoCarta.Numero, 0, "amarillo_0.png")]
        [InlineData(ColorCarta.Verde, TipoCarta.Salta, -1, "verde_salta.png")]
        [InlineData(ColorCarta.Azul, TipoCarta.Reversa, -1, "azul_reversa.png")]
        [InlineData(ColorCarta.Rojo, TipoCarta.MasDos, -1, "rojo_mas2.png")]
        [InlineData(ColorCarta.Negro, TipoCarta.Comodin, -1, "comodin.png")]
        [InlineData(ColorCarta.Negro, TipoCarta.ComodinMasCuatro, -1, "comodin_mas4.png")]
        public void NombreImagen_DevuelveElNombreCorrecto(ColorCarta color, TipoCarta tipo, int numero, string esperado)
        {
            Carta carta = new Carta(color, tipo, numero);

            Assert.Equal(esperado, carta.NombreImagen());
        }

        [Fact]
        public void Robar_BajaLaCantidadEnUno()
        {
            Mazo mazo = new Mazo();
            mazo.Crear();
            int antes = mazo.CantidadCartas;

            mazo.Robar();

            Assert.Equal(antes - 1, mazo.CantidadCartas);
        }

        // Prueba extra: la parte "muy importante" de Rellenar
        [Fact]
        public void Rellenar_QuitaDelDescarteTodasMenosLaUltima()
        {
            Mazo mazo = new Mazo();
            List<Carta> descarte = new List<Carta>
            {
                new Carta(ColorCarta.Rojo, TipoCarta.Numero, 1),
                new Carta(ColorCarta.Verde, TipoCarta.Numero, 2),
                new Carta(ColorCarta.Azul, TipoCarta.Numero, 3)
            };
            Carta ultima = descarte[2];

            mazo.Rellenar(descarte);

            Assert.Single(descarte);                  // solo queda 1 en el descarte
            Assert.Same(ultima, descarte[0]);         // y es la que estaba arriba
            Assert.Equal(2, mazo.CantidadCartas);     // las otras 2 pasaron al mazo
        }
    }
}