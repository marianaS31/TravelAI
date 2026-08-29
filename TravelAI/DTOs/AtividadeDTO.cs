using TravelAI.Models;

namespace TravelAI.DTOs
{
    
    public class AtividadeCreateDto(
        string Nome,
        TipoAtividade Tipo,
        string HoraInicio,
        string HoraFim,
        string Local,
        string? Detalhes
    );

    public class AtividadeUpdateDto(
        string? Nome,
        TipoAtividade? Tipo,
        string? HoraInicio,
        string? HoraFim,
        string? Local,
        string? Detalhes
    );

    public class AtividadeResponseDto(
        Guid Id,
        int Ordem,
        string Nome,
        TipoAtividade Tipo,
        string HoraInicio,
        string HoraFim,
        string Local,
        string? Detalhes
    );

    public class ReordenarAtividadesDto(List<Guid> OrdemAtividadeIds);
}
