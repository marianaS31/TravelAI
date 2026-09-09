using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using TravelAI.Interfaces;
using Microsoft.Extensions.Options;

namespace TravelAI.McpIntegration
{
    public class McpOrchestrator : IMcpOrchestrator, IAsyncDisposable
    {
        private readonly List<McpServerConfig> _configs;
        private readonly ILogger<McpOrchestrator> _logger;

        // servidor -> cliente MCP já ligado
        private readonly Dictionary<string, McpClient> _clientes = new();
        // nome qualificado -> (servidor, nome original) para routing na execução
        private readonly Dictionary<string, (string Servidor, string NomeOriginal)> _mapaFerramentas = new();
        private bool _inicializado;

        public McpOrchestrator(IOptions<List<McpServerConfig>> config, ILogger<McpOrchestrator> logger)
        {
            _configs = config.Value;
            _logger = logger;
        }

        private async Task GarantirInicializadoAsync()
        {
            if (_inicializado) return;

            foreach (var cfg in _configs)
            {
                try
                {
                    var transport = new StdioClientTransport(new StdioClientTransportOptions
                    {
                        Name = cfg.Nome,
                        Command = cfg.Comando,
                        Arguments = cfg.Argumentos.ToArray()
                    });

                    var client = await McpClient.CreateAsync(transport);
                    _clientes[cfg.Nome] = client;

                    _logger.LogInformation("Ligado ao MCP server '{Nome}'", cfg.Nome);
                }
                catch (Exception ex)
                {
                    // Um server em falha não deve impedir os outros de funcionar —
                    // o ItinerarioService/fallback determinístico lida com ferramentas em falta
                    _logger.LogError(ex, "Falha ao ligar ao MCP server '{Nome}'", cfg.Nome);
                }
            }

            _inicializado = true;
        }

        public async Task<List<McpToolDefinition>> DescobrirFerramentasAsync()
        {
            await GarantirInicializadoAsync();

            var definicoes = new List<McpToolDefinition>();
            _mapaFerramentas.Clear();

            foreach (var (nomeServidor, cliente) in _clientes)
            {
                List<McpClientTool> tools;
                try
                {
                    tools = (await cliente.ListToolsAsync()).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao listar ferramentas do server '{Nome}'", nomeServidor);
                    continue;
                }

                foreach (var tool in tools)
                {
                    var nomeQualificado = $"{nomeServidor}__{tool.Name}";
                    _mapaFerramentas[nomeQualificado] = (nomeServidor, tool.Name);

                    definicoes.Add(new McpToolDefinition(
                        nomeQualificado,
                        tool.Name,
                        tool.Description ?? string.Empty,
                        tool.JsonSchema,
                        nomeServidor));
                }
            }

            return definicoes;
        }

        public async Task<McpToolCallResult> ExecutarFerramentaAsync(
            string nomeFerramentaQualificado, Dictionary<string, object?> argumentos)
        {
            if (_mapaFerramentas.Count == 0)
                await DescobrirFerramentasAsync();

            if (!_mapaFerramentas.TryGetValue(nomeFerramentaQualificado, out var alvo))
                return new McpToolCallResult(false, null, $"Ferramenta '{nomeFerramentaQualificado}' não encontrada.");

            if (!_clientes.TryGetValue(alvo.Servidor, out var cliente))
                return new McpToolCallResult(false, null, $"Servidor '{alvo.Servidor}' não está ligado.");

            try
            {
                var resultado = await cliente.CallToolAsync(alvo.NomeOriginal, argumentos);
                var textoResultado = string.Join("\n",
                    resultado.Content.OfType<TextContentBlock>().Select(c => c.Text));

                return new McpToolCallResult(true, textoResultado, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao executar ferramenta '{Nome}'", nomeFerramentaQualificado);
                return new McpToolCallResult(false, null, ex.Message);
            }
        }

        // Traduz para o formato "tools" OpenAI-compatible que o LM Studio espera
        public async Task<List<object>> ObterDefinicoesParaLlmAsync()
        {
            var definicoes = await DescobrirFerramentasAsync();

            return definicoes.Select(d => (object)new
            {
                type = "function",
                function = new
                {
                    name = d.NomeQualificado,
                    description = d.Descricao,
                    parameters = d.ParametrosSchema
                }
            }).ToList();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var cliente in _clientes.Values)
            {
                if (cliente is IAsyncDisposable disposable)
                    await disposable.DisposeAsync();
            }
        }
    }
}