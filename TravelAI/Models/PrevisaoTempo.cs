namespace TravelAI.Models
{
    public class PrevisaoTempo
    {
        public Guid Id { get; set; }
        public float TempMax { get; set; }
        public float TempMin { get; set; }
        public string Condicao { get; set; } = string.Empty;
        public float ProbabilidadePrecipitacao { get; set; }

        public Guid DiaItenerarioId { get; set; }
        public DiaItenerario? DiaItenerario { get; set; }
    }
}
