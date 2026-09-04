namespace TravelAI.DTOs
{
    public record GerarItinerarioRequestDTO(
        Guid ViagemId,
        string? InstrucoesAdicionais
    );

    public record ItinerarioResponseDTO(
        Guid Id,
        Guid ViagemId,
        int Versao,
        DateTime CriadoEm,
        List<DiaItinerarioResponseDTO> Dias
    );
}