namespace UnoUI
{
    public class FormInicio : Form
    {
        // Valores de la paleta de Form1.
        private static readonly Color Fondo = Color.FromArgb(14, 15, 18);
        private static readonly Color PanelFondo = Color.FromArgb(30, 33, 38);
        private static readonly Color Marco = Color.FromArgb(60, 64, 72);
        private static readonly Color Dorado = Color.FromArgb(255, 200, 60);
        private static readonly Color TextoClaro = Color.FromArgb(225, 225, 230);

        private readonly ComboBox comboNombre = new ComboBox();

        private readonly TextBox cajaIp = new TextBox();
        private readonly Button botonCrear = new Button();
        private readonly Button botonUnirse = new Button();

        public event EventHandler? CrearPartidaClic;
        public event EventHandler? UnirseClic;

        public string IpServidor => cajaIp.Text.Trim();
        public string NombreElegido => comboNombre.Text;

        public FormInicio()
        {
            Text = "UNO - Inicio";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(440, 430);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Segoe UI", 10);
            BackColor = Fondo;
            ForeColor = TextoClaro;

            var titulo = new Label
            {
                Text = "UNO | Jugar en red",
                AutoSize = true,
                Location = new Point(24, 20),
                ForeColor = Dorado
            };

            var etiquetaNombre = new Label
            {
                Text = "Elige tu nombre",
                AutoSize = true,
                Location = new Point(24, 60)
            };

            comboNombre.DropDownStyle = ComboBoxStyle.DropDownList;
            comboNombre.BackColor = PanelFondo;
            comboNombre.ForeColor = TextoClaro;
            comboNombre.Items.AddRange(new object[] { "Paul", "Rosa", "Dalton" });
            comboNombre.SelectedIndex = 0;
            comboNombre.SetBounds(24, 86, 392, 28);

            var etiquetaIp = new Label
            {
                Text = "IP del servidor",
                AutoSize = true,
                Location = new Point(24, 126)
            };
            cajaIp.Text = "127.0.0.1";
            cajaIp.BackColor = PanelFondo;
            cajaIp.ForeColor = TextoClaro;
            cajaIp.SetBounds(24, 152, 256, 28);

            var etiquetaPuerto = new Label
            {
                Text = "Puerto 5000",
                AutoSize = true,
                Location = new Point(296, 155)
            };

            ConfigurarBoton(botonCrear, "Crear partida", 24, Dorado, Fondo);
            ConfigurarBoton(botonUnirse, "Unirse", 228, PanelFondo, TextoClaro);

            botonCrear.Click += (_, _) => CrearPartidaClic?.Invoke(this, EventArgs.Empty);
            botonUnirse.Click += (_, _) => UnirseClic?.Invoke(this, EventArgs.Empty);

            Controls.AddRange(new Control[]
            {
                titulo, etiquetaNombre, comboNombre, etiquetaIp, cajaIp,
                etiquetaPuerto, botonCrear, botonUnirse
            });
        }

        private static void ConfigurarBoton(Button boton, string texto, int x, Color fondo, Color textoColor)
        {
            boton.Text = texto;
            boton.SetBounds(x, 194, 188, 36);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = Marco;
            boton.BackColor = fondo;
            boton.ForeColor = textoColor;
        }
    }
}
