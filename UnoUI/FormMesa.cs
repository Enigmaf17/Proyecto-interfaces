using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UnoRed;
using static System.Net.Mime.MediaTypeNames;

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
        }

        public void MostrarVista(VistaJugador vista)
        {
            vistaActual = vista;
            Invalidate();
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