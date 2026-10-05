using UnoLogica;

public class ControladorPartidaTests
{
    private List<Jugador> CrearJugadores() => new List<Jugador>
    {
        new Jugador(1, "Paul"),
        new Jugador(2, "Rosa"),
        new Jugador(3, "Dalton")
    };

    [Fact]
    public async Task IniciarAsync_CreaLaPartidaYReparte()
    {
        var controlador = new ControladorPartida(new ApiCliente());

        await controlador.IniciarAsync(CrearJugadores());

        Assert.True(controlador.PartidaId > 0);
        foreach (var jugador in controlador.Partida.Jugadores)
            Assert.Equal(7, jugador.Mano.Count);
    }

    [Fact]
    public async Task RobarCartaAsync_AgregaCartaYNoPasaTurno()
    {
        var controlador = new ControladorPartida(new ApiCliente());
        await controlador.IniciarAsync(CrearJugadores());

        await controlador.RobarCartaAsync();

        Assert.Equal(8, controlador.Partida.Jugadores[0].Mano.Count);
        Assert.Equal(0, controlador.Partida.TurnoActual);
    }
}