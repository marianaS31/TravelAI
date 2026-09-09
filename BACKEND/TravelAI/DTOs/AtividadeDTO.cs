using TravelAI.Models;

namespace TravelAI.DTOs
{
    public class AtividadeCreateDTO
    {
        public string Nome { get; set; } = string.Empty;
        public TipoAtividade Tipo { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
    }

    public class AtividadeUpdateDTO
    {
        public string? Nome { get; set; }
        public TipoAtividade? Tipo { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFim { get; set; }
        public string? Local { get; set; }
        public string? Detalhes { get; set; }
    }

    public class AtividadeResponseDTO
    {
        public Guid Id { get; set; }
        public int Ordem { get; set; }
        public string Nome { get; set; } = string.Empty;
        public TipoAtividade Tipo { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
    }

    public class ReordenarAtividadesDTO
    {
        public List<Guid> OrdemAtividadeIds { get; set; } = new List<Guid>();
    }
}