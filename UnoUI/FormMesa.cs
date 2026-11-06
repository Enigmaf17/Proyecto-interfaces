using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UnoRed;

namespace UnoUI
{
    public class FormMesa : Form
    {
        private static readonly Color FondoOrilla =
            Color.FromArgb(14, 15, 18);

        private static readonly Color PanelFondo =
            Color.FromArgb(30, 33, 38);

        private static readonly Color FieltroCentro =
            Color.FromArgb(40, 140, 80);

        private static readonly Color FieltroOrilla =
            Color.FromArgb(18, 90, 50);

        private static readonly Color Madera =
            Color.FromArgb(105, 65, 30);

        private static readonly Color Dorado =
            Color.FromArgb(255, 200, 60);

        private static readonly Color TextoClaro =
            Color.FromArgb(225, 225, 230);

        private VistaJugador? vistaActual;
        private bool partidaTerminada;

        private readonly FlowLayoutPanel panelMano = new();
        private readonly FlowLayoutPanel panelBotones = new();

        private readonly Label etiquetaTurno = new();
        private readonly Label etiquetaMovimiento = new();
        private readonly Label etiquetaAviso = new();

        private readonly Button botonPasar = new();
        private readonly Button botonUno = new();
        private readonly Button botonHistorial = new();

        private readonly Dictionary<string, Image> imagenes = new();

        private Rectangle zonaMazo;

        // Paul se suscribe a estos eventos.
        public event Action<int>? CartaClic;
        public event EventHandler? MazoClic;
        public event EventHandler? PasarClic;
        public event EventHandler? UnoClic;
        public event EventHandler? HistorialClic;

        private bool EsMiTurno =>
            !partidaTerminada &&
            vistaActual != null &&
            vistaActual.TurnoId == vistaActual.MiId;

        public FormMesa()
        {
            Text = "UNO - Mesa en red";
            ClientSize = new Size(1100, 800);
            MinimumSize = new Size(950, 780);
            StartPosition = FormStartPosition.CenterScreen;

            BackColor = FondoOrilla;
            ForeColor = TextoClaro;
            Font = new System.Drawing.Font("Segoe UI", 10);

            DoubleBuffered = true;
            ResizeRedraw = true;
            AutoScaleMode = AutoScaleMode.Dpi;

            ConfigurarEtiqueta(etiquetaTurno, 35, Dorado);
            ConfigurarEtiqueta(etiquetaMovimiento, 28, TextoClaro);
            ConfigurarEtiqueta(etiquetaAviso, 28, Dorado);

            etiquetaTurno.Text = "Esperando datos de la partida";

            panelMano.Dock = DockStyle.Bottom;
            panelMano.Height = 165;
            panelMano.Padding = new Padding(12, 8, 12, 8);
            panelMano.BackColor = FondoOrilla;
            panelMano.AutoScroll = true;
            panelMano.WrapContents = false;

            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Height = 50;
            panelBotones.BackColor = PanelFondo;
            panelBotones.Padding = new Padding(8, 4, 8, 4);
            panelBotones.WrapContents = false;

            ConfigurarBoton(botonPasar, "Pasar");
            ConfigurarBoton(botonUno, "UNO");
            ConfigurarBoton(botonHistorial, "Historial");

            botonPasar.Visible = false;
            botonUno.Enabled = false;

            botonPasar.Click += (_, _) =>
            {
                if (EsMiTurno && vistaActual?.CartaRobada != null)
                {
                    PasarClic?.Invoke(this, EventArgs.Empty);
                }
            };

            botonUno.Click += (_, _) =>
            {
                if (EsMiTurno)
                {
                    UnoClic?.Invoke(this, EventArgs.Empty);
                }
            };

            botonHistorial.Click += (_, _) =>
            {
                HistorialClic?.Invoke(this, EventArgs.Empty);
            };

            panelBotones.Controls.AddRange(new Control[]
            {
                botonPasar,
                botonUno,
                botonHistorial
            });

            Controls.Add(panelMano);
            Controls.Add(panelBotones);
            Controls.Add(etiquetaAviso);
            Controls.Add(etiquetaMovimiento);
            Controls.Add(etiquetaTurno);
        }

        private static void ConfigurarEtiqueta(
            Label etiqueta,
            int alto,
            Color color)
        {
            etiqueta.Dock = DockStyle.Top;
            etiqueta.Height = alto;
            etiqueta.TextAlign = ContentAlignment.MiddleCenter;
            etiqueta.ForeColor = color;
            etiqueta.BackColor = FondoOrilla;
            etiqueta.AutoEllipsis = true;
        }

        private static void ConfigurarBoton(
            Button boton,
            string texto)
        {
            boton.Text = texto;
            boton.Size = new Size(120, 36);
            boton.Margin = new Padding(5, 2, 5, 2);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = Dorado;
            boton.BackColor = PanelFondo;
            boton.ForeColor = TextoClaro;
        }

        public void MostrarVista(VistaJugador vista)
        {
            // Las actualizaciones de controles van en el hilo de la ventana.
            if (InvokeRequired)
            {
                Invoke(new Action(() => MostrarVista(vista)));
                return;
            }

            vistaActual = vista;

            JugadorVisible? jugadorTurno = vista.Jugadores
                .FirstOrDefault(j => j.Id == vista.TurnoId);

            etiquetaTurno.Text = vista.TurnoId == vista.MiId
                ? "Tu turno"
                : $"Turno de {jugadorTurno?.Nombre ?? "jugador"}";

            etiquetaMovimiento.Text =
                vista.UltimoMovimiento ?? "";

            etiquetaAviso.Text = vista.CartaRobada != null
                ? "Puedes tirar la carta que robaste o pasar"
                : "";

            botonPasar.Visible =
                !partidaTerminada && vista.CartaRobada != null;

            botonPasar.Enabled = EsMiTurno;
            botonUno.Enabled = EsMiTurno;

            MostrarMiMano();
            Invalidate();
        }

        private void MostrarMiMano()
        {
            panelMano.SuspendLayout();

            try
            {
                while (panelMano.Controls.Count > 0)
                {
                    Control anterior = panelMano.Controls[0];
                    panelMano.Controls.Remove(anterior);
                    anterior.Dispose();
                }

                if (vistaActual == null)
                {
                    return;
                }

                for (int i = 0; i < vistaActual.MiMano.Count; i++)
                {
                    // Guardar el índice correspondiente a esta carta.
                    int indiceCarta = i;
                    string nombreCarta = vistaActual.MiMano[i];

                    PictureBox carta = new PictureBox
                    {
                        Width = 85,
                        Height = 125,
                        Margin = new Padding(5),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = EsMiTurno
                            ? Dorado
                            : PanelFondo,
                        Padding = new Padding(3),
                        Image = CargarImagen(nombreCarta),
                        AccessibleName = nombreCarta,
                        Enabled = EsMiTurno,
                        Cursor = Cursors.Hand
                    };

                    carta.Click += (_, _) =>
                    {
                        if (EsMiTurno)
                        {
                            CartaClic?.Invoke(indiceCarta);
                        }
                    };

                    panelMano.Controls.Add(carta);
                }
            }
            finally
            {
                panelMano.ResumeLayout();
            }
        }

        private Image? CargarImagen(string nombreCarta)
        {
            if (imagenes.TryGetValue(nombreCarta, out Image? guardada))
            {
                return guardada;
            }

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
                Image copia = new Bitmap(original);
                imagenes.Add(nombreCarta, copia);
                return copia;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics dibujo = e.Graphics;
            dibujo.SmoothingMode = SmoothingMode.AntiAlias;

            int inicioMesa = etiquetaAviso.Bottom + 120;
            int finMesa = panelBotones.Top - 16;

            Rectangle mesa = new Rectangle(
                35,
                inicioMesa,
                ClientSize.Width - 70,
                Math.Max(100, finMesa - inicioMesa)
            );

            using (GraphicsPath forma = new GraphicsPath())
            {
                forma.AddEllipse(mesa);

                using (PathGradientBrush fieltro =
                    new PathGradientBrush(forma))
                {
                    fieltro.CenterColor = FieltroCentro;
                    fieltro.SurroundColors =
                        new Color[] { FieltroOrilla };

                    dibujo.FillPath(fieltro, forma);
                }
            }

            using (Pen borde = new Pen(Madera, 18))
            {
                dibujo.DrawEllipse(borde, mesa);
            }

            zonaMazo = Rectangle.Empty;

            if (vistaActual == null)
            {
                return;
            }

            Rectangle anillo = Rectangle.Inflate(mesa, -22, -22);

            using (Pen pincelColor =
                new Pen(ObtenerColor(vistaActual.ColorActual), 6))
            {
                dibujo.DrawEllipse(pincelColor, anillo);
            }

            DibujarRivales(dibujo);

            int centroX = mesa.Left + mesa.Width / 2;
            int centroY = mesa.Top + mesa.Height / 2;

            zonaMazo = new Rectangle(
                centroX - 120,
                centroY - 65,
                85,
                125
            );

            Rectangle descarte = new Rectangle(
                centroX + 35,
                centroY - 65,
                85,
                125
            );

            DibujarCarta(dibujo, "reverso", zonaMazo);
            DibujarCarta(dibujo, vistaActual.CartaArriba, descarte);

            DibujarTexto(
                dibujo,
                $"Mazo: {vistaActual.CartasEnMazo}",
                new Rectangle(zonaMazo.Left - 25, zonaMazo.Bottom + 5, 135, 25),
                TextoClaro
            );

            DibujarTexto(
                dibujo,
                "Carta de arriba",
                new Rectangle(descarte.Left - 25, descarte.Bottom + 5, 135, 25),
                TextoClaro
            );

            DibujarTexto(
                dibujo,
                $"Color: {vistaActual.ColorActual}",
                new Rectangle(centroX - 150, centroY - 103, 300, 28),
                ObtenerColor(vistaActual.ColorActual)
            );
        }

        private void DibujarRivales(Graphics dibujo)
        {
            if (vistaActual == null)
            {
                return;
            }

            List<JugadorVisible> rivales = vistaActual.Jugadores
                .Where(j => j.Id != vistaActual.MiId)
                .ToList();

            int anchoZona = (ClientSize.Width - 90) / 2;
            int y = etiquetaAviso.Bottom + 5;

            for (int i = 0; i < rivales.Count; i++)
            {
                JugadorVisible rival = rivales[i];
                int x = 30 + i * (anchoZona + 30);

                Color colorNombre = rival.Id == vistaActual.TurnoId
                    ? Dorado
                    : TextoClaro;

                DibujarTexto(
                    dibujo,
                    $"{rival.Nombre} — {rival.Cartas} cartas",
                    new Rectangle(x, y, anchoZona, 26),
                    colorNombre
                );

                int cantidad = Math.Max(0, rival.Cartas);
                int separacion = 22;

                if (cantidad > 1)
                {
                    separacion = Math.Min(
                        22,
                        Math.Max(0, (anchoZona - 50) / (cantidad - 1))
                    );
                }

                int anchoCartas = cantidad == 0
                    ? 0
                    : 50 + (cantidad - 1) * separacion;

                int inicioX = x + (anchoZona - anchoCartas) / 2;

                for (int carta = 0; carta < cantidad; carta++)
                {
                    DibujarCarta(
                        dibujo,
                        "reverso",
                        new Rectangle(
                            inicioX + carta * separacion,
                            y + 30,
                            50,
                            72
                        )
                    );
                }
            }
        }

        private void DibujarCarta(
            Graphics dibujo,
            string nombre,
            Rectangle zona)
        {
            Image? imagen = CargarImagen(nombre);

            if (imagen != null)
            {
                dibujo.DrawImage(imagen, zona);
                return;
            }

            // Mostrar el nombre si falta el archivo de imagen.
            using (SolidBrush fondo = new SolidBrush(PanelFondo))
            {
                dibujo.FillRectangle(fondo, zona);
            }

            DibujarTexto(dibujo, nombre, zona, TextoClaro);
        }

        private void DibujarTexto(
            Graphics dibujo,
            string texto,
            Rectangle zona,
            Color color)
        {
            TextRenderer.DrawText(
                dibujo,
                texto,
                Font,
                zona,
                color,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix
            );
        }

        private static Color ObtenerColor(string nombre)
        {
            return nombre.ToLowerInvariant() switch
            {
                "rojo" => Color.FromArgb(235, 60, 60),
                "amarillo" => Color.FromArgb(250, 210, 50),
                "verde" => Color.FromArgb(90, 220, 120),
                "azul" => Color.FromArgb(60, 140, 240),
                _ => Color.Gray
            };
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left &&
                EsMiTurno &&
                zonaMazo.Contains(e.Location))
            {
                MazoClic?.Invoke(this, EventArgs.Empty);
            }
        }

        public void MostrarError(string texto)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => MostrarError(texto)));
                return;
            }

            MessageBox.Show(
                this,
                texto,
                "UNO",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        public void MostrarGanador(string nombre)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => MostrarGanador(nombre)));
                return;
            }

            DesactivarJuego();

            MessageBox.Show(
                this,
                $"¡Ganó {nombre}!",
                "Fin de la partida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        public void MostrarDesconexion(string nombre)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => MostrarDesconexion(nombre)));
                return;
            }

            DesactivarJuego();

            MessageBox.Show(
                this,
                $"{nombre} se desconectó. La partida se canceló.",
                "Partida cancelada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private void DesactivarJuego()
        {
            partidaTerminada = true;
            botonPasar.Visible = false;
            botonUno.Enabled = false;
            etiquetaAviso.Text = "Partida finalizada";

            MostrarMiMano();
            Invalidate();
        }

        public static VistaJugador VistaDeEjemplo()
        {
            return new VistaJugador
            {
                MiId = 2,

                MiMano = new List<string>
                {
                    "rojo_5",
                    "azul_salta",
                    "verde_3",
                    "amarillo_8",
                    "rojo_2",
                    "azul_9",
                    "comodin_mas4"
                },

                Jugadores = new List<JugadorVisible>
                {
                    new JugadorVisible
                    {
                        Id = 1,
                        Nombre = "Paul",
                        Cartas = 6
                    },
                    new JugadorVisible
                    {
                        Id = 2,
                        Nombre = "Rosa",
                        Cartas = 7
                    },
                    new JugadorVisible
                    {
                        Id = 3,
                        Nombre = "Dalton",
                        Cartas = 4
                    }
                },

                CartaArriba = "rojo_7",
                ColorActual = "Rojo",
                TurnoId = 2,
                CartasEnMazo = 90,
                CartaRobada = null,
                UltimoMovimiento = "Dalton jugó rojo_7"
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Image imagen in imagenes.Values)
                {
                    imagen.Dispose();
                }

                imagenes.Clear();
            }

            base.Dispose(disposing);
        }
    }
}