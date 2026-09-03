namespace TravelAI.DTOs
{
   
    public class GerarItinerarioRequestDTO(
        Guid ViagemId,
        string? InstrucoesAdicionais
    );

    public class ItinerarioResponseDTO(
        Guid Id,
        Guid ViagemId,
        int Versao,
        DateTime CriadoEm,
        List<DiaItinerarioResponseDTO> Dias
    );

}
