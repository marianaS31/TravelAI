using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IAtividadeService
    {
        Task<IEnumerable<AtividadeResponseDTO>> ObterPorDiaIdAsync(Guid diaId);
        Task<AtividadeResponseDTO> AdicionarAsync(Guid diaId, AtividadeCreateDTO dto);
        Task<AtividadeResponseDTO?> AtualizarAsync(Guid atividadeId, AtividadeUpdateDTO dto);
        Task<bool> RemoverAsync(Guid atividadeId);
        Task<bool> ReordenarAsync(Guid diaId, ReordenarAtividadesDTO dto);
    }
}
