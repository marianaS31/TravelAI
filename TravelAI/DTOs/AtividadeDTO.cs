using TravelAI.Models;

namespace TravelAI.DTOs
{
    
    public class AtividadeCreateDTO(
        string Nome,
        TipoAtividade Tipo,
        string HoraInicio,
        string HoraFim,
        string Local,
        string? Detalhes
    );

    public class AtividadeUpdateDTO(
        string? Nome,
        TipoAtividade? Tipo,
        string? HoraInicio,
        string? HoraFim,
        string? Local,
        string? Detalhes
    );

    public class AtividadeResponseDTO(
        Guid Id,
        int Ordem,
        string Nome,
        TipoAtividade Tipo,
        string HoraInicio,
        string HoraFim,
        string Local,
        string? Detalhes
    );

    public class ReordenarAtividadesDTO(List<Guid> OrdemAtividadeIds);
}
