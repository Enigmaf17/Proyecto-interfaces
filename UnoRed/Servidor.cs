using System.Net;
using System.Net.Sockets;
using UnoLogica;

namespace UnoRed
{
    // Servidor del juego. Corre solo en la computadora de Paul.
    // Es el único que tiene la partida real (Partida + ControladorPartida):
    // los clientes mandan acciones, el servidor las valida, las aplica,
    // las guarda en la base de datos y le manda a cada jugador lo que le toca ver.
    public class Servidor
    {
        private const int JugadoresNecesarios = 3;

        private readonly ApiCliente api;
        private readonly int puerto;
        private TcpListener? listener;

        // Todos los clientes conectados (unidos o todavía no)
        private readonly List<Conexion> conexiones = new List<Conexion>();

        // Solo un mensaje se procesa a la vez, aunque lleguen de 3 clientes al mismo tiempo
        private readonly SemaphoreSlim candado = new SemaphoreSlim(1, 1);

        private List<JugadorDto> jugadoresBd = new List<JugadorDto>();
        private ControladorPartida? controlador;
        private bool partidaEnCurso = false;
        private Carta? cartaRobadaPendiente;   // carta jugable que el jugador en turno acaba de robar
        private string? ultimoMovimiento;

        // Avisa lo que pasa en el servidor (para mostrarlo en pantalla o en la consola)
        public event Action<string>? Registro;

        public Servidor(ApiCliente api, int puerto = Protocolo.Puerto)
        {
            this.api = api;
            this.puerto = puerto;
        }

        // Carga los jugadores de la base de datos y empieza a aceptar clientes
        public async Task IniciarAsync()
        {
            jugadoresBd = await api.ObtenerJugadoresAsync();

            listener = new TcpListener(IPAddress.Any, puerto);
            listener.Start();
            Log($"Servidor escuchando en el puerto {puerto}");

            _ = AceptarClientesAsync();   // corre en segundo plano
        }

        public void Detener()
        {
            listener?.Stop();
            foreach (Conexion c in conexiones.ToList())
                c.Cerrar();
        }

        // ---------------- Conexiones ----------------

        private async Task AceptarClientesAsync()
        {
            while (true)
            {
                TcpClient cliente;
                try
                {
                    cliente = await listener!.AcceptTcpClientAsync();
                }
                catch (Exception)
                {
                    return;   // el servidor se detuvo
                }

                var conexion = new Conexion(cliente);
                await candado.WaitAsync();
                try { conexiones.Add(conexion); }
                finally { candado.Release(); }

                Log($"Cliente conectado desde {cliente.Client.RemoteEndPoint}");
                _ = AtenderClienteAsync(conexion);   // una tarea por cliente
            }
        }

        // Lee los mensajes de un cliente hasta que se desconecta
        private async Task AtenderClienteAsync(Conexion c)
        {
            try
            {
                string? linea;
                while ((linea = await c.Lector.ReadLineAsync()) != null)
                {
                    Mensaje? mensaje = Protocolo.Deserializar(linea);

                    await candado.WaitAsync();
                    try
                    {
                        if (mensaje == null)
                            await EnviarErrorAsync(c, "Mensaje no válido");
                        else
                            await ProcesarAsync(c, mensaje);
                    }
                    catch (Exception ex)
                    {
                        // Por ejemplo, si la API está apagada
                        Log($"Error al procesar {mensaje?.Tipo}: {ex.Message}");
                        await EnviarErrorAsync(c, "Error en el servidor: " + ex.Message);
                    }
                    finally
                    {
                        candado.Release();
                    }
                }
            }
            catch (IOException) { }               // el cliente se desconectó de golpe
            catch (ObjectDisposedException) { }   // la conexión ya estaba cerrada

            await candado.WaitAsync();
            try { await QuitarClienteAsync(c); }
            finally { candado.Release(); }
        }

        private async Task QuitarClienteAsync(Conexion c)
        {
            if (!conexiones.Remove(c))
                return;
            c.Cerrar();
            Log($"Se desconectó {c.Nombre ?? "un cliente"}");

            if (c.JugadorId == null)
                return;   // nunca se unió, no afecta a nadie

            if (partidaEnCurso)
            {
                // Sin un jugador no se puede seguir: se cancela la partida
                partidaEnCurso = false;
                controlador = null;
                cartaRobadaPendiente = null;
                await EnviarATodosAsync(new Mensaje { Tipo = TiposMensaje.Desconexion, Nombre = c.Nombre });
            }
            else
            {
                await EnviarSalaAsync();
            }
        }

        // ---------------- Mensajes ----------------

        private async Task ProcesarAsync(Conexion c, Mensaje m)
        {
            if (m.Tipo == TiposMensaje.Unirse)
            {
                await UnirseAsync(c, m.Nombre);
                return;
            }

            if (c.JugadorId == null)
            {
                await EnviarErrorAsync(c, "Primero únete a la partida");
                return;
            }

            if (m.Tipo == TiposMensaje.Historial)
            {
                await EnviarHistorialAsync(c);
                return;
            }

            if (!partidaEnCurso || controlador == null)
            {
                await EnviarErrorAsync(c, "La partida no está en curso");
                return;
            }

            Partida partida = controlador.Partida;
            if (partida.JugadorEnTurno.Id != c.JugadorId)
            {
                await EnviarErrorAsync(c, "No es tu turno");
                return;
            }

            switch (m.Tipo)
            {
                case TiposMensaje.Jugar:
                    await JugarAsync(c, m);
                    break;
                case TiposMensaje.Robar:
                    await RobarAsync(c);
                    break;
                case TiposMensaje.Pasar:
                    await PasarAsync(c);
                    break;
                case TiposMensaje.Uno:
                    await controlador.DecirUnoAsync();
                    ultimoMovimiento = $"{c.Nombre} dijo UNO";
                    await EnviarEstadosAsync();
                    break;
                default:
                    await EnviarErrorAsync(c, "Mensaje no válido");
                    break;
            }
        }

        private async Task UnirseAsync(Conexion c, string? nombre)
        {
            if (c.JugadorId != null)
            {
                await EnviarErrorAsync(c, "Ya estás en la partida");
                return;
            }
            if (partidaEnCurso)
            {
                await EnviarErrorAsync(c, "La partida ya empezó");
                return;
            }

            // El nombre debe existir en la base de datos y no estar ocupado
            JugadorDto? jugador = jugadoresBd.FirstOrDefault(j =>
                string.Equals(j.Nombre, nombre?.Trim(), StringComparison.OrdinalIgnoreCase));
            bool ocupado = jugador != null && conexiones.Any(x => x.JugadorId == jugador.Id);
            if (jugador == null || ocupado)
            {
                await EnviarErrorAsync(c, "Ese jugador no existe o ya está conectado");
                return;
            }

            c.JugadorId = jugador.Id;
            c.Nombre = jugador.Nombre;
            Log($"{c.Nombre} se unió");

            await c.EnviarAsync(new Mensaje { Tipo = TiposMensaje.Bienvenida, JugadorId = jugador.Id });
            await EnviarSalaAsync();

            if (conexiones.Count(x => x.JugadorId != null) < JugadoresNecesarios)
                return;

            try
            {
                await IniciarPartidaAsync();
            }
            catch (Exception ex)
            {
                // Normalmente pasa si la API está apagada. Se saca al último jugador
                // de la sala para que pueda volver a intentarlo con otro UNIRSE.
                Log($"No se pudo iniciar la partida: {ex.Message}");
                controlador = null;
                partidaEnCurso = false;
                c.JugadorId = null;
                c.Nombre = null;
                await EnviarErrorAsync(c, "No se pudo iniciar la partida. Revisen que la API esté encendida y vuelve a unirte.");
                await EnviarSalaAsync();
            }
        }

        private async Task IniciarPartidaAsync()
        {
            // Los jugadores en el orden de la base de datos
            List<Jugador> jugadores = jugadoresBd
                .Where(j => conexiones.Any(c => c.JugadorId == j.Id))
                .Select(j => new Jugador(j.Id, j.Nombre))
                .ToList();

            controlador = new ControladorPartida(api);
            await controlador.IniciarAsync(jugadores);   // crea la partida en la BD y reparte

            partidaEnCurso = true;
            cartaRobadaPendiente = null;
            ultimoMovimiento = "Comienza la partida";
            Log("Comienza la partida");
            await EnviarEstadosAsync();
        }

        private async Task JugarAsync(Conexion c, Mensaje m)
        {
            Partida partida = controlador!.Partida;
            Jugador jugador = partida.JugadorEnTurno;

            if (m.Indice == null || m.Indice < 0 || m.Indice >= jugador.Mano.Count)
            {
                await EnviarErrorAsync(c, "Esa carta no existe");
                return;
            }

            Carta carta = jugador.Mano[m.Indice.Value];

            if (cartaRobadaPendiente != null && carta != cartaRobadaPendiente)
            {
                await EnviarErrorAsync(c, "Solo puedes tirar la carta que robaste o pasar");
                return;
            }

            ColorCarta? color = null;
            if (carta.Tipo == TipoCarta.Comodin || carta.Tipo == TipoCarta.ComodinMasCuatro)
            {
                if (!Enum.TryParse(m.Color, true, out ColorCarta elegido) || elegido == ColorCarta.Negro)
                {
                    await EnviarErrorAsync(c, "Elige un color para el comodín");
                    return;
                }
                color = elegido;
            }

            if (!partida.PuedeJugar(carta))
            {
                await EnviarErrorAsync(c, "Esa carta no se puede jugar");
                return;
            }

            await controlador.JugarCartaAsync(carta, color);
            cartaRobadaPendiente = null;
            ultimoMovimiento = $"{c.Nombre} jugó {NombreCarta(carta)}" + (color != null ? $" y eligió {color}" : "");

            await EnviarEstadosAsync();

            if (partida.Ganador != null)
            {
                Log($"Ganó {partida.Ganador.Nombre}");
                partidaEnCurso = false;
                await EnviarATodosAsync(new Mensaje { Tipo = TiposMensaje.Fin, Ganador = partida.Ganador.Nombre });
            }
        }

        private async Task RobarAsync(Conexion c)
        {
            if (cartaRobadaPendiente != null)
            {
                await EnviarErrorAsync(c, "Ya robaste: tira esa carta o pasa");
                return;
            }

            Carta carta = await controlador!.RobarCartaAsync();

            if (controlador.Partida.PuedeJugar(carta))
            {
                // La puede jugar: se le pregunta (el cliente manda JUGAR o PASAR)
                cartaRobadaPendiente = carta;
                ultimoMovimiento = $"{c.Nombre} robó una carta";
            }
            else
            {
                // No la puede jugar: pierde el turno automáticamente
                await controlador.PasarTurnoAsync();
                ultimoMovimiento = $"{c.Nombre} robó una carta y pasó";
            }
            await EnviarEstadosAsync();
        }

        private async Task PasarAsync(Conexion c)
        {
            if (cartaRobadaPendiente == null)
            {
                await EnviarErrorAsync(c, "Primero roba una carta");
                return;
            }

            await controlador!.PasarTurnoAsync();
            cartaRobadaPendiente = null;
            ultimoMovimiento = $"{c.Nombre} pasó";
            await EnviarEstadosAsync();
        }

        private async Task EnviarHistorialAsync(Conexion c)
        {
            var lista = new List<EstadisticasDto>();
            foreach (JugadorDto j in jugadoresBd)
            {
                EstadisticasDto? e = await api.ObtenerEstadisticasAsync(j.Id);
                if (e != null)
                    lista.Add(e);
            }
            await c.EnviarAsync(new Mensaje { Tipo = TiposMensaje.Estadisticas, Estadisticas = lista });
        }

        // ---------------- Envíos ----------------

        // A cada jugador le manda SU vista: su mano y cuántas cartas tienen los demás
        private async Task EnviarEstadosAsync()
        {
            Partida partida = controlador!.Partida;
            foreach (Conexion c in conexiones.Where(x => x.JugadorId != null).ToList())
            {
                bool leToca = partida.JugadorEnTurno.Id == c.JugadorId;
                string? robada = leToca && cartaRobadaPendiente != null ? NombreCarta(cartaRobadaPendiente) : null;

                VistaJugador vista = VistaJugador.Crear(partida, c.JugadorId!.Value, robada, ultimoMovimiento);
                await c.EnviarAsync(new Mensaje { Tipo = TiposMensaje.Estado, Vista = vista });
            }
        }

        private async Task EnviarSalaAsync()
        {
            List<string> nombres = conexiones.Where(x => x.Nombre != null).Select(x => x.Nombre!).ToList();
            await EnviarATodosAsync(new Mensaje { Tipo = TiposMensaje.Sala, Conectados = nombres });
        }

        private async Task EnviarATodosAsync(Mensaje m)
        {
            foreach (Conexion c in conexiones.Where(x => x.JugadorId != null).ToList())
                await c.EnviarAsync(m);
        }

        private static Task EnviarErrorAsync(Conexion c, string texto)
        {
            return c.EnviarAsync(new Mensaje { Tipo = TiposMensaje.Error, Texto = texto });
        }

        private static string NombreCarta(Carta carta) => carta.NombreImagen().Replace(".png", "");

        private void Log(string texto) => Registro?.Invoke(texto);

        // ---------------- Un cliente conectado ----------------

        private class Conexion
        {
            private readonly TcpClient cliente;
            public StreamReader Lector { get; }
            private readonly StreamWriter escritor;

            public int? JugadorId { get; set; }   // null hasta que manda UNIRSE
            public string? Nombre { get; set; }

            public Conexion(TcpClient cliente)
            {
                this.cliente = cliente;
                NetworkStream flujo = cliente.GetStream();
                Lector = new StreamReader(flujo, Protocolo.Codificacion);
                escritor = new StreamWriter(flujo, Protocolo.Codificacion) { AutoFlush = true };
            }

            // Un mensaje = una línea de JSON
            public async Task EnviarAsync(Mensaje m)
            {
                try
                {
                    await escritor.WriteLineAsync(Protocolo.Serializar(m));
                }
                catch (Exception)
                {
                    // Si falla, su tarea de lectura detecta la desconexión y lo quita
                }
            }

            public void Cerrar() => cliente.Close();
        }
    }
}
