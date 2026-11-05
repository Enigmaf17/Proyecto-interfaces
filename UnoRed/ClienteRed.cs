using System.Net.Sockets;

namespace UnoRed
{
    // Conexión de un jugador con el servidor. La usan las 3 computadoras
    // (la de Paul también, conectándose a 127.0.0.1).
    // Manda las acciones del jugador y avisa con un evento cada mensaje que llega.
    public class ClienteRed
    {
        private TcpClient? cliente;
        private StreamReader? lector;
        private StreamWriter? escritor;
        private readonly SemaphoreSlim candadoEnvio = new SemaphoreSlim(1, 1);

        // Hilo de la ventana: los eventos se disparan ahí para que la interfaz
        // pueda modificar sus controles sin usar Invoke.
        private SynchronizationContext? contexto;

        public event Action<Mensaje>? MensajeRecibido;
        public event Action? Desconectado;

        public bool Conectado => cliente?.Connected ?? false;

        // Se conecta al servidor. Lanza una excepción si no responde a tiempo.
        public async Task ConectarAsync(string ip, int puerto = Protocolo.Puerto, int esperaMs = 5000)
        {
            contexto = SynchronizationContext.Current;
            cliente = new TcpClient();

            using var cancelar = new CancellationTokenSource(esperaMs);
            try
            {
                await cliente.ConnectAsync(ip.Trim(), puerto, cancelar.Token);
            }
            catch (OperationCanceledException)
            {
                cliente.Close();
                throw new TimeoutException("El servidor no respondió. Revisa la IP, la red y el firewall.");
            }

            NetworkStream flujo = cliente.GetStream();
            lector = new StreamReader(flujo, Protocolo.Codificacion);
            escritor = new StreamWriter(flujo, Protocolo.Codificacion) { AutoFlush = true };

            _ = EscucharAsync();   // recibe mensajes en segundo plano
        }

        public void Desconectar()
        {
            cliente?.Close();
        }

        // ---------------- Acciones del jugador ----------------

        public Task UnirseAsync(string nombre) =>
            EnviarAsync(new Mensaje { Tipo = TiposMensaje.Unirse, Nombre = nombre });

        // indice = posición de la carta en MiMano; color solo para comodines ("Rojo", "Azul"...)
        public Task JugarAsync(int indice, string? color = null) =>
            EnviarAsync(new Mensaje { Tipo = TiposMensaje.Jugar, Indice = indice, Color = color });

        public Task RobarAsync() => EnviarAsync(new Mensaje { Tipo = TiposMensaje.Robar });

        public Task PasarAsync() => EnviarAsync(new Mensaje { Tipo = TiposMensaje.Pasar });

        public Task DecirUnoAsync() => EnviarAsync(new Mensaje { Tipo = TiposMensaje.Uno });

        public Task PedirHistorialAsync() => EnviarAsync(new Mensaje { Tipo = TiposMensaje.Historial });

        // Un mensaje = una línea de JSON. El candado evita que dos envíos se mezclen.
        public async Task EnviarAsync(Mensaje m)
        {
            if (escritor == null)
                throw new InvalidOperationException("Primero hay que conectarse.");

            await candadoEnvio.WaitAsync();
            try
            {
                await escritor.WriteLineAsync(Protocolo.Serializar(m));
            }
            finally
            {
                candadoEnvio.Release();
            }
        }

        // ---------------- Recepción ----------------

        private async Task EscucharAsync()
        {
            try
            {
                string? linea;
                // ReadLineAsync regresa un mensaje completo; null = el servidor cerró la conexión
                while ((linea = await lector!.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    Mensaje? m = Protocolo.Deserializar(linea);
                    if (m != null)
                        Notificar(() => MensajeRecibido?.Invoke(m));
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }

            Notificar(() => Desconectado?.Invoke());
        }

        // Ejecuta el aviso en el hilo de la ventana (si lo hay)
        private void Notificar(Action accion)
        {
            if (contexto != null)
                contexto.Post(_ => accion(), null);
            else
                accion();
        }
    }
}
