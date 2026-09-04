using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IItinerarioService
    {
        // Devolve a versão mais alta (o itinerário "atual") para a viagem
        Task<ItinerarioResponseDTO?> ObterAtualPorViagemIdAsync(Guid viagemId);

        // Devolve todas as versões (histórico completo)
        Task<IEnumerable<ItinerarioResponseDTO>> ObterHistoricoPorViagemIdAsync(Guid viagemId);

        Task<ItinerarioResponseDTO?> ObterPorIdAsync(Guid itinerarioId);

        // Gera uma NOVA versão via MCP + Gemma, com fallback determinístico
        Task<ItinerarioResponseDTO> GerarNovaVersaoAsync(GerarItinerarioRequestDTO dto);

        Task<bool> RemoverAsync(Guid itinerarioId);
    }
}