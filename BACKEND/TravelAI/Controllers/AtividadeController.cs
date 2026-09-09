using Microsoft.AspNetCore.Mvc;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Controllers
{
    [ApiController]
    [Route("api")]
    public class AtividadeController : ControllerBase
    {
        private readonly IAtividadeService _atividadeService;

        public AtividadeController(IAtividadeService atividadeService)
        {
            _atividadeService = atividadeService;
        }

        [HttpGet("dias/{diaId:guid}/atividades")]
        public async Task<ActionResult<IEnumerable<AtividadeResponseDTO>>> ObterPorDia(Guid diaId)
        {
            var atividades = await _atividadeService.ObterPorDiaIdAsync(diaId);
            return Ok(atividades);
        }

        [HttpPost("dias/{diaId:guid}/atividades")]
        public async Task<ActionResult<AtividadeResponseDTO>> Adicionar(Guid diaId, AtividadeCreateDTO dto)
        {
            var atividade = await _atividadeService.AdicionarAsync(diaId, dto);
            return CreatedAtAction(nameof(ObterPorDia), new { diaId }, atividade);
        }

        [HttpPut("atividades/{id:guid}")]
        public async Task<ActionResult<AtividadeResponseDTO>> Atualizar(Guid id, AtividadeUpdateDTO dto)
        {
            var atividade = await _atividadeService.AtualizarAsync(id, dto);
            return atividade is null ? NotFound() : Ok(atividade);
        }

        [HttpDelete("atividades/{id:guid}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var removido = await _atividadeService.RemoverAsync(id);
            return removido ? NoContent() : NotFound();
        }

        [HttpPut("dias/{diaId:guid}/atividades/reordenar")]
        public async Task<IActionResult> Reordenar(Guid diaId, ReordenarAtividadesDTO dto)
        {
            var sucesso = await _atividadeService.ReordenarAsync(diaId, dto);
            return sucesso ? NoContent() : BadRequest(new { erro = "Lista de IDs não corresponde às atividades do dia." });
        }
    }
}