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

            Controls.AddRange(new Control[] { titulo, etiquetaNombre, comboNombre });
        }
    }
}
