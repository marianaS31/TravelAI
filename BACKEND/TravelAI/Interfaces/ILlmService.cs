namespace TravelAI.Interfaces
{
    public record ChatMessage(string Role, string Content);

    public record LlmToolCall(string Id, string NomeFerramenta, Dictionary<string, object?> Argumentos);

    public record LlmResponse(
        string? TextoResposta,
        List<LlmToolCall> ToolCalls,
        bool TerminouComSucesso
    );

    public interface ILlmService
    {
        Task<LlmResponse> ConversarAsync(
            List<ChatMessage> mensagens,
            List<object>? ferramentasDisponiveis = null,
            int maxTokens = 4000);

      
        Task<T?> ExtrairEstruturadoAsync<T>(string prompt, string systemPrompt, object jsonSchemaDefinition) where T : class;
    }
}