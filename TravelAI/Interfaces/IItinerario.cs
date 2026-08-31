using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IItinerario
    {
        // Devolve o itinerário de versão mais alta para a viagem
        Task<ItinerarioResponseDto?> ObterAtualPorViagemIdAsync(Guid viagemId);

        // Devolve todas as versões (histórico)
        Task<IEnumerable<ItinerarioResponseDto>> ObterHistoricoPorViagemIdAsync(Guid viagemId);

        Task<ItinerarioResponseDto?> ObterPorIdAsync(Guid itinerarioId);

        // Gera uma NOVA versão (v+1) via MCP + Gemma, com fallback determinístico
        Task<ItinerarioResponseDto> GerarNovaVersaoAsync(GerarItinerarioRequestDto dto);

        Task<bool> RemoverAsync(Guid itinerarioId);
   
    }
}
