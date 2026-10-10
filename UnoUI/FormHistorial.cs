using UnoLogica;

namespace UnoUI
{
    // Ventana que muestra cuántas partidas ha ganado y perdido cada jugador
    public class FormHistorial : Form
    {
        private readonly ApiCliente api;
        private readonly DataGridView tabla = new DataGridView();
        private readonly Label lblEstado = new Label();

        private static readonly Color Fondo = Color.FromArgb(30, 33, 38);
        private static readonly Color Fila = Color.FromArgb(40, 44, 52);
        private static readonly Color Dorado = Color.FromArgb(255, 200, 60);

        public FormHistorial(ApiCliente api)
        {
            this.api = api;

            Text = "Historial de partidas";
            ClientSize = new Size(560, 390);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Fondo;

            var lblTitulo = new Label
            {
                Text = "Historial de partidas",
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold)
            };

            CrearTabla();

            lblEstado.Location = new Point(20, 300);
            lblEstado.AutoSize = true;
            lblEstado.ForeColor = Color.FromArgb(200, 200, 210);
            lblEstado.Font = new Font("Segoe UI", 10);

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(440, 330),
                Size = new Size(100, 40),
                BackColor = Dorado,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lblTitulo, tabla, lblEstado, btnCerrar });

            // Cuando la ventana ya se ve, se piden los datos a la base de datos
            Shown += async (s, e) => await CargarAsync();
        }

        // Crea la tabla con sus columnas y colores
        private void CrearTabla()
        {
            tabla.Location = new Point(20, 65);
            tabla.Size = new Size(520, 220);
            tabla.ReadOnly = true;                 // no se puede editar
            tabla.AllowUserToAddRows = false;      // sin la fila vacía del final
            tabla.AllowUserToDeleteRows = false;
            tabla.AllowUserToResizeRows = false;
            tabla.RowHeadersVisible = false;       // sin la columna gris de la izquierda
            tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabla.BackgroundColor = Fondo;
            tabla.BorderStyle = BorderStyle.None;
            tabla.GridColor = Color.FromArgb(60, 64, 72);
            tabla.RowTemplate.Height = 34;

            // Encabezados dorados
            tabla.EnableHeadersVisualStyles = false;   // necesario para poder cambiarles el color
            tabla.ColumnHeadersHeight = 38;
            tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            tabla.ColumnHeadersDefaultCellStyle.BackColor = Dorado;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            tabla.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Filas oscuras
            tabla.DefaultCellStyle.BackColor = Fila;
            tabla.DefaultCellStyle.ForeColor = Color.White;
            tabla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 75, 85);
            tabla.DefaultCellStyle.SelectionForeColor = Color.White;
            tabla.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            tabla.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            tabla.Columns.Add("jugador", "Jugador");
            tabla.Columns.Add("ganadas", "Ganadas");
            tabla.Columns.Add("perdidas", "Perdidas");
            tabla.Columns.Add("jugadas", "Jugadas");
            tabla.Columns.Add("porcentaje", "% Victorias");

            tabla.Columns["jugador"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            tabla.Columns["jugador"]!.FillWeight = 140;   // la columna del nombre, más ancha
        }

        // Pide las estadísticas de cada jugador a la API y llena la tabla
        private async Task CargarAsync()
        {
            lblEstado.Text = "Cargando historial...";

            try
            {
                List<JugadorDto> jugadores = await api.ObtenerJugadoresAsync();

                var estadisticas = new List<EstadisticasDto>();
                foreach (JugadorDto jugador in jugadores)
                {
                    EstadisticasDto? datos = await api.ObtenerEstadisticasAsync(jugador.Id);
                    if (datos != null)
                        estadisticas.Add(datos);
                }

                // Primero el que más ha ganado; si empatan, el que menos ha perdido
                estadisticas = estadisticas
                    .OrderByDescending(e => e.Ganadas)
                    .ThenBy(e => e.Perdidas)
                    .ToList();

                tabla.Rows.Clear();
                foreach (EstadisticasDto e in estadisticas)
                {
                    int jugadas = e.Ganadas + e.Perdidas;
                    string porcentaje = jugadas == 0 ? "—" : $"{e.Ganadas * 100 / jugadas}%";
                    tabla.Rows.Add(e.Nombre, e.Ganadas, e.Perdidas, jugadas, porcentaje);
                }

                // El primer lugar con estrella dorada (si ya ganó al menos una)
                if (estadisticas.Count > 0 && estadisticas[0].Ganadas > 0)
                {
                    DataGridViewRow primero = tabla.Rows[0];
                    primero.Cells["jugador"].Value = "★ " + estadisticas[0].Nombre;
                    primero.DefaultCellStyle.ForeColor = Dorado;
                    primero.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                }

                tabla.ClearSelection();

                // Cada partida terminada tiene exactamente un ganador
                int partidasTerminadas = estadisticas.Sum(e => e.Ganadas);
                lblEstado.Text = $"Partidas terminadas: {partidasTerminadas}";
            }
            catch (Exception)
            {
                lblEstado.Text = "No se pudo cargar el historial. ¿Está prendida la API?";
            }
        }
    }
}