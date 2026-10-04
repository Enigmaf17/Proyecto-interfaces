using System.Net.Http.Json;

namespace UnoLogica
{
    // Datos que regresa la API
    public record JugadorDto(int Id, string Nombre);
    public record EstadisticasDto(int Id, string Nombre, int Ganadas, int Perdidas);
    record IdRespuesta(int Id);

    // Se comunica con la API de FastAPI para guardar todo en MySQL
    public class ApiCliente
    {
        private readonly HttpClient http;

        public ApiCliente(string urlBase = "http://127.0.0.1:8000")
        {
            http = new HttpClient { BaseAddress = new Uri(urlBase) };
        }

        // GET /jugadores
        public async Task<List<JugadorDto>> ObtenerJugadoresAsync()
        {
            var jugadores = await http.GetFromJsonAsync<List<JugadorDto>>("/jugadores");
            return jugadores ?? new List<JugadorDto>();
        }

        // POST /partidas  → regresa el id de la partida nueva
        public async Task<int> CrearPartidaAsync(List<int> idsJugadores)
        {
            var respuesta = await http.PostAsJsonAsync("/partidas", new { jugadores = idsJugadores });
            respuesta.EnsureSuccessStatusCode();
            var datos = await respuesta.Content.ReadFromJsonAsync<IdRespuesta>();
            return datos!.Id;
        }

        // POST /partidas/{id}/movimientos  → guarda un movimiento en el log
        public async Task RegistrarMovimientoAsync(int partidaId, int jugadorId, int turno,
            string accion, string? carta = null, string? colorElegido = null)
        {
            var movimiento = new
            {
                jugador_id = jugadorId,
                turno,
                accion,
                carta,
                color_elegido = colorElegido
            };
            var respuesta = await http.PostAsJsonAsync($"/partidas/{partidaId}/movimientos", movimiento);
            respuesta.EnsureSuccessStatusCode();
        }

        // PUT /partidas/{id}/finalizar
        public async Task FinalizarPartidaAsync(int partidaId, int ganadorId)
        {
            var respuesta = await http.PutAsJsonAsync($"/partidas/{partidaId}/finalizar", new { ganador_id = ganadorId });
            respuesta.EnsureSuccessStatusCode();
        }

        // GET /jugadores/{id}/estadisticas
        public async Task<EstadisticasDto?> ObtenerEstadisticasAsync(int jugadorId)
        {
            return await http.GetFromJsonAsync<EstadisticasDto>($"/jugadores/{jugadorId}/estadisticas");
        }
    }
}