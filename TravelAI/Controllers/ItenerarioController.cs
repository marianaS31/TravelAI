using Microsoft.AspNetCore.Mvc;
using TravelAI.DTOs;
using TravelAI.Interfaces;

namespace TravelAI.Controllers
{
    [ApiController]
    [Route("api")]
    public class ItinerarioController : ControllerBase
    {
        private readonly IItinerarioService _itinerarioService;

        public ItinerarioController(IItinerarioService itinerarioService)
        {
            _itinerarioService = itinerarioService;
        }

        // Gera uma nova versão do itinerário (chama MCP + Gemma) — endpoint principal do projeto
        [HttpPost("itinerarios/gerar")]
        public async Task<ActionResult<ItinerarioResponseDTO>> Gerar(GerarItinerarioRequestDTO dto)
        {
            try
            {
                var itinerario = await _itinerarioService.GerarNovaVersaoAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = itinerario.Id }, itinerario);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("itinerarios/{id:guid}")]
        public async Task<ActionResult<ItinerarioResponseDTO>> ObterPorId(Guid id)
        {
            var itinerario = await _itinerarioService.ObterPorIdAsync(id);
            return itinerario is null ? NotFound() : Ok(itinerario);
        }

        [HttpGet("viagens/{viagemId:guid}/itinerario-atual")]
        public async Task<ActionResult<ItinerarioResponseDTO>> ObterAtual(Guid viagemId)
        {
            var itinerario = await _itinerarioService.ObterAtualPorViagemIdAsync(viagemId);
            return itinerario is null ? NotFound() : Ok(itinerario);
        }

        [HttpGet("viagens/{viagemId:guid}/itinerarios")]
        public async Task<ActionResult<IEnumerable<ItinerarioResponseDTO>>> ObterHistorico(Guid viagemId)
        {
            var historico = await _itinerarioService.ObterHistoricoPorViagemIdAsync(viagemId);
            return Ok(historico);
        }

        [HttpDelete("itinerarios/{id:guid}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var removido = await _itinerarioService.RemoverAsync(id);
            return removido ? NoContent() : NotFound();
        }
    }
}