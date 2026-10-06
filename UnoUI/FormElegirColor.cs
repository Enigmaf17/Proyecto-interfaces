using UnoLogica;

namespace UnoUI
{
    // Ventanita que aparece al tirar un comodín para elegir el nuevo color
    public class FormElegirColor : Form
    {
        public ColorCarta ColorElegido { get; private set; } = ColorCarta.Rojo;

        public FormElegirColor()
        {
            Text = "Comodín";
            ClientSize = new Size(344, 115);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;   // aparece en medio del juego
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;   // sin botón de cerrar: hay que elegir un color
            BackColor = Color.FromArgb(55, 35, 105);

            var titulo = new Label
            {
                Text = "¿Qué color quieres?",
                Location = new Point(20, 8),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            Controls.Add(titulo);

            AgregarBoton(ColorCarta.Rojo, Color.FromArgb(230, 70, 70), 0);
            AgregarBoton(ColorCarta.Amarillo, Color.FromArgb(245, 205, 50), 1);
            AgregarBoton(ColorCarta.Verde, Color.FromArgb(60, 180, 95), 2);
            AgregarBoton(ColorCarta.Azul, Color.FromArgb(50, 130, 230), 3);
        }

        // Crea un botón cuadrado de un color
        private void AgregarBoton(ColorCarta color, Color colorPantalla, int posicion)
        {
            var boton = new Button
            {
                Location = new Point(20 + posicion * 78, 40),
                Size = new Size(70, 60),
                BackColor = colorPantalla,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            boton.FlatAppearance.BorderColor = Color.White;
            boton.FlatAppearance.BorderSize = 2;

            // Al dar clic: guardar el color y cerrar la ventana
            boton.Click += (s, e) =>
            {
                ColorElegido = color;
                DialogResult = DialogResult.OK;
            };

            Controls.Add(boton);
        }
    }
}