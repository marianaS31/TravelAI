namespace TravelAI.DTOs
{
    public record SegmentoVooDTO(
        string Origem,
        string Destino,
        string? Duracao,
        int Paragens
    );

    // Representa uma oferta de voo real devolvida pelo servidor MCP duffel,
    // capturada diretamente da tool call — não passa pelo resumo do LLM.
    public record VooSugeridoDTO(
        string? Id,
        string? Companhia,
        string? Preco,
        List<SegmentoVooDTO> Segmentos,
        string? Url = null
    );
}
