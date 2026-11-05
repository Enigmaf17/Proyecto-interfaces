using Xunit;
using UnoRed;

namespace UnoPruebas
{
    public class ProtocoloTests
    {
        [Fact]
        public void SerializarYDeserializar_MensajeValido_FuncionaCorrectamente()
        {
            var mensajeOriginal = new Mensaje
            {
                Tipo = TiposMensaje.Jugar,
                JugadorId = 1,
                Color = "Rojo"
            };

            string json = Protocolo.Serializar(mensajeOriginal);
            var mensajeDeserializado = Protocolo.Deserializar(json);

            Assert.NotNull(mensajeDeserializado);
            Assert.Equal(mensajeOriginal.Tipo, mensajeDeserializado.Tipo);
            Assert.Equal(mensajeOriginal.JugadorId, mensajeDeserializado.JugadorId);
            Assert.Equal(mensajeOriginal.Color, mensajeDeserializado.Color);
        }

        [Fact]
        public void Serializar_CamposNullNoSeEscribenYUnaSolaLinea()
        {
            var mensaje = new Mensaje
            {
                Tipo = TiposMensaje.Robar
            };

            string json = Protocolo.Serializar(mensaje);

            Assert.DoesNotContain("\n", json);
            Assert.DoesNotContain("\r", json);
            Assert.Contains("tipo", json);
            Assert.DoesNotContain("jugadorId", json); // Los campos null se ignoran
        }

        [Fact]
        public void Deserializar_TextoInvalido_RegresaNull()
        {
            var resultado = Protocolo.Deserializar("esto no es un json valido");
            Assert.Null(resultado);
        }
    }
}