using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TravelAI.Interfaces;

namespace TravelAI.Services
{
    public class LmStudioLlmService : ILlmService
    {
        private readonly HttpClient _http;
        private readonly string _modelo;
        private readonly ILogger<LmStudioLlmService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public LmStudioLlmService(HttpClient http, IConfiguration config, ILogger<LmStudioLlmService> logger)
        {
            _http = http;
            _http.BaseAddress = new Uri(config["LmStudio:BaseUrl"] ?? "http://host.docker.internal:1234/v1");
            _modelo = config["LmStudio:Model"] ?? "gemma-4-e4b";
            _logger = logger;
        }

        public async Task<LlmResponse> ConversarAsync(
            List<ChatMessage> mensagens, List<object>? ferramentasDisponiveis = null, int maxTokens = 4000)
        {
            var payload = new Dictionary<string, object?>
            {
                ["model"] = _modelo,
                ["messages"] = mensagens.Select(m => new { role = m.Role, content = m.Content }),
                ["max_tokens"] = maxTokens,
                ["temperature"] = 0.7
            };

            if (ferramentasDisponiveis is { Count: > 0 })
                payload["tools"] = ferramentasDisponiveis;

            var resposta = await EnviarAsync(payload);
            return InterpretarResposta(resposta);
        }

        public async Task<T?> ExtrairEstruturadoAsync<T>(string prompt, string systemPrompt) where T : class
        {
            var payload = new Dictionary<string, object?>
            {
                ["model"] = _modelo,
                ["messages"] = new[]
                {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = prompt }
        },
                ["temperature"] = 0.1,
                ["response_format"] = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "itinerario_estruturado",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                dias = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            numeroDia = new { type = "integer" },
                                            data = new { type = "string" },
                                            atividades = new
                                            {
                                                type = "array",
                                                items = new
                                                {
                                                    type = "object",
                                                    properties = new
                                                    {
                                                        nome = new { type = "string" },
                                                        tipo = new
                                                        {
                                                            type = "string",
                                                            @enum = new[] { "VOO", "ALOJAMENTO", "PONTO_INTERESSE", "ALUGUER_CARRO", "REFEICAO", "DESLOCACAO", "OUTRO" }
                                                        },
                                                        horaInicio = new { type = "string" },
                                                        horaFim = new { type = "string" },
                                                        local = new { type = "string" },
                                                        detalhes = new { type = "string" }
                                                    },
                                                    required = new[] { "nome", "tipo", "horaInicio", "horaFim", "local", "detalhes" }
                                                }
                                            }
                                        },
                                        required = new[] { "numeroDia", "data", "atividades" }
                                    }
                                }
                            },
                            required = new[] { "dias" }
                        }
                    }
                }
            };

            var resposta = await EnviarAsync(payload);
            var conteudo = ExtrairConteudoTexto(resposta);

            if (string.IsNullOrWhiteSpace(conteudo)) return null;

            // Salvaguarda: remove blocos markdown ```json se o modelo os incluir mesmo assim
            conteudo = conteudo.Trim();
            if (conteudo.StartsWith("```"))
            {
                conteudo = conteudo.Trim('`').Replace("json", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            try
            {
                return JsonSerializer.Deserialize<T>(conteudo, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha ao desserializar resposta estruturada do Gemma: {Conteudo}", conteudo);
                return null;
            }
        }

        private async Task<JsonElement> EnviarAsync(Dictionary<string, object?> payload)
        {
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            var resposta = await _http.PostAsync("chat/completions", conteudo);
            var corpo = await resposta.Content.ReadAsStringAsync();

            if (!resposta.IsSuccessStatusCode)
            {
                _logger.LogError("LM Studio devolveu {StatusCode}: {Corpo}", resposta.StatusCode, corpo);
                throw new HttpRequestException($"LM Studio erro {resposta.StatusCode}: {corpo}");
            }

            return JsonDocument.Parse(corpo).RootElement;
        }

        private static string? ExtrairConteudoTexto(JsonElement resposta)
        {
            var mensagem = resposta.GetProperty("choices")[0].GetProperty("message");
            return mensagem.TryGetProperty("content", out var content) ? content.GetString() : null;
        }

        private LlmResponse InterpretarResposta(JsonElement resposta)
        {
            var mensagem = resposta.GetProperty("choices")[0].GetProperty("message");
            var texto = mensagem.TryGetProperty("content", out var c) ? c.GetString() : null;

            var toolCalls = new List<LlmToolCall>();
            if (mensagem.TryGetProperty("tool_calls", out var tcArray) && tcArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var tc in tcArray.EnumerateArray())
                {
                    var id = tc.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                    var funcao = tc.GetProperty("function");
                    var nome = funcao.GetProperty("name").GetString() ?? string.Empty;
                    var argsJson = funcao.GetProperty("arguments").GetString() ?? "{}";

                    Dictionary<string, object?> argumentos;
                    try
                    {
                        argumentos = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsJson) ?? new();
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Argumentos de tool call inválidos para '{Nome}': {Args}", nome, argsJson);
                        argumentos = new();
                    }

                    toolCalls.Add(new LlmToolCall(id, nome, argumentos));
                }
            }

            return new LlmResponse(texto, toolCalls, true);
        }
    }
}