using UnoLogica;

public class MazoTests
{
    [Fact]
    public void Crear_Genera108Cartas()
    {
        var mazo = new Mazo();
        mazo.Crear();
        Assert.Equal(108, mazo.CantidadCartas);
    }
}