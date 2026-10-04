namespace UnoLogica
{
    public class Jugador
    {
        public int Id { get; }          // el mismo id que tiene en la base de datos
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
            throw new NotImplementedException();
        }

        public void QuitarCarta(Carta carta)
        {
            throw new NotImplementedException();
        }

        public bool SinCartas()
        {
            throw new NotImplementedException();
        }
    }
}