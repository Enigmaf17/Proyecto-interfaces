using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnoLogica;
using UnoRed;

namespace UnoUI
{
    // Une las pantallas con la red.
    // Las pantallas (FormInicio y, después, FormMesa) solo muestran datos y avisan clics;
    // esta clase decide qué mandar al servidor y qué hacer con cada mensaje que llega.
    public class SesionRed
    {
        private readonly FormInicio inicio;
        private ClienteRed? cliente;
        private Servidor? servidor;   // solo existe en la computadora que crea la partida
        private int miId;             // 0 = el servidor todavía no me acepta
        private bool partidaIniciada;

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
                    inicio.MostrarError(m.Texto ?? "Error desconocido");
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

                case TiposMensaje.Fin:
                    MessageBox.Show(inicio, $"¡Ganó {m.Ganador}!", "UNO");
                    break;

                case TiposMensaje.Desconexion:
                    MessageBox.Show(inicio, $"{m.Nombre} se desconectó. La partida se canceló.", "UNO");
                    break;
            }
        }

        // PROVISIONAL: cuando Rosa suba FormMesa, aquí se abre la mesa y se le pasa la vista.
        // Por ahora solo avisa que la partida comenzó, para probar la sala de espera.
        private void MostrarVista(VistaJugador vista)
        {
            if (partidaIniciada)
                return;
            partidaIniciada = true;

            string empieza = vista.Jugadores.FirstOrDefault(j => j.Id == vista.TurnoId)?.Nombre ?? "?";
            MessageBox.Show(inicio,
                "¡La partida comenzó!\n\n" +
                $"Tus cartas: {string.Join(", ", vista.MiMano)}\n" +
                $"Carta en la mesa: {vista.CartaArriba}\n" +
                $"Empieza: {empieza}",
                "UNO");
        }

        private void ConexionPerdida()
        {
            SoltarCliente();
            inicio.MostrarError("Se perdió la conexión con el servidor.");
            inicio.MostrarSala(new List<string>());
            inicio.Bloquear(false);
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
            partidaIniciada = false;
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

        // La IP de esta computadora en la red (primero la del Wi-Fi), para que los demás la escriban
        private static string ObtenerIpLocal()
        {
            var ips = new List<string>();
            foreach (NetworkInterface red in NetworkInterface.GetAllNetworkInterfaces()
                         .Where(r => r.OperationalStatus == OperationalStatus.Up
                                     && r.NetworkInterfaceType != NetworkInterfaceType.Loopback)
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
