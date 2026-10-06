using System.Drawing.Drawing2D;
using UnoLogica;

namespace UnoUI
{
    public partial class Form1 : Form
    {
        // ===== Estado del juego =====
        private Partida partida = null!;
        private ColorCarta colorMesa = ColorCarta.Verde;   // color con el que se pinta el óvalo

        // ===== Jugadores alrededor de la mesa =====
        // Jugador 0 = izquierda, 1 = arriba, 2 = derecha.
        // Así el sentido normal (0 → 1 → 2) gira como las manecillas del reloj.
        private readonly FlowLayoutPanel[] panelesManos = new FlowLayoutPanel[3];
        private readonly Label[] etiquetasNombres = new Label[3];
        private readonly Size[] tamanosCarta =
        {
            new Size(60, 90),    // izquierda
            new Size(70, 105),   // arriba
            new Size(60, 90)     // derecha
        };

        // ===== Centro de la mesa =====
        private readonly Label lblFlecha = new Label();
        private readonly PictureBox picMazo = new PictureBox();
        private readonly PictureBox picDescarte = new PictureBox();
        private readonly Label lblMazo = new Label();
        private readonly Label lblTurno = new Label();

        // ===== Abajo =====
        private readonly Button btnUno = new Button();

        // ===== Imágenes ya cargadas =====
        private readonly Dictionary<string, Image> imagenes = new Dictionary<string, Image>();

        // Óvalo de la mesa: x, y, ancho, alto
        private readonly Rectangle mesa = new Rectangle(220, 195, 840, 470);

        // ===== Colores =====
        private static readonly Color FondoArriba = Color.FromArgb(55, 35, 105);   // morado
        private static readonly Color FondoAbajo = Color.FromArgb(25, 85, 140);    // azul
        private static readonly Color PanelNormal = Color.FromArgb(80, 65, 150);
        private static readonly Color PanelTurno = Color.FromArgb(255, 200, 60);   // dorado
        private static readonly Color Dorado = Color.FromArgb(255, 215, 80);
        private static readonly Color Madera = Color.FromArgb(140, 85, 40);

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;   // evita parpadeos al redibujar
            CrearMesa();
            IniciarPartidaDePrueba();
        }

        // ---------------------------------------------------------------
        // Crea todos los controles de la ventana (solo se llama una vez)
        // ---------------------------------------------------------------
        private void CrearMesa()
        {
            Text = "UNO";
            ClientSize = new Size(1280, 760);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Paint += DibujarFondoYMesa;

            // ----- Jugadores -----
            CrearJugador(0, new Point(20, 70), new Rectangle(20, 100, 175, 560), variasFilas: true);     // izquierda
            CrearJugador(1, new Point(230, 12), new Rectangle(230, 42, 820, 140), variasFilas: false);   // arriba
            CrearJugador(2, new Point(1085, 70), new Rectangle(1085, 100, 175, 560), variasFilas: true); // derecha

            // ----- Flecha del sentido (centrada en la mesa) -----
            lblFlecha.AutoSize = false;
            lblFlecha.Location = new Point(mesa.Left, 215);
            lblFlecha.Size = new Size(mesa.Width, 70);
            lblFlecha.TextAlign = ContentAlignment.MiddleCenter;
            lblFlecha.Font = new Font("Segoe UI Symbol", 40, FontStyle.Bold);
            lblFlecha.BackColor = Color.Transparent;

            // ----- Mazo (de donde se roba) -----
            picMazo.Location = new Point(520, 300);
            picMazo.Size = new Size(100, 150);
            picMazo.SizeMode = PictureBoxSizeMode.Zoom;
            picMazo.BackColor = Color.Transparent;
            picMazo.Image = ObtenerImagen("reverso.png");
            picMazo.Cursor = Cursors.Hand;

            lblMazo.AutoSize = false;
            lblMazo.Location = new Point(510, 452);
            lblMazo.Size = new Size(120, 24);
            lblMazo.TextAlign = ContentAlignment.MiddleCenter;
            lblMazo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblMazo.BackColor = Color.Transparent;

            // ----- Carta de arriba del descarte -----
            picDescarte.Location = new Point(660, 300);
            picDescarte.Size = new Size(100, 150);
            picDescarte.SizeMode = PictureBoxSizeMode.Zoom;
            picDescarte.BackColor = Color.Transparent;

            // ----- Nombre del jugador en turno (grande, centrado) -----
            lblTurno.AutoSize = false;
            lblTurno.Location = new Point(mesa.Left, 495);
            lblTurno.Size = new Size(mesa.Width, 70);
            lblTurno.TextAlign = ContentAlignment.MiddleCenter;
            lblTurno.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTurno.BackColor = Color.Transparent;

            // ----- Botón UNO (abajo al centro) -----
            btnUno.Size = new Size(160, 55);
            btnUno.Location = new Point((ClientSize.Width - btnUno.Width) / 2, 685);
            btnUno.Text = "¡UNO!";
            btnUno.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            btnUno.ForeColor = Color.White;
            btnUno.BackColor = Color.FromArgb(235, 50, 50);   // rojo
            btnUno.FlatStyle = FlatStyle.Flat;
            btnUno.FlatAppearance.BorderColor = Dorado;
            btnUno.FlatAppearance.BorderSize = 3;
            btnUno.Cursor = Cursors.Hand;

            Controls.AddRange(new Control[]
            {
                lblFlecha, picMazo, lblMazo, picDescarte, lblTurno, btnUno
            });
        }

        // Crea el nombre y el panel de cartas de un jugador
        private void CrearJugador(int indice, Point posicionNombre, Rectangle zona, bool variasFilas)
        {
            etiquetasNombres[indice] = new Label
            {
                Location = posicionNombre,
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            panelesManos[indice] = new FlowLayoutPanel
            {
                Location = zona.Location,
                Size = zona.Size,
                AutoScroll = true,           // aparece una barra si no caben las cartas
                WrapContents = variasFilas,  // a los lados: varias filas; arriba: una sola fila
                BackColor = PanelNormal,
                Padding = new Padding(4)
            };

            Controls.Add(etiquetasNombres[indice]);
            Controls.Add(panelesManos[indice]);
        }

        // Dibuja el fondo con degradado y la mesa ovalada del color actual.
        // Windows llama a este método cada vez que repinta la ventana.
        private void DibujarFondoYMesa(object? sender, PaintEventArgs e)
        {
            if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
                return;   // la ventana está minimizada

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fondo: degradado de morado (arriba) a azul (abajo)
            using (var fondo = new LinearGradientBrush(ClientRectangle, FondoArriba, FondoAbajo,
                                                        LinearGradientMode.Vertical))
            {
                g.FillRectangle(fondo, ClientRectangle);
            }

            // Mesa: óvalo del color actual, borde de madera y un brillo interior
            using var relleno = new SolidBrush(ColorDeMesa(colorMesa));
            using var borde = new Pen(Madera, 14);
            using var brillo = new Pen(Color.FromArgb(90, Color.White), 3);

            g.FillEllipse(relleno, mesa);
            g.DrawEllipse(borde, mesa);

            Rectangle interior = mesa;
            interior.Inflate(-22, -22);   // un óvalo un poco más chico, para el brillo
            g.DrawEllipse(brillo, interior);
        }

        // ---------------------------------------------------------------
        // Partida de prueba (en la fase 3 se cambia por la base de datos)
        // ---------------------------------------------------------------
        private void IniciarPartidaDePrueba()
        {
            var jugadores = new List<Jugador>
            {
                new Jugador(1, "Paul"),
                new Jugador(2, "Rosa"),
                new Jugador(3, "Dalton")
            };

            partida = new Partida(jugadores);
            partida.Iniciar();
            ActualizarPantalla();
        }

        // ---------------------------------------------------------------
        // Redibuja toda la mesa leyendo el estado de la partida
        // ---------------------------------------------------------------
        private void ActualizarPantalla()
        {
            // Jugadores
            for (int i = 0; i < 3; i++)
            {
                Jugador jugador = partida.Jugadores[i];
                bool enTurno = i == partida.TurnoActual;

                etiquetasNombres[i].Text = enTurno
                    ? $"▶ {jugador.Nombre} ({jugador.Mano.Count})"
                    : $"{jugador.Nombre} ({jugador.Mano.Count})";
                etiquetasNombres[i].ForeColor = enTurno ? Dorado : Color.White;
                panelesManos[i].BackColor = enTurno ? PanelTurno : PanelNormal;

                MostrarMano(jugador, panelesManos[i], tamanosCarta[i]);
            }

            // Centro de la mesa
            colorMesa = partida.ColorActual;
            Color texto = ColorDeTexto(colorMesa);

            picDescarte.Image = ObtenerImagen(partida.CartaArriba().NombreImagen());
            lblMazo.Text = $"Mazo: {partida.Mazo.CantidadCartas}";
            lblMazo.ForeColor = texto;
            lblFlecha.Text = partida.SentidoHorario ? "↻" : "↺";
            lblFlecha.ForeColor = texto;
            lblTurno.Text = $"Turno de {partida.JugadorEnTurno.Nombre}";
            lblTurno.ForeColor = texto;

            Invalidate(true);   // repinta la ventana para que el óvalo tome el nuevo color
        }

        // Pone un PictureBox por cada carta de la mano del jugador
        private void MostrarMano(Jugador jugador, FlowLayoutPanel panel, Size tamano)
        {
            panel.SuspendLayout();

            while (panel.Controls.Count > 0)
            {
                Control anterior = panel.Controls[0];
                panel.Controls.RemoveAt(0);
                anterior.Dispose();
            }

            foreach (Carta carta in jugador.Mano)
            {
                var pic = new PictureBox
                {
                    Size = tamano,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = ObtenerImagen(carta.NombreImagen()),
                    Tag = carta,   // la carta, para saber cuál se tocó (fase 2)
                    Cursor = Cursors.Hand,
                    Margin = new Padding(3)
                };
                panel.Controls.Add(pic);
            }

            panel.ResumeLayout();
        }

        // Carga una imagen de la carpeta Imagenes (solo la primera vez)
        private Image ObtenerImagen(string nombreArchivo)
        {
            if (!imagenes.TryGetValue(nombreArchivo, out Image? imagen))
            {
                string ruta = Path.Combine(AppContext.BaseDirectory, "Imagenes", nombreArchivo);
                imagen = Image.FromFile(ruta);
                imagenes[nombreArchivo] = imagen;
            }
            return imagen;
        }

        // Color con el que se pinta la mesa según el color actual del juego
        private static Color ColorDeMesa(ColorCarta color)
        {
            return color switch
            {
                ColorCarta.Rojo => Color.FromArgb(230, 70, 70),
                ColorCarta.Amarillo => Color.FromArgb(245, 205, 50),
                ColorCarta.Verde => Color.FromArgb(60, 180, 95),
                ColorCarta.Azul => Color.FromArgb(50, 130, 230),
                _ => Color.FromArgb(60, 60, 60)
            };
        }

        // Sobre la mesa amarilla el texto blanco no se lee, así que ahí se usa oscuro
        private static Color ColorDeTexto(ColorCarta color)
        {
            return color == ColorCarta.Amarillo ? Color.FromArgb(50, 40, 20) : Color.White;
        }
    }
}