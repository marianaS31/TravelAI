namespace TravelAI.DTOs
{
    // Representa um resultado real devolvido pelo servidor MCP google-places,
    // capturado diretamente da tool call — não passa pelo resumo do LLM.
    public record LugarSugeridoDTO(
        string Nome,
        string? Morada,
        double? Avaliacao,
        int? NumAvaliacoes,
        string? NivelPreco,
        string? Url = null,
        double? Latitude = null,
        double? Longitude = null
    );
}
