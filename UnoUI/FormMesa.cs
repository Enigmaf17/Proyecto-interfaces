using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using UnoRed;

namespace UnoUI
{
    public class FormMesa : Form
    {
        private static readonly Color FondoOrilla =
            Color.FromArgb(14, 15, 18);

        private static readonly Color FieltroCentro =
            Color.FromArgb(40, 140, 80);

        private static readonly Color FieltroOrilla =
            Color.FromArgb(18, 90, 50);

        private static readonly Color Madera =
            Color.FromArgb(105, 65, 30);

        private static readonly Color TextoClaro =
            Color.FromArgb(225, 225, 230);

        private VistaJugador? vistaActual;

        private readonly FlowLayoutPanel panelMano =
            new FlowLayoutPanel();

        public FormMesa()
        {
            Text = "UNO - Mesa en red";
            ClientSize = new Size(1100, 720);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;

            BackColor = FondoOrilla;
            ForeColor = TextoClaro;
            Font = new System.Drawing.Font("Segoe UI", 10);

            DoubleBuffered = true;
            ResizeRedraw = true;

            panelMano.Dock = DockStyle.Bottom;
            panelMano.Height = 160;
            panelMano.Padding = new Padding(15, 8, 15, 8);
            panelMano.BackColor = FondoOrilla;
            panelMano.AutoScroll = true;
            panelMano.WrapContents = false;

            Controls.Add(panelMano);
        }

        public void MostrarVista(VistaJugador vista)
        {
            vistaActual = vista;
            MostrarMiMano();
            Invalidate();
        }

        private void MostrarMiMano()
        {
            panelMano.SuspendLayout();

            try
            {
                // Quitar y liberar las cartas anteriores.
                while (panelMano.Controls.Count > 0)
                {
                    Control cartaAnterior = panelMano.Controls[0];
                    panelMano.Controls.Remove(cartaAnterior);
                    cartaAnterior.Dispose();
                }

                if (vistaActual == null)
                {
                    return;
                }

                // Mostrar las cartas de la vista recibida.
                foreach (string nombreCarta in vistaActual.MiMano)
                {
                    Image? imagen = CargarImagen(nombreCarta);

                    PictureBox carta = new PictureBox
                    {
                        Width = 85,
                        Height = 125,
                        Margin = new Padding(5),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent,
                        Image = imagen,
                        AccessibleName = nombreCarta
                    };

                    carta.Disposed += (_, _) => imagen?.Dispose();

                    panelMano.Controls.Add(carta);
                }
            }
            finally
            {
                panelMano.ResumeLayout();
            }
        }

        private static Image? CargarImagen(string nombreCarta)
        {
            string ruta = Path.Combine(
                AppContext.BaseDirectory,
                "Imagenes",
                nombreCarta + ".png"
            );

            if (!File.Exists(ruta))
            {
                return null;
            }

            using (Image original = Image.FromFile(ruta))
            {
                return new Bitmap(original);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics dibujo = e.Graphics;
            dibujo.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle mesa = new Rectangle(
                40,
                100,
                ClientSize.Width - 80,
                ClientSize.Height - 260
            );

            using (GraphicsPath formaMesa = new GraphicsPath())
            {
                formaMesa.AddEllipse(mesa);

                using (PathGradientBrush fieltro =
                    new PathGradientBrush(formaMesa))
                {
                    fieltro.CenterColor = FieltroCentro;
                    fieltro.SurroundColors =
                        new Color[] { FieltroOrilla };

                    dibujo.FillPath(fieltro, formaMesa);
                }
            }

            using (Pen borde = new Pen(Madera, 18))
            {
                dibujo.DrawEllipse(borde, mesa);
            }
        }
    }
}