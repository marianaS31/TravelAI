using TravelAI.Models;

namespace TravelAI.DTOs
{
    public class ViagemCreateDTO(
        string Titulo,
        string Destino,
        DateTime DataInicio,
        DateTime DataFim,
        int NumViajantes,
        decimal Orcamento
        );
    public class ViagemUpdateDTO(
        string Titulo,
        string Destino,
        DateTime DataInicio,
        DateTime DataFim,
        int NumViajantes,
        decimal Orcamento,
        EstadoViagem? Estado
        );

    public class ViagemResponseDTO(
        Guid Id,
        string Titulo,
        string Destino,
        DateTime DataInicio,
        DateTime DataFim,
        int NumViajantes,
        decimal Orcamento,
        EstadoViagem Estado,
        DateTime CriadoEm,
        DateTime AtualizadoEm
    );




}
