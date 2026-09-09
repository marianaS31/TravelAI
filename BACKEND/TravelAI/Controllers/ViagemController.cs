using Microsoft.AspNetCore.Mvc;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Controllers
{
    [ApiController]
    [Route("api/viagens")]
    public class ViagemController : ControllerBase
    {
        private readonly IViagemService _viagemService;

        public ViagemController(IViagemService viagemService)
        {
            _viagemService = viagemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViagemResponseDTO>>> ObterTodas()
        {
            var viagens = await _viagemService.ObterTodasAsync();
            return Ok(viagens);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ViagemResponseDTO>> ObterPorId(Guid id)
        {
            var viagem = await _viagemService.ObterPorIdAsync(id);
            return viagem is null ? NotFound() : Ok(viagem);
        }

        [HttpPost]
        public async Task<ActionResult<ViagemResponseDTO>> Criar(ViagemCreateDTO dto)
        {
            var viagem = await _viagemService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = viagem.Id }, viagem);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ViagemResponseDTO>> Atualizar(Guid id, ViagemUpdateDTO dto)
        {
            var viagem = await _viagemService.AtualizarAsync(id, dto);
            return viagem is null ? NotFound() : Ok(viagem);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var removido = await _viagemService.RemoverAsync(id);
            return removido ? NoContent() : NotFound();
        }
    }
}