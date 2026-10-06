namespace UnoUI
{
    // Pantalla de carga que se muestra mientras se conecta con la base de datos
    public class FormCarga : Form
    {
        private readonly Label lblEstado = new Label();

        private static readonly Color Fondo = Color.FromArgb(30, 33, 38);
        private static readonly Color Dorado = Color.FromArgb(255, 200, 60);

        public FormCarga()
        {
            FormBorderStyle = FormBorderStyle.None;   // sin barra de título
            ClientSize = new Size(520, 300);
            BackColor = Fondo;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Paint += (s, e) =>
            {
                // Marco dorado alrededor de la ventana
                using var marco = new Pen(Dorado, 4);
                e.Graphics.DrawRectangle(marco, 2, 2, ClientSize.Width - 4, ClientSize.Height - 4);
            };

            // Reverso de la carta (tiene el logo de UNO)
            var picLogo = new PictureBox
            {
                Location = new Point(40, 60),
                Size = new Size(120, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(Path.Combine(AppContext.BaseDirectory, "Imagenes", "reverso.png"))
            };

            var lblTitulo = new Label
            {
                Text = "Juego UNO",
                Location = new Point(190, 70),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 30, FontStyle.Bold)
            };

            lblEstado.Location = new Point(194, 135);
            lblEstado.AutoSize = true;
            lblEstado.ForeColor = Dorado;
            lblEstado.Font = new Font("Segoe UI", 11);

            var barra = new ProgressBar
            {
                Location = new Point(194, 170),
                Size = new Size(280, 18),
                Style = ProgressBarStyle.Marquee,   // barra animada que va y viene
                MarqueeAnimationSpeed = 30
            };

            var lblPie = new Label
            {
                Text = "Interfaces · Facultad de Ingeniería, UASLP",
                Location = new Point(194, 250),
                AutoSize = true,
                ForeColor = Color.FromArgb(150, 150, 160),
                Font = new Font("Segoe UI", 9)
            };

            Controls.AddRange(new Control[] { picLogo, lblTitulo, lblEstado, barra, lblPie });
        }

        // Cambia el texto de lo que se está haciendo
        public void MostrarEstado(string texto)
        {
            lblEstado.Text = texto;
            lblEstado.Refresh();   // que se vea de inmediato
        }

        // Muestra la pantalla centrada encima de la ventana del juego
        public void MostrarSobre(Form dueño)
        {
            Location = new Point(
                dueño.Left + (dueño.Width - Width) / 2,
                dueño.Top + (dueño.Height - Height) / 2);
            Show(dueño);
            Refresh();
        }
    }
}