using System.Drawing.Drawing2D;
using UnoLogica;

namespace UnoUI
{
    public partial class Form1 : Form
    {
        // ===== Conexión con la base de datos =====
        private readonly ApiCliente api = new ApiCliente();
        private readonly ControladorPartida controlador;

        // Atajo: "partida" es la Partida que maneja el controlador.
        // Así el resto del código la lee igual que antes.
        private Partida partida => controlador.Partida;

        // ===== Estado de la ventana =====
        private ColorCarta colorActual = ColorCarta.Verde;   // color del anillo de la mesa
        private bool partidaActiva = false;   // false = todavía no empieza o ya terminó
        private bool ocupado = false;         // true mientras esperamos a la base de datos

        // ===== Jugadores alrededor de la mesa =====
        // Jugador 0 = izquierda, 1 = arriba, 2 = derecha.
        private readonly Panel[] marcos = new Panel[3];
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
        private readonly Label lblColor = new Label();
        private readonly Label lblTurno = new Label();
        private readonly Label lblMensaje = new Label();

        // ===== Abajo =====
        private readonly Button btnUno = new Button();
        private readonly Button btnHistorial = new Button();

        // ===== Imágenes ya cargadas =====
        private readonly Dictionary<string, Image> imagenes = new Dictionary<string, Image>();

        // Óvalo de la mesa: x, y, ancho, alto
        private readonly Rectangle mesa = new Rectangle(220, 195, 840, 470);

        // ===== Colores =====
        private static readonly Color FondoOrilla = Color.FromArgb(14, 15, 18);
        private static readonly Color FondoCentro = Color.FromArgb(40, 44, 50);
        private static readonly Color FieltroCentro = Color.FromArgb(40, 140, 80);
        private static readonly Color FieltroOrilla = Color.FromArgb(18, 90, 50);
        private static readonly Color Madera = Color.FromArgb(105, 65, 30);
        private static readonly Color PanelFondo = Color.FromArgb(30, 33, 38);
        private static readonly Color MarcoNormal = Color.FromArgb(60, 64, 72);
        private static readonly Color Dorado = Color.FromArgb(255, 200, 60);
        private static readonly Color TextoClaro = Color.FromArgb(225, 225, 230);

        public Form1()
        {
            InitializeComponent();
            Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "uno.ico"));
            DoubleBuffered = true;
            controlador = new ControladorPartida(api);
            CrearMesa();

            // Cuando la ventana ya se ve, empezamos la partida (necesita la base de datos)
            Shown += async (s, e) => await IniciarPartidaAsync();
        }

        // ===============================================================
        //  CREACIÓN DE LA MESA (solo se llama una vez)
        // ===============================================================
        private void CrearMesa()
        {
            Text = "UNO";
            ClientSize = new Size(1280, 760);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = FondoOrilla;
            Paint += DibujarFondoYMesa;

            CrearJugador(0, new Point(20, 70), new Rectangle(20, 100, 175, 560), variasFilas: true);
            CrearJugador(1, new Point(230, 12), new Rectangle(230, 42, 820, 140), variasFilas: false);
            CrearJugador(2, new Point(1085, 70), new Rectangle(1085, 100, 175, 560), variasFilas: true);

            lblFlecha.AutoSize = false;
            lblFlecha.Location = new Point(mesa.Left, 215);
            lblFlecha.Size = new Size(mesa.Width, 70);
            lblFlecha.TextAlign = ContentAlignment.MiddleCenter;
            lblFlecha.Font = new Font("Segoe UI Symbol", 40, FontStyle.Bold);
            lblFlecha.ForeColor = Dorado;
            lblFlecha.BackColor = Color.Transparent;

            picMazo.Location = new Point(520, 300);
            picMazo.Size = new Size(100, 150);
            picMazo.SizeMode = PictureBoxSizeMode.Zoom;
            picMazo.BackColor = Color.Transparent;
            picMazo.Image = ObtenerImagen("reverso.png");
            picMazo.Cursor = Cursors.Hand;
            picMazo.Click += ClicEnMazo;

            lblMazo.AutoSize = false;
            lblMazo.Location = new Point(510, 452);
            lblMazo.Size = new Size(120, 24);
            lblMazo.TextAlign = ContentAlignment.MiddleCenter;
            lblMazo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblMazo.ForeColor = TextoClaro;
            lblMazo.BackColor = Color.Transparent;

            picDescarte.Location = new Point(660, 300);
            picDescarte.Size = new Size(100, 150);
            picDescarte.SizeMode = PictureBoxSizeMode.Zoom;
            picDescarte.BackColor = Color.Transparent;

            lblColor.AutoSize = false;
            lblColor.Location = new Point(650, 452);
            lblColor.Size = new Size(120, 24);
            lblColor.TextAlign = ContentAlignment.MiddleCenter;
            lblColor.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblColor.ForeColor = TextoClaro;
            lblColor.BackColor = Color.Transparent;

            lblTurno.AutoSize = false;
            lblTurno.Location = new Point(mesa.Left, 495);
            lblTurno.Size = new Size(mesa.Width, 70);
            lblTurno.TextAlign = ContentAlignment.MiddleCenter;
            lblTurno.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTurno.ForeColor = Color.White;
            lblTurno.BackColor = Color.Transparent;

            lblMensaje.AutoSize = false;
            lblMensaje.Location = new Point(340, 568);
            lblMensaje.Size = new Size(600, 46);
            lblMensaje.TextAlign = ContentAlignment.TopCenter;
            lblMensaje.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblMensaje.ForeColor = TextoClaro;
            lblMensaje.BackColor = Color.Transparent;

            btnUno.Size = new Size(160, 55);
            btnUno.Location = new Point((ClientSize.Width - btnUno.Width) / 2, 690);
            btnUno.Text = "¡UNO!";
            btnUno.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            btnUno.ForeColor = Color.White;
            btnUno.BackColor = Color.FromArgb(210, 40, 40);
            btnUno.FlatStyle = FlatStyle.Flat;
            btnUno.FlatAppearance.BorderColor = Dorado;
            btnUno.FlatAppearance.BorderSize = 3;
            btnUno.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 60, 60);
            btnUno.Cursor = Cursors.Hand;
            btnUno.Click += ClicEnUno;

            // ----- Botón Historial (abajo a la izquierda) -----
            btnHistorial.Location = new Point(20, 690);
            btnHistorial.Size = new Size(175, 50);
            btnHistorial.Text = "Historial";
            btnHistorial.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            btnHistorial.ForeColor = Color.White;
            btnHistorial.BackColor = Color.FromArgb(45, 48, 56);
            btnHistorial.FlatStyle = FlatStyle.Flat;
            btnHistorial.FlatAppearance.BorderColor = Dorado;
            btnHistorial.FlatAppearance.BorderSize = 2;
            btnHistorial.Cursor = Cursors.Hand;
            btnHistorial.Click += ClicEnHistorial;

            Controls.AddRange(new Control[]
            {
                lblFlecha, picMazo, lblMazo, picDescarte, lblColor, lblTurno, lblMensaje, btnUno, btnHistorial
            });

        }

        private void CrearJugador(int indice, Point posicionNombre, Rectangle zona, bool variasFilas)
        {
            etiquetasNombres[indice] = new Label
            {
                Location = posicionNombre,
                AutoSize = true,
                ForeColor = TextoClaro,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            marcos[indice] = new Panel
            {
                Location = zona.Location,
                Size = zona.Size,
                Padding = new Padding(3),
                BackColor = MarcoNormal
            };

            panelesManos[indice] = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = variasFilas,
                BackColor = PanelFondo,
                Padding = new Padding(4)
            };

            marcos[indice].Controls.Add(panelesManos[indice]);
            Controls.Add(etiquetasNombres[indice]);
            Controls.Add(marcos[indice]);
        }

        private void DibujarFondoYMesa(object? sender, PaintEventArgs e)
        {
            if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var zonaLuz = new GraphicsPath())
            {
                zonaLuz.AddEllipse(-200, -150, ClientSize.Width + 400, ClientSize.Height + 300);
                using var luz = new PathGradientBrush(zonaLuz)
                {
                    CenterColor = FondoCentro,
                    SurroundColors = new[] { FondoOrilla }
                };
                g.FillPath(luz, zonaLuz);
            }

            Rectangle sombra = mesa;
            sombra.Offset(0, 12);
            using (var pincelSombra = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                g.FillEllipse(pincelSombra, sombra);

            using (var forma = new GraphicsPath())
            {
                forma.AddEllipse(mesa);
                using var fieltro = new PathGradientBrush(forma)
                {
                    CenterColor = FieltroCentro,
                    SurroundColors = new[] { FieltroOrilla }
                };
                g.FillPath(fieltro, forma);
            }

            using (var madera = new Pen(Madera, 16))
                g.DrawEllipse(madera, mesa);

            Rectangle anillo = mesa;
            anillo.Inflate(-15, -15);
            using (var pincelColor = new Pen(ColorDePantalla(colorActual), 7))
                g.DrawEllipse(pincelColor, anillo);
        }

        // ===============================================================
        //  INICIO DE LA PARTIDA (con la base de datos)
        // ===============================================================
        private async Task IniciarPartidaAsync()
        {
            partidaActiva = false;
            Exception? error = null;

            var carga = new FormCarga();
            carga.MostrarSobre(this);

            try
            {
                // Para que la pantalla de carga se alcance a ver aunque todo sea muy rápido
                Task tiempoMinimo = Task.Delay(1500);

                carga.MostrarEstado("Conectando con la base de datos...");
                List<JugadorDto> datos = await api.ObtenerJugadoresAsync();
                if (datos.Count < 3)
                    throw new Exception("La base de datos necesita al menos 3 jugadores.");

                carga.MostrarEstado("Repartiendo cartas...");
                List<Jugador> jugadores = datos
                    .Take(3)
                    .Select(d => new Jugador(d.Id, d.Nombre))
                    .ToList();

                await controlador.IniciarAsync(jugadores);
                if (controlador.PartidaId <= 0)
                    throw new Exception("La API no pudo crear la partida. ¿Está prendido MySQL?");

                carga.MostrarEstado("¡Listo!");
                await tiempoMinimo;
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                carga.Close();   // la pantalla de carga se cierra pase lo que pase
                carga.Dispose();
            }

            if (error != null)
            {
                DialogResult respuesta = MessageBox.Show(
                    "No se pudo conectar con la base de datos.\n\n" +
                    "Revisa que la API (uvicorn) y MySQL estén prendidos.\n\n" +
                    $"Detalle: {error.Message}",
                    "Error de conexión", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (respuesta == DialogResult.Retry)
                    await IniciarPartidaAsync();
                else
                    Close();
                return;
            }

            partidaActiva = true;
            Text = $"UNO — Partida #{controlador.PartidaId}";
            ActualizarPantalla();
            MostrarMensaje("¡Empieza la partida!");
        }

        // ===============================================================
        //  ACCIONES DEL JUGADOR
        // ===============================================================

        // ¿Se puede hacer algo ahorita? (hay partida y no estamos esperando a la base de datos)
        private bool PuedeActuar() => partidaActiva && !ocupado;

        // Ejecuta una acción del jugador: evita dobles clics mientras se guarda,
        // maneja los errores y al final revisa si alguien ganó.
        private async Task EjecutarAsync(Func<Task> accion)
        {
            ocupado = true;
            try
            {
                await accion();
            }
            catch (InvalidOperationException ex)
            {
                // Error de las reglas (jugada no válida)
                MostrarMensaje(ex.Message);
            }
            catch (Exception)
            {
                // Error de conexión: la jugada sí se hizo, pero no se pudo guardar
                ActualizarPantalla();
                MostrarMensaje("⚠ No se pudo guardar en la base de datos. ¿Sigue prendida la API?");
            }
            finally
            {
                ocupado = false;
            }

            await RevisarGanadorAsync();
        }

        private async void ClicEnCarta(object? sender, EventArgs e)
        {
            if (!PuedeActuar()) return;
            if (sender is not PictureBox pic || pic.Tag is not Carta carta) return;

            Jugador jugador = partida.JugadorEnTurno;
            if (!jugador.Mano.Contains(carta))
            {
                MostrarMensaje($"No es tu turno: le toca a {jugador.Nombre}.");
                return;
            }

            await EjecutarAsync(() => JugarCartaDelJugadorAsync(carta));
        }

        private async void ClicEnMazo(object? sender, EventArgs e)
        {
            if (!PuedeActuar()) return;
            await EjecutarAsync(RobarAsync);
        }

        private async void ClicEnUno(object? sender, EventArgs e)
        {
            if (!PuedeActuar()) return;

            Jugador jugador = partida.JugadorEnTurno;
            if (jugador.Mano.Count != 2)
            {
                MostrarMensaje("UNO se dice cuando te quedan 2 cartas, antes de tirar la penúltima.");
                return;
            }

            await EjecutarAsync(async () =>
            {
                await controlador.DecirUnoAsync();
                MostrarMensaje($"¡{jugador.Nombre} dijo UNO!");
            });
        }

        // Abre la ventana del historial de partidas
        private void ClicEnHistorial(object? sender, EventArgs e)
        {
            if (ocupado) return;   // no abrir mientras se está guardando una jugada

            using var ventana = new FormHistorial(api);
            ventana.ShowDialog(this);
        }

        // Robar del mazo; si la carta se puede jugar, preguntar si la tira
        private async Task RobarAsync()
        {
            Jugador jugador = partida.JugadorEnTurno;
            Carta robada = await controlador.RobarCartaAsync();
            ActualizarPantalla();

            if (partida.PuedeJugar(robada))
            {
                DialogResult respuesta = MessageBox.Show(
                    $"{jugador.Nombre}, robaste {Describir(robada)} y la puedes jugar.\n\n¿Quieres tirarla?",
                    "Carta robada", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                {
                    await JugarCartaDelJugadorAsync(robada);
                    return;
                }
            }

            await controlador.PasarTurnoAsync();
            ActualizarPantalla();
            MostrarMensaje($"{jugador.Nombre} robó una carta y pasó.");
        }

        // Juega una carta del jugador en turno (desde la mano o recién robada)
        private async Task JugarCartaDelJugadorAsync(Carta carta)
        {
            if (!partida.PuedeJugar(carta))
            {
                MostrarMensaje($"No puedes tirar {Describir(carta)} sobre {Describir(partida.CartaArriba())}.");
                return;
            }

            ColorCarta? colorElegido = EsComodin(carta) ? PedirColor() : null;

            // Guardamos esto ANTES de jugar, para saber si hubo castigo por no decir UNO
            Jugador jugador = partida.JugadorEnTurno;
            int cartasAntes = jugador.Mano.Count;
            bool dijoUno = jugador.DijoUno;

            // Aplica la regla Y guarda el movimiento en la base de datos
            await controlador.JugarCartaAsync(carta, colorElegido);

            string mensaje = $"{jugador.Nombre} jugó {Describir(carta)}";
            if (colorElegido != null)
                mensaje += $" y eligió {colorElegido.Value.ToString().ToLower()}";
            if (cartasAntes == 2 && !dijoUno)
                mensaje += ". ¡No dijo UNO! Roba 2 cartas";

            ActualizarPantalla();
            MostrarMensaje(mensaje);
        }

        private ColorCarta PedirColor()
        {
            using var ventana = new FormElegirColor();
            ventana.ShowDialog(this);
            return ventana.ColorElegido;
        }

        // Si alguien ganó, avisa y ofrece jugar otra vez.
        // (ControladorPartida ya guardó al ganador en la base de datos)
        private async Task RevisarGanadorAsync()
        {
            if (!partidaActiva || partida.Ganador == null) return;

            partidaActiva = false;
            ActualizarPantalla();
            lblTurno.Text = $"¡Ganó {partida.Ganador.Nombre}!";
            lblTurno.ForeColor = Dorado;

            DialogResult respuesta = MessageBox.Show(
                $"¡{partida.Ganador.Nombre} ganó la partida!\n\n" +
                "El resultado ya se guardó en la base de datos.\n\n¿Jugar otra vez?",
                "Fin de la partida", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (respuesta == DialogResult.Yes)
                await IniciarPartidaAsync();
        }

        // ===============================================================
        //  DIBUJO
        // ===============================================================
        private void ActualizarPantalla()
        {
            for (int i = 0; i < 3; i++)
            {
                Jugador jugador = partida.Jugadores[i];
                bool enTurno = i == partida.TurnoActual && partidaActiva;

                etiquetasNombres[i].Text = enTurno
                    ? $"▶ {jugador.Nombre} ({jugador.Mano.Count})"
                    : $"{jugador.Nombre} ({jugador.Mano.Count})";
                etiquetasNombres[i].ForeColor = enTurno ? Dorado : TextoClaro;
                marcos[i].BackColor = enTurno ? Dorado : MarcoNormal;

                MostrarMano(jugador, panelesManos[i], tamanosCarta[i], enTurno);
            }

            colorActual = partida.ColorActual;

            picDescarte.Image = ObtenerImagen(partida.CartaArriba().NombreImagen());
            lblMazo.Text = $"Mazo: {partida.Mazo.CantidadCartas}";
            lblColor.Text = $"Color: {colorActual.ToString().ToLower()}";
            lblFlecha.Text = partida.SentidoHorario ? "↻" : "↺";
            lblTurno.Text = $"Turno de {partida.JugadorEnTurno.Nombre}";
            lblTurno.ForeColor = Color.White;

            Invalidate(true);
        }

        private void MostrarMano(Jugador jugador, FlowLayoutPanel panel, Size tamano, bool enTurno)
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
                bool sePuedeJugar = enTurno && partida.PuedeJugar(carta);

                var pic = new PictureBox
                {
                    Size = tamano,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = ObtenerImagen(carta.NombreImagen()),
                    Tag = carta,
                    Cursor = enTurno ? Cursors.Hand : Cursors.Default,
                    Margin = new Padding(3),
                    Padding = sePuedeJugar ? new Padding(3) : new Padding(0),
                    BackColor = sePuedeJugar ? Color.White : Color.Transparent
                };
                pic.Click += ClicEnCarta;
                panel.Controls.Add(pic);
            }

            panel.ResumeLayout();
        }

        private void MostrarMensaje(string texto)
        {
            lblMensaje.Text = texto;
        }

        // ===============================================================
        //  AYUDANTES
        // ===============================================================
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

        private static string Describir(Carta carta)
        {
            string color = carta.Color.ToString().ToLower();
            return carta.Tipo switch
            {
                TipoCarta.Numero => $"{carta.Numero} {color}",
                TipoCarta.Salta => $"Salta {color}",
                TipoCarta.Reversa => $"Reversa {color}",
                TipoCarta.MasDos => $"+2 {color}",
                TipoCarta.Comodin => "Comodín",
                _ => "Comodín +4"
            };
        }

        private static bool EsComodin(Carta carta)
        {
            return carta.Tipo == TipoCarta.Comodin || carta.Tipo == TipoCarta.ComodinMasCuatro;
        }

        private static Color ColorDePantalla(ColorCarta color)
        {
            return color switch
            {
                ColorCarta.Rojo => Color.FromArgb(235, 60, 60),
                ColorCarta.Amarillo => Color.FromArgb(250, 210, 50),
                ColorCarta.Verde => Color.FromArgb(90, 220, 120),
                ColorCarta.Azul => Color.FromArgb(60, 140, 240),
                _ => Color.Gray
            };
        }
    }
}