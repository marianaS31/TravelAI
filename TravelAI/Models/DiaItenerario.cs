namespace TravelAI.Models
{
    public class DiaItenerario
    {
        public Guid Id { get; set; }
        public int NumeroDia { get; set; }
        public DateTime Data { get; set; }

        public Guid ItinerarioId { get; set; }
        public Itenerario? Itenerario { get; set; }

        public List<Atividade> Atividades { get; set; } = new();
        public PrevisaoTempo? PrevisaoTempo { get; set; }
    }
}
