namespace UnoLogica
{
    public class Mazo
    {
        private List<Carta> cartas = new List<Carta>();

        public int CantidadCartas => cartas.Count;

        // Crea las 108 cartas oficiales del UNO
        public void Crear()
        {
            throw new NotImplementedException();
        }

        // Revuelve las cartas
        public void Barajar()
        {
            throw new NotImplementedException();
        }

        // Saca la carta de arriba del mazo
        public Carta Robar()
        {
            throw new NotImplementedException();
        }

        // Cuando se acaba el mazo, se rellena con la pila de descarte (menos la carta de arriba)
        public void Rellenar(List<Carta> descarte)
        {
            throw new NotImplementedException();
        }
    }
}