namespace UnoUI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // El juego en red empieza en la pantalla de inicio;
            // SesionRed la conecta con el servidor y el cliente.
            var inicio = new FormInicio();
            var sesion = new SesionRed(inicio);
            Application.Run(inicio);
        }
    }
}
