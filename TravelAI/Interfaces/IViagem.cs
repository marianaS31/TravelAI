using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IViagem
    {
     
        Task<ViagemResponseDto?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<ViagemResponseDto>> ObterTodasAsync();
        Task<ViagemResponseDto> CriarAsync(ViagemCreateDto dto);
        Task<ViagemResponseDto?> AtualizarAsync(Guid id, ViagemUpdateDto dto);
        Task<bool> RemoverAsync(Guid id);
 
    }
}
