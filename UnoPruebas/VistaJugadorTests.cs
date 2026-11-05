using Xunit;
using UnoLogica;
using UnoRed;

namespace UnoPruebas
{
    public class VistaJugadorTests
    {
        [Fact]
        public void VistaJugador_DespuesDeIniciar_ContieneDatosCorrectos()
        {
            var jugadores = new List<Jugador>
            {
                new Jugador(1, "Paul"),
                new Jugador(2, "Rosa"),
                new Jugador(3, "Dalton")
            };

            var partida = new Partida(jugadores);
            partida.Iniciar(); 
            
            var vista = VistaJugador.Crear(partida, 1);

            Assert.Equal(1, vista.MiId);
            Assert.Equal(7, vista.MiMano.Count); 
            Assert.Equal(3, vista.Jugadores.Count); 
            Assert.Equal(partida.CartaArriba().NombreImagen().Replace(".png", ""), vista.CartaArriba);
            Assert.Equal(partida.ColorActual.ToString(), vista.ColorActual);
            Assert.Equal(partida.JugadorEnTurno.Id, vista.TurnoId);
        }
    }
}