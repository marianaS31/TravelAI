using TravelAI.McpIntegration;

namespace TravelAI.Interfaces
{
    public interface IMcpOrchestrator
    {
        Task<List<McpToolDefinition>> DescobrirFerramentasAsync();

        Task<McpToolCallResult> ExecutarFerramentaAsync(
            string nomeFerramentaQualificado,
            Dictionary<string, object?> argumentos);

        // Formato pronto a injetar no payload "tools" da API do LM Studio
        Task<List<object>> ObterDefinicoesParaLlmAsync();
    }
}