using UnoLogica;

public class PartidaTests
{
    private Partida CrearPartida()
    {
        var jugadores = new List<Jugador>
        {
            new Jugador(1, "Paul"),
            new Jugador(2, "Rosa"),
            new Jugador(3, "Dalton")
        };
        return new Partida(jugadores);
    }

    [Fact]
    public void PasarTurno_AvanzaAlSiguiente()
    {
        var partida = CrearPartida();
        partida.PasarTurno();
        Assert.Equal(1, partida.TurnoActual);
    }

    [Fact]
    public void PasarTurno_DespuesDelUltimoRegresaAlPrimero()
    {
        var partida = CrearPartida();
        partida.PasarTurno();
        partida.PasarTurno();
        partida.PasarTurno();
        Assert.Equal(0, partida.TurnoActual);
    }

    [Fact]
    public void PuedeJugar_MismoColor()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        Assert.True(partida.PuedeJugar(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 8)));
    }

    [Fact]
    public void PuedeJugar_MismoNumero()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        Assert.True(partida.PuedeJugar(new Carta(ColorCarta.Azul, TipoCarta.Numero, 5)));
    }

    [Fact]
    public void PuedeJugar_NoCoincideNada()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        Assert.False(partida.PuedeJugar(new Carta(ColorCarta.Azul, TipoCarta.Numero, 8)));
    }

    [Fact]
    public void PuedeJugar_DespuesDeComodinUsaColorElegido()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Negro, TipoCarta.Comodin), ColorCarta.Verde);
        Assert.True(partida.PuedeJugar(new Carta(ColorCarta.Verde, TipoCarta.Numero, 2)));
        Assert.False(partida.PuedeJugar(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 2)));
    }
}