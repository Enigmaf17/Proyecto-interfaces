using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnoLogica;
using UnoRed;

namespace UnoUI
{
    // Une las pantallas con la red.
    // Las pantallas (FormInicio y FormMesa) solo muestran datos y avisan clics;
    // esta clase decide qué mandar al servidor y qué hacer con cada mensaje que llega.
    public class SesionRed
    {
        private readonly FormInicio inicio;
        private FormMesa? mesa;              // se crea cuando llega el primer ESTADO
        private ClienteRed? cliente;
        private Servidor? servidor;          // solo existe en la computadora que crea la partida
        private VistaJugador? ultimaVista;   // lo último que mandó el servidor
        private int miId;                    // 0 = el servidor todavía no me acepta

        public SesionRed(FormInicio inicio)
        {
            this.inicio = inicio;
            inicio.CrearPartidaClic += async (s, e) => await CrearPartidaAsync();
            inicio.UnirseClic += async (s, e) => await UnirseAsync(inicio.IpServidor);
            inicio.FormClosed += (s, e) => Cerrar();
        }

        // ---------------- Botones de la pantalla de inicio ----------------

        // "Crear partida": prende el servidor en esta computadora y se une a él
        private async Task CrearPartidaAsync()
        {
            inicio.Bloquear(true);
            try
            {
                servidor = new Servidor(new ApiCliente());
                servidor.Registro += texto => Debug.WriteLine("[Servidor] " + texto);
                await servidor.IniciarAsync();
            }
            catch (SocketException)
            {
                DetenerServidor();
                inicio.MostrarError($"El puerto {Protocolo.Puerto} está ocupado. ¿Ya hay una partida creada en esta computadora?");
                inicio.Bloquear(false);
                return;
            }
            catch (Exception)
            {
                DetenerServidor();
                inicio.MostrarError("No se pudo conectar con la API. Enciéndela (uvicorn main:app) y revisa que MySQL esté corriendo.");
                inicio.Bloquear(false);
                return;
            }

            inicio.MostrarIpLocal(ObtenerIpLocal());

            // El que crea la partida también juega: se conecta a su propio servidor
            await UnirseAsync("127.0.0.1");
        }

        // "Unirse": se conecta al servidor de la IP escrita y pide entrar con el nombre elegido
        private async Task UnirseAsync(string ip)
        {
            inicio.Bloquear(true);
            try
            {
                cliente = new ClienteRed();
                cliente.MensajeRecibido += ProcesarMensaje;
                cliente.Desconectado += ConexionPerdida;
                await cliente.ConectarAsync(ip);
                await cliente.UnirseAsync(inicio.NombreElegido);
            }
            catch (Exception ex)
            {
                SoltarCliente();
                inicio.MostrarError("No se pudo conectar al servidor: " + ex.Message);
                inicio.Bloquear(false);
            }
        }

        // ---------------- Mensajes del servidor ----------------

        // ClienteRed dispara este evento en el hilo de la ventana, así que aquí
        // se pueden modificar los controles directamente.
        private void ProcesarMensaje(Mensaje m)
        {
            switch (m.Tipo)
            {
                case TiposMensaje.Bienvenida:
                    miId = m.JugadorId ?? 0;
                    break;

                case TiposMensaje.Sala:
                    inicio.MostrarSala(m.Conectados ?? new List<string>());
                    break;

                case TiposMensaje.Error:
                    MostrarError(m.Texto ?? "Error desconocido");
                    if (miId == 0)
                    {
                        // El servidor no me aceptó (por ejemplo, ese nombre ya está ocupado):
                        // se cierra esta conexión para poder intentarlo otra vez
                        SoltarCliente();
                        inicio.Bloquear(false);
                    }
                    break;

                case TiposMensaje.Estado:
                    if (m.Vista != null)
                        MostrarVista(m.Vista);
                    break;

                case TiposMensaje.Estadisticas:
                    if (mesa != null && m.Estadisticas != null)
                    {
                        using var ventana = new FormHistorial(m.Estadisticas);
                        ventana.ShowDialog(mesa);
                    }
                    break;

                case TiposMensaje.Fin:
                    mesa?.MostrarGanador(m.Ganador ?? "?");
                    break;

                case TiposMensaje.Desconexion:
                    mesa?.MostrarDesconexion(m.Nombre ?? "Un jugador");
                    break;
            }
        }

        // Con el primer ESTADO se cambia de la pantalla de inicio a la mesa
        private void MostrarVista(VistaJugador vista)
        {
            ultimaVista = vista;

            if (mesa == null)
            {
                mesa = new FormMesa();
                mesa.CartaClic += indice => Enviar(() => JugarCartaAsync(indice));
                mesa.MazoClic += (s, e) => Enviar(() => cliente!.RobarAsync());
                mesa.PasarClic += (s, e) => Enviar(() => cliente!.PasarAsync());
                mesa.UnoClic += (s, e) => Enviar(() => cliente!.DecirUnoAsync());
                mesa.HistorialClic += (s, e) => Enviar(() => cliente!.PedirHistorialAsync());
                mesa.FormClosed += (s, e) => inicio.Close();   // cerrar la mesa cierra el juego

                inicio.Hide();
                mesa.Show();
            }

            mesa.MostrarVista(vista);
        }

        // Si la carta es un comodín, primero se pregunta el color
        private async Task JugarCartaAsync(int indice)
        {
            if (cliente == null || ultimaVista == null || indice >= ultimaVista.MiMano.Count)
                return;

            string? color = null;
            if (ultimaVista.MiMano[indice].StartsWith("comodin"))
            {
                using var ventana = new FormElegirColor();
                ventana.ShowDialog(mesa);
                color = ventana.ColorElegido.ToString();   // "Rojo", "Azul"...
            }

            await cliente.JugarAsync(indice, color);
        }

        // Manda una acción al servidor; si la conexión ya no existe, avisa en lugar de tronar
        private async void Enviar(Func<Task> accion)
        {
            if (cliente == null)
                return;
            try
            {
                await accion();
            }
            catch (Exception)
            {
                MostrarError("No se pudo enviar la jugada: se perdió la conexión con el servidor.");
            }
        }

        private void ConexionPerdida()
        {
            SoltarCliente();
            if (mesa != null)
            {
                mesa.MostrarDesconexion("El servidor");
            }
            else
            {
                inicio.MostrarError("Se perdió la conexión con el servidor.");
                inicio.MostrarSala(new List<string>());
                inicio.Bloquear(false);
            }
        }

        private void MostrarError(string texto)
        {
            if (mesa != null)
                mesa.MostrarError(texto);
            else
                inicio.MostrarError(texto);
        }

        // ---------------- Cierre ----------------

        // Deja de escuchar al cliente y cierra su conexión
        private void SoltarCliente()
        {
            if (cliente != null)
            {
                cliente.MensajeRecibido -= ProcesarMensaje;
                cliente.Desconectado -= ConexionPerdida;
                cliente.Desconectar();
                cliente = null;
            }
            miId = 0;
        }

        private void DetenerServidor()
        {
            servidor?.Detener();
            servidor = null;
        }

        private void Cerrar()
        {
            SoltarCliente();
            DetenerServidor();
        }

        // La IP de esta computadora en la red (primero la del Wi-Fi), para que los demás la escriban.
        // Se saltan los adaptadores virtuales (VirtualBox, VMware, Hyper-V), que no sirven para jugar.
        private static string ObtenerIpLocal()
        {
            string[] virtuales = { "virtual", "vmware", "hyper-v", "vethernet", "bluetooth" };
            var ips = new List<string>();

            foreach (NetworkInterface red in NetworkInterface.GetAllNetworkInterfaces()
                         .Where(r => r.OperationalStatus == OperationalStatus.Up
                                     && r.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                     && !virtuales.Any(v => (r.Description + " " + r.Name).ToLower().Contains(v)))
                         .OrderBy(r => r.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ? 0 : 1))
            {
                foreach (UnicastIPAddressInformation dir in red.GetIPProperties().UnicastAddresses)
                {
                    if (dir.Address.AddressFamily == AddressFamily.InterNetwork)
                        ips.Add(dir.Address.ToString());
                }
            }
            return ips.Count > 0 ? string.Join("  /  ", ips) : "sin red";
        }
    }
}
