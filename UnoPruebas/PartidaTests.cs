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

    private void DarCartas(Jugador jugador, params Carta[] cartas)
    {
        jugador.Mano.AddRange(cartas);
    }

    [Fact]
    public void JugarCarta_NumeroNormal_PasaAlSiguiente()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var carta = new Carta(ColorCarta.Rojo, TipoCarta.Numero, 8);
        DarCartas(partida.Jugadores[0], carta,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1),
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 2));

        partida.JugarCarta(carta);

        Assert.Equal(1, partida.TurnoActual);
        Assert.Equal(2, partida.Jugadores[0].Mano.Count);
        Assert.Equal(carta, partida.CartaArriba());
    }

    [Fact]
    public void JugarCarta_Salta_BrincaAlSiguiente()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var salta = new Carta(ColorCarta.Rojo, TipoCarta.Salta);
        DarCartas(partida.Jugadores[0], salta,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1),
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 2));

        partida.JugarCarta(salta);

        Assert.Equal(2, partida.TurnoActual);
    }

    [Fact]
    public void JugarCarta_Reversa_CambiaSentido()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var reversa = new Carta(ColorCarta.Rojo, TipoCarta.Reversa);
        DarCartas(partida.Jugadores[0], reversa,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1),
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 2));

        partida.JugarCarta(reversa);

        Assert.False(partida.SentidoHorario);
        Assert.Equal(2, partida.TurnoActual); // del jugador 0 regresa al 2
    }

    [Fact]
    public void JugarCarta_Comodin_CambiaElColor()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var comodin = new Carta(ColorCarta.Negro, TipoCarta.Comodin);
        DarCartas(partida.Jugadores[0], comodin,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1),
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 2));

        partida.JugarCarta(comodin, ColorCarta.Verde);

        Assert.Equal(ColorCarta.Verde, partida.ColorActual);
    }

    [Fact]
    public void JugarCarta_UltimaCarta_Gana()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var ultima = new Carta(ColorCarta.Rojo, TipoCarta.Numero, 3);
        DarCartas(partida.Jugadores[0], ultima);

        partida.JugarCarta(ultima);

        Assert.Equal(partida.Jugadores[0], partida.Ganador);
    }

    [Fact]
    public void JugarCarta_NoValida_LanzaError()
    {
        var partida = CrearPartida();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var mala = new Carta(ColorCarta.Azul, TipoCarta.Numero, 8);
        DarCartas(partida.Jugadores[0], mala,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1));

        Assert.Throws<InvalidOperationException>(() => partida.JugarCarta(mala));
    }

    [Fact]
    public void Iniciar_Reparte7ACadaJugadorYPoneUnaCartaDeNumero()
    {
        var partida = CrearPartida();
        partida.Iniciar();

        foreach (var jugador in partida.Jugadores)
            Assert.Equal(7, jugador.Mano.Count);

        Assert.Equal(TipoCarta.Numero, partida.CartaArriba().Tipo);

        // Ninguna carta se pierde: mazo + manos + descarte = 108
        int total = partida.Mazo.CantidadCartas + 21 + partida.Descarte.Count;
        Assert.Equal(108, total);
    }

    [Fact]
    public void RobarCarta_AgregaUnaCartaALaMano()
    {
        var partida = CrearPartida();
        partida.Iniciar();

        var robada = partida.RobarCarta();

        Assert.Equal(8, partida.Jugadores[0].Mano.Count);
        Assert.Contains(robada, partida.Jugadores[0].Mano);
        Assert.Equal(0, partida.TurnoActual); // no pasa el turno solo
    }

    [Fact]
    public void JugarCarta_MasDos_SiguienteRoba2YPierdeTurno()
    {
        var partida = CrearPartida();
        partida.Mazo.Crear();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var masDos = new Carta(ColorCarta.Rojo, TipoCarta.MasDos);
        DarCartas(partida.Jugadores[0], masDos,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1),
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 2));

        partida.JugarCarta(masDos);

        Assert.Equal(2, partida.Jugadores[1].Mano.Count);
        Assert.Equal(2, partida.TurnoActual);
    }

    [Fact]
    public void JugarCarta_MasCuatro_SiguienteRoba4YCambiaColor()
    {
        var partida = CrearPartida();
        partida.Mazo.Crear();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var masCuatro = new Carta(ColorCarta.Negro, TipoCarta.ComodinMasCuatro);
        DarCartas(partida.Jugadores[0], masCuatro,
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 1),
            new Carta(ColorCarta.Azul, TipoCarta.Numero, 2));

        partida.JugarCarta(masCuatro, ColorCarta.Verde);

        Assert.Equal(4, partida.Jugadores[1].Mano.Count);
        Assert.Equal(2, partida.TurnoActual);
        Assert.Equal(ColorCarta.Verde, partida.ColorActual);
    }

    [Fact]
    public void JugarCarta_SinDecirUno_Roba2DeCastigo()
    {
        var partida = CrearPartida();
        partida.Mazo.Crear();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var carta = new Carta(ColorCarta.Rojo, TipoCarta.Numero, 8);
        DarCartas(partida.Jugadores[0], carta, new Carta(ColorCarta.Azul, TipoCarta.Numero, 1));

        partida.JugarCarta(carta);

        Assert.Equal(3, partida.Jugadores[0].Mano.Count); // le quedaba 1 + 2 de castigo
    }

    [Fact]
    public void JugarCarta_DiciendoUno_NoHayCastigo()
    {
        var partida = CrearPartida();
        partida.Mazo.Crear();
        partida.PonerCartaEnMesa(new Carta(ColorCarta.Rojo, TipoCarta.Numero, 5));
        var carta = new Carta(ColorCarta.Rojo, TipoCarta.Numero, 8);
        DarCartas(partida.Jugadores[0], carta, new Carta(ColorCarta.Azul, TipoCarta.Numero, 1));

        partida.DecirUno();
        partida.JugarCarta(carta);

        Assert.Single(partida.Jugadores[0].Mano);
    }
}