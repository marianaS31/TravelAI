namespace TravelAI.DTOs
{
  
    public class PrevisaoTempoResponseDto(
        float TempMax,
        float TempMin,
        string Condicao,
        float ProbabilidadePrecipitacao
    );

    public class DiaItinerarioResponseDto(
        Guid Id,
        int NumeroDia,
        DateTime Data,
        List<AtividadeResponseDto> Atividades,
        PrevisaoTempoResponseDto? PrevisaoTempo
    );
}
