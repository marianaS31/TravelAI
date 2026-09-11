namespace TravelAI.DTOs
{
    public record GerarItinerarioRequestDTO(
        Guid ViagemId,
        string? InstrucoesAdicionais,
        string? OrigemPartida = null
    );

    public record ItinerarioResponseDTO(
        Guid Id,
        Guid ViagemId,
        int Versao,
        DateTime CriadoEm,
        List<DiaItinerarioResponseDTO> Dias
    );
}