using UnoLogica;

public class ApiClienteTests
{
    [Fact]
    public async Task ObtenerJugadores_RegresaLosTres()
    {
        var api = new ApiCliente();
        var jugadores = await api.ObtenerJugadoresAsync();
        Assert.Equal(3, jugadores.Count);
    }
}