using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IItinerarioService
    {
        // Devolve o itinerário de versão mais alta para a viagem
        Task<ItinerarioResponseDTO?> ObterAtualPorViagemIdAsync(Guid viagemId);

        // Devolve todas as versões (histórico)
        Task<IEnumerable<ItinerarioResponseDTO>> ObterHistoricoPorViagemIdAsync(Guid viagemId);

        Task<ItinerarioResponseDTO?> ObterPorIdAsync(Guid itinerarioId);

        // Gera uma NOVA versão (v+1) via MCP + Gemma, com fallback determinístico
        Task<ItinerarioResponseDTO> GerarNovaVersaoAsync(GerarItinerarioRequestDTO dto);

        Task<bool> RemoverAsync(Guid itinerarioId);
   
    }
}
