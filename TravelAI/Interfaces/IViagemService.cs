// IViagem.cs
using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IViagemService
    {
        Task<ViagemResponseDTO?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<ViagemResponseDTO>> ObterTodasAsync();
        Task<ViagemResponseDTO> CriarAsync(ViagemCreateDTO dto);
        Task<ViagemResponseDTO?> AtualizarAsync(Guid id, ViagemUpdateDTO dto);
        Task<bool> RemoverAsync(Guid id);
    }
}