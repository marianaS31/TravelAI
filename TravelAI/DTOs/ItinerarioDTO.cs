namespace TravelAI.DTOs
{
   
    public class GerarItinerarioRequestDto(
        Guid ViagemId,
        string? InstrucoesAdicionais
    );

    public class ItinerarioResponseDto(
        Guid Id,
        Guid ViagemId,
        int Versao,
        DateTime CriadoEm,
        List<DiaItinerarioResponseDto> Dias
    );

}
