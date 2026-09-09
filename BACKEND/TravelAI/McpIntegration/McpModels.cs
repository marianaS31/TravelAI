namespace TravelAI.McpIntegration
{
    public class McpServerConfig
    {
        public string Nome { get; set; } = string.Empty;
        public string Comando { get; set; } = string.Empty;
        public List<string> Argumentos { get; set; } = new();
    }

    // Ferramenta descoberta num servidor MCP, já com nome qualificado
    // (prefixado pelo servidor de origem) para evitar colisões entre servers
    public record McpToolDefinition(
        string NomeQualificado,   // ex: "amadeus__pesquisar_voos"
        string NomeOriginal,      // ex: "pesquisar_voos"
        string Descricao,
        object ParametrosSchema,
        string ServidorOrigem
    );

    public record McpToolCallResult(
        bool Sucesso,
        string? ResultadoJson,
        string? MensagemErro
    );
}
