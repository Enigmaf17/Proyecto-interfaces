using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnoRed
{
    public static class Protocolo
    {
        public const int Puerto = 5000;
        public static readonly UTF8Encoding Codificacion = new UTF8Encoding(false);

        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serializar(Mensaje m)
        {
            return JsonSerializer.Serialize(m, OpcionesJson);
        }

        public static Mensaje? Deserializar(string linea)
        {
            try
            {
                return JsonSerializer.Deserialize<Mensaje>(linea, OpcionesJson);
            }
            catch
            {
                return null;
            }
        }
    }
}