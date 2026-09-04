namespace TravelAI.DTOs
{
    public record PrevisaoTempoResponseDTO(
        float TempMax,
        float TempMin,
        string Condicao,
        float ProbabilidadePrecipitacao
    );

    public record DiaItinerarioResponseDTO(
        Guid Id,
        int NumeroDia,
        DateTime Data,
        List<AtividadeResponseDTO> Atividades,
        PrevisaoTempoResponseDTO? PrevisaoTempo
    );
}