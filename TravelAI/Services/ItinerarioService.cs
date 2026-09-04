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

        private const string SystemPrompt = """
            És um assistente de planeamento de viagens. Usa as ferramentas disponíveis
            para pesquisar voos, alojamento e pontos de interesse. Gera um itinerário
            dia-a-dia, conciso, com horários realistas (formato HH:mm), cobrindo todos
            os dias da viagem.
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

            var mensagens = new List<ChatMessage>
            {
                new("system", SystemPrompt),
                new("user", ConstruirPromptViagem(viagem, dto.InstrucoesAdicionais))
            };

            var resposta = await _llmService.ConversarAsync(mensagens, ferramentas);

            // Caso o Gemma tenha chamado ferramentas autonomamente, executa-as
            // e volta a perguntar-lhe com os resultados incluídos na conversa
            if (resposta.ToolCalls.Count > 0)
            {
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

                resposta = await _llmService.ConversarAsync(mensagens, ferramentas);
            }

            // Fallback determinístico: o Gemma frequentemente ignora instruções
            // de tool-calling — extraímos a estrutura do texto livre à parte
            var estrutura = await ExtrairEGuardarItinerarioAsync(resposta.TextoResposta ?? "");

            if (estrutura is null)
            {
                _logger.LogError("Falha total na extração do itinerário para viagem {Id}", viagem.Id);
                throw new InvalidOperationException("Não foi possível gerar um itinerário válido.");
            }

            return await PersistirItinerarioAsync(viagem.Id, estrutura);
        }

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
                textoLivre, extractSystemPrompt);
        }

        private async Task<ItinerarioResponseDTO> PersistirItinerarioAsync(
            Guid viagemId, ItinerarioEstruturado estrutura)
        {
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

            foreach (var diaDto in estrutura.Dias)
            {
                var dia = new DiaItinerario
                {
                    Id = Guid.NewGuid(),
                    ItinerarioId = itinerario.Id,
                    NumeroDia = diaDto.NumeroDia,
                    Data = diaDto.Data
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
            }

            await _context.SaveChangesAsync();

            return await ObterPorIdAsync(itinerario.Id)
                ?? throw new InvalidOperationException("Falha ao persistir itinerário.");
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

            return itinerarios.Select(MapToDto);
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

        private static string ConstruirPromptViagem(Viagem v, string? instrucoes) =>
            $"Planeia uma viagem para {v.Destino}, de {v.DataInicio:yyyy-MM-dd} a {v.DataFim:yyyy-MM-dd}, " +
            $"para {v.NumViajantes} pessoa(s), com orçamento aproximado de {v.Orcamento:C}. " +
            (string.IsNullOrWhiteSpace(instrucoes) ? "" : $"Instruções adicionais: {instrucoes}");

        private static ItinerarioResponseDTO MapToDto(Itinerario i) => new(
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
            )).ToList()
        );
    }

    // Modelos internos usados só para desserializar a resposta JSON do Gemma —
    // não confundir com os DTOs públicos da API
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
}