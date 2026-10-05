namespace UnoLogica
{
    public class Jugador
    {
        public int Id { get; }
        public string Nombre { get; }
        public List<Carta> Mano { get; } = new List<Carta>();
        public bool DijoUno { get; set; }

        public Jugador(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public void RecibirCarta(Carta carta)
        {
            Mano.Add(carta);
        }

        public void QuitarCarta(Carta carta)
        {
            Mano.Remove(carta);
        }

        public bool SinCartas()
        {
            return Mano.Count == 0;
        }
    }
}