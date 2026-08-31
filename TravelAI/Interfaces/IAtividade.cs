using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IAtividade
    {
        Task<IEnumerable<AtividadeResponseDto>> ObterPorDiaIdAsync(Guid diaId);
        Task<AtividadeResponseDto> AdicionarAsync(Guid diaId, AtividadeCreateDto dto);
        Task<AtividadeResponseDto?> AtualizarAsync(Guid atividadeId, AtividadeUpdateDto dto);
        Task<bool> RemoverAsync(Guid atividadeId);
        Task<bool> ReordenarAsync(Guid diaId, ReordenarAtividadesDto dto);
    }
}
