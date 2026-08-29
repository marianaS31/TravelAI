namespace TravelAI.Models
{
    public class DiaItinerario
    {
        public Guid Id { get; set; }
        public int NumeroDia { get; set; }
        public DateTime Data { get; set; }

        public Guid ItinerarioId { get; set; }
        public Itinerario? Itinerario { get; set; }

        public List<Atividade> Atividades { get; set; } = new();
        public PrevisaoTempo? PrevisaoTempo { get; set; }
    }
}
