namespace TravelAI.DTOs
{
  
    public class PrevisaoTempoResponseDTO(
        float TempMax,
        float TempMin,
        string Condicao,
        float ProbabilidadePrecipitacao
    );

    public class DiaItinerarioResponseDTO(
        Guid Id,
        int NumeroDia,
        DateTime Data,
        List<AtividadeResponseDTO> Atividades,
        PrevisaoTempoResponseDTO? PrevisaoTempo
    );
}
