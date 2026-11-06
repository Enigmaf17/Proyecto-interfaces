using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

// Programa de prueba de red para el juego UNO.
// Sirve para comprobar, antes de programar el juego, que las 3 computadoras
// se pueden conectar por TCP en la zona Wi-Fi.
//
// Uso: ejecutar el programa y elegir
//   1 = servidor (solo en la computadora de Paul)
//   2 = cliente  (en las 3 computadoras, incluida la de Paul)
//
// Cada línea que escribe un cliente le llega al servidor, y el servidor
// la reenvía a todos los clientes conectados.

const int Puerto = 5000;
var utf8 = new UTF8Encoding(false); // sin BOM, para que el primer mensaje llegue limpio

Console.WriteLine("Prueba de red - Juego UNO");
Console.WriteLine("1) Servidor");
Console.WriteLine("2) Cliente");
Console.Write("Elige una opción: ");
string? opcion = Console.ReadLine();

if (opcion == "1")
    await CorrerServidorAsync();
else
    await CorrerClienteAsync();

// ---------------- SERVIDOR ----------------

async Task CorrerServidorAsync()
{
    var clientes = new List<StreamWriter>();
    var candado = new object();

    var listener = new TcpListener(IPAddress.Any, Puerto);
    listener.Start();

    Console.WriteLine($"\nServidor escuchando en el puerto {Puerto}.");
    Console.WriteLine("Los clientes deben escribir alguna de estas IP:");
    foreach (string ip in ObtenerIpsLocales())
        Console.WriteLine($"  {ip}");
    Console.WriteLine("(Ctrl+C para terminar)\n");

    int contador = 0;
    while (true)
    {
        TcpClient cliente = await listener.AcceptTcpClientAsync();
        int numero = ++contador;
        _ = AtenderClienteAsync(cliente, numero, clientes, candado);
    }
}

async Task AtenderClienteAsync(TcpClient cliente, int numero, List<StreamWriter> clientes, object candado)
{
    string origen = cliente.Client.RemoteEndPoint?.ToString() ?? "?";
    Console.WriteLine($"[+] Cliente {numero} conectado desde {origen}");

    NetworkStream flujo = cliente.GetStream();
    var lector = new StreamReader(flujo, utf8);
    var escritor = new StreamWriter(flujo, utf8) { AutoFlush = true };
    lock (candado) clientes.Add(escritor);

    try
    {
        string? linea;
        // ReadLineAsync regresa un mensaje completo (hasta el salto de línea),
        // aunque TCP lo haya entregado en pedazos. Regresa null si el cliente se desconecta.
        while ((linea = await lector.ReadLineAsync()) != null)
        {
            Console.WriteLine($"Cliente {numero}: {linea}");
            Reenviar($"Cliente {numero} dice: {linea}", clientes, candado);
        }
    }
    catch (IOException)
    {
        // El cliente cerró la conexión de golpe
    }
    finally
    {
        lock (candado) clientes.Remove(escritor);
        cliente.Close();
        Console.WriteLine($"[-] Cliente {numero} desconectado");
    }
}

void Reenviar(string texto, List<StreamWriter> clientes, object candado)
{
    lock (candado)
    {
        foreach (StreamWriter escritor in clientes)
        {
            try { escritor.WriteLine(texto); }
            catch (IOException) { /* se limpia cuando su tarea detecte la desconexión */ }
        }
    }
}

List<string> ObtenerIpsLocales()
{
    var ips = new List<string>();
    foreach (NetworkInterface red in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (red.OperationalStatus != OperationalStatus.Up) continue;
        if (red.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

        foreach (UnicastIPAddressInformation dir in red.GetIPProperties().UnicastAddresses)
        {
            if (dir.Address.AddressFamily == AddressFamily.InterNetwork)
                ips.Add($"{dir.Address}  ({red.Name})");
        }
    }
    return ips;
}

// ---------------- CLIENTE ----------------

async Task CorrerClienteAsync()
{
    Console.Write("\nIP del servidor (Enter = 127.0.0.1): ");
    string? ip = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

    using var cliente = new TcpClient();
    try
    {
        var conexion = cliente.ConnectAsync(ip.Trim(), Puerto);
        if (await Task.WhenAny(conexion, Task.Delay(5000)) != conexion)
        {
            Console.WriteLine("No hubo respuesta en 5 segundos. Revisa la IP, que estén en la misma red y el firewall del servidor.");
            return;
        }
        await conexion; // lanza la excepción si la conexión fue rechazada
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"No se pudo conectar: {ex.Message}");
        return;
    }

    Console.WriteLine("Conectado. Escribe un mensaje y presiona Enter (escribe 'salir' para terminar).\n");

    NetworkStream flujo = cliente.GetStream();
    var lector = new StreamReader(flujo, utf8);
    var escritor = new StreamWriter(flujo, utf8) { AutoFlush = true };

    // Tarea en segundo plano que muestra lo que manda el servidor
    _ = Task.Run(async () =>
    {
        try
        {
            string? linea;
            while ((linea = await lector.ReadLineAsync()) != null)
                Console.WriteLine($"  << {linea}");
            Console.WriteLine("El servidor cerró la conexión.");
        }
        catch (IOException)
        {
            Console.WriteLine("Se perdió la conexión con el servidor.");
        }
    });

    while (true)
    {
        string? texto = Console.ReadLine();
        if (texto == null || texto.Trim().ToLower() == "salir") break;
        await escritor.WriteLineAsync(texto);
    }
}
