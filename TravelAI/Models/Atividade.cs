
namespace TravelAI.Models
{
    public class Atividade
    {
        public Guid Id { get; set; }
        public int Ordem { get; set; }
        public string Nome { get; set; } = string.Empty;
        public TipoAtividade Tipo { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string? Detalhes { get; set; }

        public Guid DiaItenerarioId { get; set; }
        public DiaItenerario? DiaItenerario { get; set; }
    }
}
