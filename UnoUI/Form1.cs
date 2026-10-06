using UnoLogica;

namespace UnoUI
{
    public partial class Form1 : Form
    {
        // ===== Estado del juego =====
        private Partida partida = null!;

        // ===== Controles de la mesa =====
        private readonly FlowLayoutPanel[] panelesManos = new FlowLayoutPanel[3];
        private readonly Label[] etiquetasNombres = new Label[3];
        private readonly PictureBox picMazo = new PictureBox();
        private readonly PictureBox picDescarte = new PictureBox();
        private readonly Label lblMazo = new Label();
        private readonly Label lblTurno = new Label();
        private readonly Label lblSentido = new Label();
        private readonly Panel panelColor = new Panel();
        private readonly Button btnUno = new Button();

        // ===== Imágenes ya cargadas (para no leer el disco cada vez) =====
        private readonly Dictionary<string, Image> imagenes = new Dictionary<string, Image>();

        private const int AnchoCarta = 70;
        private const int AltoCarta = 105;

        private static readonly Color VerdeMesa = Color.FromArgb(20, 90, 50);
        private static readonly Color VerdePanel = Color.FromArgb(15, 70, 40);
        private static readonly Color VerdeTurno = Color.FromArgb(40, 120, 70);

        public Form1()
        {
            InitializeComponent();
            CrearMesa();
            IniciarPartidaDePrueba();
        }

        // ---------------------------------------------------------------
        // Crea todos los controles de la ventana (solo se llama una vez)
        // ---------------------------------------------------------------
        private void CrearMesa()
        {
            Text = "UNO";
            ClientSize = new Size(1240, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = VerdeMesa;

            // Un renglón por jugador: nombre arriba y sus cartas abajo
            for (int i = 0; i < 3; i++)
            {
                int y = 20 + i * 175;

                etiquetasNombres[i] = new Label
                {
                    Location = new Point(20, y),
                    AutoSize = true,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold)
                };

                panelesManos[i] = new FlowLayoutPanel
                {
                    Location = new Point(20, y + 28),
                    Size = new Size(1200, 140),
                    AutoScroll = true,      // barra para desplazarse si hay muchas cartas
                    WrapContents = false,   // todas las cartas en una sola fila
                    BackColor = VerdePanel
                };

                Controls.Add(etiquetasNombres[i]);
                Controls.Add(panelesManos[i]);
            }

            // ----- Centro de la mesa -----
            picMazo.Location = new Point(400, 555);
            picMazo.Size = new Size(110, 165);
            picMazo.SizeMode = PictureBoxSizeMode.Zoom;
            picMazo.Image = ObtenerImagen("reverso.png");
            picMazo.Cursor = Cursors.Hand;

            lblMazo.Location = new Point(400, 725);
            lblMazo.AutoSize = true;
            lblMazo.ForeColor = Color.White;
            lblMazo.Font = new Font("Segoe UI", 10);

            picDescarte.Location = new Point(540, 555);
            picDescarte.Size = new Size(110, 165);
            picDescarte.SizeMode = PictureBoxSizeMode.Zoom;

            panelColor.Location = new Point(700, 565);
            panelColor.Size = new Size(60, 60);
            panelColor.BorderStyle = BorderStyle.FixedSingle;

            lblTurno.Location = new Point(780, 565);
            lblTurno.AutoSize = true;
            lblTurno.ForeColor = Color.White;
            lblTurno.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            lblSentido.Location = new Point(780, 605);
            lblSentido.AutoSize = true;
            lblSentido.ForeColor = Color.White;
            lblSentido.Font = new Font("Segoe UI", 11);

            btnUno.Location = new Point(780, 650);
            btnUno.Size = new Size(140, 50);
            btnUno.Text = "UNO";
            btnUno.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnUno.BackColor = Color.Gold;
            btnUno.FlatStyle = FlatStyle.Flat;

            Controls.Add(picMazo);
            Controls.Add(lblMazo);
            Controls.Add(picDescarte);
            Controls.Add(panelColor);
            Controls.Add(lblTurno);
            Controls.Add(lblSentido);
            Controls.Add(btnUno);
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
            partida.Iniciar();   // baraja, reparte 7 a cada uno y voltea la primera carta
            ActualizarPantalla();
        }

        // ---------------------------------------------------------------
        // Redibuja toda la mesa leyendo el estado de la partida
        // ---------------------------------------------------------------
        private void ActualizarPantalla()
        {
            for (int i = 0; i < 3; i++)
            {
                Jugador jugador = partida.Jugadores[i];
                bool enTurno = i == partida.TurnoActual;

                etiquetasNombres[i].Text = enTurno
                    ? $"▶ {jugador.Nombre}  ({jugador.Mano.Count} cartas)  —  EN TURNO"
                    : $"{jugador.Nombre}  ({jugador.Mano.Count} cartas)";
                etiquetasNombres[i].ForeColor = enTurno ? Color.Gold : Color.White;
                panelesManos[i].BackColor = enTurno ? VerdeTurno : VerdePanel;

                MostrarMano(jugador, panelesManos[i]);
            }

            picDescarte.Image = ObtenerImagen(partida.CartaArriba().NombreImagen());
            lblMazo.Text = $"Mazo: {partida.Mazo.CantidadCartas} cartas";
            panelColor.BackColor = ColorDePantalla(partida.ColorActual);
            lblTurno.Text = $"Turno de: {partida.JugadorEnTurno.Nombre}";
            lblSentido.Text = partida.SentidoHorario ? "Sentido: ↓ normal" : "Sentido: ↑ en reversa";
        }

        // Pone un PictureBox por cada carta de la mano del jugador
        private void MostrarMano(Jugador jugador, FlowLayoutPanel panel)
        {
            panel.SuspendLayout();   // pausa el dibujo mientras cambiamos todo

            // Quitar las cartas que había antes
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
                    Size = new Size(AnchoCarta, AltoCarta),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = ObtenerImagen(carta.NombreImagen()),
                    Tag = carta,             // guardamos la carta para saber cuál se tocó (fase 2)
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

        // Convierte el color del juego a un color de pantalla
        private static Color ColorDePantalla(ColorCarta color)
        {
            return color switch
            {
                ColorCarta.Rojo => Color.Red,
                ColorCarta.Amarillo => Color.Gold,
                ColorCarta.Verde => Color.LimeGreen,
                ColorCarta.Azul => Color.DodgerBlue,
                _ => Color.Black
            };
        }
    }
}