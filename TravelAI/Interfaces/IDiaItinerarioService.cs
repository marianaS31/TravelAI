using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IDiaItinerarioService
    {
        Task<IEnumerable<DiaItinerarioResponseDTO>> ObterPorItinerarioIdAsync(Guid itinerarioId);
        Task<DiaItinerarioResponseDTO?> ObterPorIdAsync(Guid diaId);
    }
}
