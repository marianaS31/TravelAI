using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IDiaItinerario
    {
        Task<IEnumerable<DiaItinerarioResponseDto>> ObterPorItinerarioIdAsync(Guid itinerarioId);
        Task<DiaItinerarioResponseDto?> ObterPorIdAsync(Guid diaId);
    }
}
