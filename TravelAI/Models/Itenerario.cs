namespace TravelAI.Models
{
    public class Itenerario
    {
        public Guid Id { get; set; }
        public int Versao { get; set; }
        public DateTime CriadoEm { get; set; }

        public Guid ViagemId { get; set; }
        public Viagem? Viagem { get; set; }

        public List<DiaItenerario> Dias { get; set; } = new();
    }
}
