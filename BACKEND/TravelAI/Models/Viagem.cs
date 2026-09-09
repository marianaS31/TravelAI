namespace TravelAI.Models
{
    public class Viagem
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int NumViajantes { get; set; }
        public decimal Orcamento { get; set; }
        public EstadoViagem Estado { get; set; } = EstadoViagem.Planeamento;   
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }

        public List<Itinerario> Itinerarios { get; set; } = new();
    }
}
