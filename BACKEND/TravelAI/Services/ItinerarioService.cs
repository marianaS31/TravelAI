using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelAI.Data;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Services
{
    public class ItinerarioService : IItinerarioService
    {
        private readonly TravelAIContext _context;
        private readonly IMcpOrchestrator _mcpOrchestrator;
        private readonly ILlmService _llmService;
        private readonly ILogger<ItinerarioService> _logger;

        private const int MaxIteracoesToolCalling = 4;

        private const string SystemPrompt = """
            És um assistente de planeamento de viagens. Usa as ferramentas disponíveis
            para pesquisar voos, alojamento e pontos de interesse. Gera um itinerário
            dia-a-dia, conciso, com horários realistas (formato HH:mm), cobrindo todos
            os dias da viagem.
            """;

        private const string SystemPromptExtracaoTempo = """
             Extrai a informação meteorológica do relatório seguinte e devolve APENAS
            JSON válido, sem markdown, no formato pedido. Para "probabilidadePrecipitacao",
            estima um valor entre 0 e 100 com base na descrição da condição (ex: "chuva forte"
            ≈ 80, "chuvisco" ≈ 30, "céu limpo" ≈ 0).
            """;

        private const string SystemPromptCodigoIata = """
            Indica o código IATA de 3 letras do aeroporto principal (ou mais movimentado)
            da cidade indicada. Devolve APENAS o código, em maiúsculas, sem explicação.
            Se não conseguires identificar com confiança um código válido, devolve uma
            string vazia.
            """;

        public ItinerarioService(
            TravelAIContext context,
            IMcpOrchestrator mcpOrchestrator,
            ILlmService llmService,
            ILogger<ItinerarioService> logger)
        {
            _context = context;
            _mcpOrchestrator = mcpOrchestrator;
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<ItinerarioResponseDTO> GerarNovaVersaoAsync(GerarItinerarioRequestDTO dto)
        {
            var viagem = await _context.Viagens.FindAsync(dto.ViagemId)
                ?? throw new InvalidOperationException($"Viagem {dto.ViagemId} não encontrada.");

            var ferramentas = await _mcpOrchestrator.ObterDefinicoesParaLlmAsync();

            // Sem origem, a tool de voos não tem como funcionar — remove-a do conjunto
            // disponível para o Gemma, para não gastar iterações a tentar chamá-la.
            if (string.IsNullOrWhiteSpace(dto.OrigemPartida))
            {
                ferramentas = ferramentas
                    .Where(f => !((dynamic)f).function.name.ToString().StartsWith("duffel__"))
                    .ToList();
            }

            var mensagens = new List<ChatMessage>
            {
                new("system", SystemPrompt),
                new("user", ConstruirPromptViagem(viagem, dto.InstrucoesAdicionais, dto.OrigemPartida))
            };

            // Alojamento e restaurantes são pesquisados diretamente pelo backend,
            // uma vez, de forma garantida — não se depende do Gemma decidir chamar
            // estas tools nem com que argumento "tipo". São feitas SEQUENCIALMENTE
            // (não em paralelo) porque ambas usam o mesmo cliente MCP stdio do
            // servidor 'google-places'; chamá-las em simultâneo (Task.WhenAll)
            // já causou cross-talk observado — resultados de uma pesquisa a
            // aparecerem na outra. Round-trip um pouco mais lento, mas fiável.
            var alojamentosReais = await BuscarLugaresReaisAsync(
                "google-places__pesquisar_alojamento",
                new Dictionary<string, object?> { ["destino"] = viagem.Destino });

            var restaurantesReais = await BuscarLugaresReaisAsync(
                "google-places__pesquisar_pontos_interesse",
                new Dictionary<string, object?> { ["destino"] = viagem.Destino, ["tipo"] = "restaurantes" });

            // Voos, também de forma determinística — só se houver origem definida.
            // O código IATA do destino é resolvido por uma chamada dedicada ao LLM
            // (o mesmo padrão usado para extrair o itinerário/tempo estruturado),
            // já que a tool da Duffel exige um código de 3 letras, não um nome de
            // cidade em texto livre.
            var voosReais = new List<VooSugeridoDTO>();
            if (!string.IsNullOrWhiteSpace(dto.OrigemPartida))
            {
                var codigoDestino = await ResolverCodigoIataAsync(viagem.Destino);

                if (!string.IsNullOrWhiteSpace(codigoDestino))
                {
                    voosReais = await BuscarVoosReaisAsync(
                        dto.OrigemPartida!, codigoDestino, viagem.DataInicio, viagem.DataFim, viagem.NumViajantes);
                }
                else
                {
                    _logger.LogWarning(
                        "Não foi possível resolver o código IATA para '{Destino}'; pesquisa de voos ignorada.",
                        viagem.Destino);
                }
            }

            var resposta = await _llmService.ConversarAsync(mensagens, ferramentas);

            var iteracao = 0;
            while (resposta.ToolCalls.Count > 0 && iteracao < MaxIteracoesToolCalling)
            {
                iteracao++;
                mensagens.Add(new ChatMessage("assistant", resposta.TextoResposta ?? ""));

                foreach (var toolCall in resposta.ToolCalls)
                {
                    var resultado = await _mcpOrchestrator.ExecutarFerramentaAsync(
                        toolCall.NomeFerramenta, toolCall.Argumentos);

                    if (!resultado.Sucesso)
                    {
                        _logger.LogWarning("Ferramenta {Nome} falhou: {Erro}",
                            toolCall.NomeFerramenta, resultado.MensagemErro);
                    }

                    mensagens.Add(new ChatMessage("tool",
                        resultado.ResultadoJson ?? $"Erro: {resultado.MensagemErro}"));
                }

                _logger.LogInformation(
                    "Iteração {N}/{Max} de tool-calling concluída para viagem {Id}, a repetir chamada ao LLM",
                    iteracao, MaxIteracoesToolCalling, viagem.Id);

                resposta = await _llmService.ConversarAsync(mensagens, ferramentas);
            }

            if (resposta.ToolCalls.Count > 0 || string.IsNullOrWhiteSpace(resposta.TextoResposta))
            {
                _logger.LogWarning(
                    "Limite de {Max} iterações atingido para viagem {Id}; a forçar resposta final sem ferramentas",
                    MaxIteracoesToolCalling, viagem.Id);

                mensagens.Add(new ChatMessage("assistant", resposta.TextoResposta ?? ""));
                mensagens.Add(new ChatMessage("user",
                    "Não uses mais nenhuma ferramenta. Com base em toda a informação já recolhida " +
                    "acima, gera agora o itinerário final completo, dia a dia."));

                resposta = await _llmService.ConversarAsync(mensagens, new List<object>());
            }

            var estrutura = await ExtrairEGuardarItinerarioAsync(resposta.TextoResposta ?? "");

            if (estrutura is null)
            {
                _logger.LogError("Falha total na extração do itinerário para viagem {Id}", viagem.Id);
                throw new InvalidOperationException("Não foi possível gerar um itinerário válido.");
            }

            var alojamentosFinais = ComLinkExterno(
                alojamentosReais
                    .GroupBy(a => a.Nome, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList(),
                viagem.Destino, ehAlojamento: true);

            var restaurantesFinais = ComLinkExterno(
                restaurantesReais
                    .GroupBy(r => r.Nome, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList(),
                viagem.Destino, ehAlojamento: false);

            return await PersistirItinerarioAsync(
                viagem.Id, estrutura, alojamentosFinais, restaurantesFinais, voosReais);
        }

        // Resolve o código IATA do aeroporto principal de uma cidade através de
        // uma chamada estruturada ao LLM (mesma técnica usada para o itinerário
        // e a previsão do tempo). Devolve null se não conseguir um código válido
        // de 3 letras — nesse caso a pesquisa de voos é simplesmente ignorada.
        private async Task<string?> ResolverCodigoIataAsync(string cidade)
        {
            var schema = new
            {
                type = "object",
                properties = new
                {
                    codigo = new { type = "string" }
                },
                required = new[] { "codigo" }
            };

            var resultado = await _llmService.ExtrairEstruturadoAsync<CodigoIataResultado>(
                cidade, SystemPromptCodigoIata, schema);

            var codigo = resultado?.Codigo?.Trim().ToUpperInvariant();

            return !string.IsNullOrWhiteSpace(codigo) && codigo.Length == 3 ? codigo : null;
        }

        private async Task<List<VooSugeridoDTO>> BuscarVoosReaisAsync(
            string origemIata, string destinoIata, DateTime dataInicio, DateTime dataFim, int numPassageiros)
        {
            var resultado = await _mcpOrchestrator.ExecutarFerramentaAsync(
                "duffel__pesquisar_voos",
                new Dictionary<string, object?>
                {
                    ["origem"] = origemIata,
                    ["destino"] = destinoIata,
                    ["dataPartida"] = dataInicio.ToString("yyyy-MM-dd"),
                    ["dataRegresso"] = dataFim.ToString("yyyy-MM-dd"),
                    ["numPassageiros"] = Math.Max(1, numPassageiros)
                });

            if (!resultado.Sucesso || string.IsNullOrWhiteSpace(resultado.ResultadoJson))
            {
                _logger.LogWarning("Falha ao obter voos reais ({Origem} -> {Destino}): {Erro}",
                    origemIata, destinoIata, resultado.MensagemErro);
                return new List<VooSugeridoDTO>();
            }

            try
            {
                var voos = JsonSerializer.Deserialize<List<VooBrutoMcp>>(
                    resultado.ResultadoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var urlGoogleFlights =
                    $"https://www.google.com/travel/flights?q={Uri.EscapeDataString($"voos de {origemIata} para {destinoIata} em {dataInicio:yyyy-MM-dd}")}";

                return voos?.Select(v => new VooSugeridoDTO(
                    v.Id, v.Companhia, v.Preco,
                    v.Segmentos?.Select(s => new SegmentoVooDTO(
                        s.Origem ?? "", s.Destino ?? "", s.Duracao, s.Paragens)).ToList()
                        ?? new List<SegmentoVooDTO>(),
                    Url: urlGoogleFlights
                )).ToList() ?? new List<VooSugeridoDTO>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Falha ao desserializar resultado de voos: {Json}", resultado.ResultadoJson);
                return new List<VooSugeridoDTO>();
            }
        }

        private class CodigoIataResultado
        {
            public string Codigo { get; set; } = string.Empty;
        }

        // Espelha a forma dos objetos devolvidos por flights_server.js
        private class VooBrutoMcp
        {
            public string? Id { get; set; }
            public string? Companhia { get; set; }
            public string? Preco { get; set; }
            public List<SegmentoBrutoMcp>? Segmentos { get; set; }
        }

        private class SegmentoBrutoMcp
        {
            public string? Origem { get; set; }
            public string? Destino { get; set; }
            public string? Duracao { get; set; }
            public int Paragens { get; set; }
        }

        private async Task<List<LugarSugeridoDTO>> BuscarLugaresReaisAsync(
            string nomeFerramenta, Dictionary<string, object?> argumentos)
        {
            var resultado = await _mcpOrchestrator.ExecutarFerramentaAsync(nomeFerramenta, argumentos);

            if (!resultado.Sucesso || string.IsNullOrWhiteSpace(resultado.ResultadoJson))
            {
                _logger.LogWarning("Falha ao obter lugares reais de '{Ferramenta}': {Erro}",
                    nomeFerramenta, resultado.MensagemErro);
                return new List<LugarSugeridoDTO>();
            }

            return TentarDeserializarLugares(resultado.ResultadoJson);
        }

        private static List<LugarSugeridoDTO> TentarDeserializarLugares(string json)
        {
            try
            {
                var lugares = JsonSerializer.Deserialize<List<LugarBrutoMcp>>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return lugares?
                    .Where(l => !string.IsNullOrWhiteSpace(l.Nome))
                    .Select(l => new LugarSugeridoDTO(
                        l.Nome!, l.Morada, l.Avaliacao, l.NumAvaliacoes, l.NivelPreco,
                        Latitude: l.Latitude, Longitude: l.Longitude))
                    .ToList() ?? new List<LugarSugeridoDTO>();
            }
            catch (JsonException)
            {
                return new List<LugarSugeridoDTO>();
            }
        }

        private class LugarBrutoMcp
        {
            public string? Nome { get; set; }
            public string? Morada { get; set; }
            public double? Avaliacao { get; set; }
            public int? NumAvaliacoes { get; set; }
            public string? NivelPreco { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
        }

        private async Task<List<PrevisaoDiaEstruturada>> ObterPrevisoesAsync(string destino, int numDias)
        {
            var diasAPedir = Math.Min(numDias, 7);
            var resultado = await _mcpOrchestrator.ExecutarFerramentaAsync(
                "open-meteo__get_weather",
                new Dictionary<string, object?> { ["cidade"] = destino, ["dias"] = diasAPedir });

            if (!resultado.Sucesso || string.IsNullOrWhiteSpace(resultado.ResultadoJson))
            {
                _logger.LogWarning("Não foi possível obter previsão do tempo para '{Destino}': {Erro}",
                    destino, resultado.MensagemErro);
                return new List<PrevisaoDiaEstruturada>();
            }

            var estrutura = await _llmService.ExtrairEstruturadoAsync<PrevisoesEstruturadas>(
                resultado.ResultadoJson, SystemPromptExtracaoTempo, ConstruirSchemaTempo());

            return estrutura?.Previsoes ?? new List<PrevisaoDiaEstruturada>();
        }

        private static object ConstruirSchemaTempo() => new
        {
            type = "object",
            properties = new
            {
                previsoes = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            numeroDia = new { type = "integer" },
                            tempMax = new { type = "number" },
                            tempMin = new { type = "number" },
                            condicao = new { type = "string" },
                            probabilidadePrecipitacao = new { type = "number" }
                        },
                        required = new[] { "numeroDia", "tempMax", "tempMin", "condicao", "probabilidadePrecipitacao" }
                    }
                }
            },
            required = new[] { "previsoes" }
        };

        private static object ConstruirSchemaItinerario() => new
        {
            type = "object",
            properties = new
            {
                dias = new
                {
                    type = "array",
                    minItems = 1,
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
                                minItems = 1,
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
        };

        private async Task<ItinerarioEstruturado?> ExtrairEGuardarItinerarioAsync(string textoLivre)
        {
            const string extractSystemPrompt = """
                Extrai a informação de itinerário do texto seguinte e devolve
                APENAS JSON válido, sem markdown, sem texto adicional, no formato:
                { "dias": [ { "numeroDia": 1, "data": "YYYY-MM-DD",
                  "atividades": [ { "nome": "", "tipo": "PONTO_INTERESSE",
                  "horaInicio": "HH:mm", "horaFim": "HH:mm", "local": "", "detalhes": "" } ] } ] }
                O campo "tipo" deve ser um destes valores exatos: VOO, ALOJAMENTO,
                PONTO_INTERESSE, ALUGUER_CARRO, REFEICAO, DESLOCACAO, OUTRO.
                """;

            if (string.IsNullOrWhiteSpace(textoLivre)) return null;

            return await _llmService.ExtrairEstruturadoAsync<ItinerarioEstruturado>(
                textoLivre, extractSystemPrompt, ConstruirSchemaItinerario());
        }

        private async Task<ItinerarioResponseDTO> PersistirItinerarioAsync(
            Guid viagemId,
            ItinerarioEstruturado estrutura,
            List<LugarSugeridoDTO> alojamentosReais,
            List<LugarSugeridoDTO> restaurantesReais,
            List<VooSugeridoDTO> voosReais)
        {
            var viagem = await _context.Viagens.FindAsync(viagemId)
                ?? throw new InvalidOperationException($"Viagem {viagemId} não encontrada.");

            var versaoAnterior = await _context.Itinerarios
                .Where(i => i.ViagemId == viagemId)
                .Select(i => (int?)i.Versao)
                .MaxAsync() ?? 0;

            var itinerario = new Itinerario
            {
                Id = Guid.NewGuid(),
                ViagemId = viagemId,
                Versao = versaoAnterior + 1,
                CriadoEm = DateTime.UtcNow
            };
            _context.Itinerarios.Add(itinerario);

            var previsoes = await ObterPrevisoesAsync(viagem.Destino, estrutura.Dias.Count);

            foreach (var diaDto in estrutura.Dias)
            {
                var dia = new DiaItinerario
                {
                    Id = Guid.NewGuid(),
                    ItinerarioId = itinerario.Id,
                    NumeroDia = diaDto.NumeroDia,
                    Data = viagem.DataInicio.AddDays(diaDto.NumeroDia - 1)
                };
                _context.DiasItinerario.Add(dia);

                var ordem = 0;
                foreach (var ativDto in diaDto.Atividades)
                {
                    _context.Atividades.Add(new Atividade
                    {
                        Id = Guid.NewGuid(),
                        DiaItinerarioId = dia.Id,
                        Ordem = ordem++,
                        Nome = ativDto.Nome,
                        Tipo = Enum.TryParse<TipoAtividade>(ativDto.Tipo, true, out var tipo)
                            ? tipo : TipoAtividade.OUTRO,
                        HoraInicio = ativDto.HoraInicio,
                        HoraFim = ativDto.HoraFim,
                        Local = ativDto.Local ?? string.Empty,
                        Detalhes = ativDto.Detalhes
                    });
                }

                var previsaoDia = previsoes.FirstOrDefault(p => p.NumeroDia == diaDto.NumeroDia);
                if (previsaoDia is not null)
                {
                    _context.PrevisoesTempo.Add(new PrevisaoTempo
                    {
                        Id = Guid.NewGuid(),
                        DiaItinerarioId = dia.Id,
                        TempMax = previsaoDia.TempMax,
                        TempMin = previsaoDia.TempMin,
                        Condicao = previsaoDia.Condicao,
                        ProbabilidadePrecipitacao = previsaoDia.ProbabilidadePrecipitacao
                    });
                }
            }

            await _context.SaveChangesAsync();

            var itinerarioCompleto = await _context.Itinerarios
                .Include(i => i.Dias).ThenInclude(d => d.Atividades)
                .Include(i => i.Dias).ThenInclude(d => d.PrevisaoTempo)
                .FirstAsync(i => i.Id == itinerario.Id);

            return MapToDto(itinerarioCompleto, alojamentosReais, restaurantesReais, voosReais);
        }

        public async Task<ItinerarioResponseDTO?> ObterAtualPorViagemIdAsync(Guid viagemId)
        {
            var itinerario = await _context.Itinerarios
                .Include(i => i.Dias).ThenInclude(d => d.Atividades)
                .Include(i => i.Dias).ThenInclude(d => d.PrevisaoTempo)
                .Where(i => i.ViagemId == viagemId)
                .OrderByDescending(i => i.Versao)
                .FirstOrDefaultAsync();

            return itinerario is null ? null : MapToDto(itinerario);
        }

        public async Task<IEnumerable<ItinerarioResponseDTO>> ObterHistoricoPorViagemIdAsync(Guid viagemId)
        {
            var itinerarios = await _context.Itinerarios
                .Include(i => i.Dias).ThenInclude(d => d.Atividades)
                .Include(i => i.Dias).ThenInclude(d => d.PrevisaoTempo)
                .Where(i => i.ViagemId == viagemId)
                .OrderByDescending(i => i.Versao)
                .ToListAsync();

            return itinerarios.Select(i => MapToDto(i));
        }

        public async Task<ItinerarioResponseDTO?> ObterPorIdAsync(Guid itinerarioId)
        {
            var itinerario = await _context.Itinerarios
                .Include(i => i.Dias).ThenInclude(d => d.Atividades)
                .Include(i => i.Dias).ThenInclude(d => d.PrevisaoTempo)
                .FirstOrDefaultAsync(i => i.Id == itinerarioId);

            return itinerario is null ? null : MapToDto(itinerario);
        }

        public async Task<bool> RemoverAsync(Guid itinerarioId)
        {
            var itinerario = await _context.Itinerarios.FindAsync(itinerarioId);
            if (itinerario is null) return false;

            _context.Itinerarios.Remove(itinerario);
            await _context.SaveChangesAsync();
            return true;
        }

        private static string ConstruirPromptViagem(Viagem v, string? instrucoes, string? origemPartida)
        {
            var origemTexto = string.IsNullOrWhiteSpace(origemPartida)
                ? ""
                : $" A viagem parte de {origemPartida}.";

            return $"Planeia uma viagem para {v.Destino}, de {v.DataInicio:yyyy-MM-dd} a {v.DataFim:yyyy-MM-dd}, " +
                   $"para {v.NumViajantes} pessoa(s), com orçamento aproximado de {v.Orcamento:C}.{origemTexto} " +
                   (string.IsNullOrWhiteSpace(instrucoes) ? "" : $"Instruções adicionais: {instrucoes}");
        }

        private static List<LugarSugeridoDTO> ComLinkExterno(
            List<LugarSugeridoDTO> lugares, string destino, bool ehAlojamento)
        {
            return lugares.Select(l =>
            {
                var url = ehAlojamento
                    ? $"https://www.booking.com/searchresults.html?ss={Uri.EscapeDataString($"{l.Nome} {destino}")}"
                    : $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString($"{l.Nome} {l.Morada ?? destino}")}";

                return l with { Url = url };
            }).ToList();
        }

        private static ItinerarioResponseDTO MapToDto(
            Itinerario i,
            List<LugarSugeridoDTO>? alojamentosReais = null,
            List<LugarSugeridoDTO>? restaurantesReais = null,
            List<VooSugeridoDTO>? voosReais = null) => new(
            i.Id, i.ViagemId, i.Versao, i.CriadoEm,
            i.Dias.OrderBy(d => d.NumeroDia).Select(d => new DiaItinerarioResponseDTO(
                d.Id, d.NumeroDia, d.Data,
                d.Atividades.OrderBy(a => a.Ordem).Select(a => new AtividadeResponseDTO
                {
                    Id = a.Id,
                    Ordem = a.Ordem,
                    Nome = a.Nome,
                    Tipo = a.Tipo,
                    HoraInicio = a.HoraInicio,
                    HoraFim = a.HoraFim,
                    Local = a.Local,
                    Detalhes = a.Detalhes
                }).ToList(),
                d.PrevisaoTempo is null ? null : new PrevisaoTempoResponseDTO(
                    d.PrevisaoTempo.TempMax, d.PrevisaoTempo.TempMin,
                    d.PrevisaoTempo.Condicao, d.PrevisaoTempo.ProbabilidadePrecipitacao)
            )).ToList(),
            alojamentosReais ?? new List<LugarSugeridoDTO>(),
            restaurantesReais ?? new List<LugarSugeridoDTO>(),
            voosReais ?? new List<VooSugeridoDTO>()
        );
    }

    internal class ItinerarioEstruturado
    {
        public List<DiaEstruturado> Dias { get; set; } = new();
    }

    internal class DiaEstruturado
    {
        public int NumeroDia { get; set; }
        public DateTime Data { get; set; }
        public List<AtividadeEstruturada> Atividades { get; set; } = new();
    }

    internal class AtividadeEstruturada
    {
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = "OUTRO";
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public string? Local { get; set; }
        public string? Detalhes { get; set; }
    }

    internal class PrevisoesEstruturadas
    {
        public List<PrevisaoDiaEstruturada> Previsoes { get; set; } = new();
    }

    internal class PrevisaoDiaEstruturada
    {
        public int NumeroDia { get; set; }
        public float TempMax { get; set; }
        public float TempMin { get; set; }
        public string Condicao { get; set; } = string.Empty;
        public float ProbabilidadePrecipitacao { get; set; }
    }
}
