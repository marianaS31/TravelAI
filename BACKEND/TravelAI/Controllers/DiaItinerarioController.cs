using Microsoft.AspNetCore.Mvc;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Controllers
{
    [ApiController]
    [Route("api")]
    public class DiaItinerarioController : ControllerBase
    {
        private readonly IDiaItinerarioService _diaItinerarioService;

        public DiaItinerarioController(IDiaItinerarioService diaItinerarioService)
        {
            _diaItinerarioService = diaItinerarioService;
        }

        [HttpGet("itinerarios/{itinerarioId:guid}/dias")]
        public async Task<ActionResult<IEnumerable<DiaItinerarioResponseDTO>>> ObterPorItinerario(Guid itinerarioId)
        {
            var dias = await _diaItinerarioService.ObterPorItinerarioIdAsync(itinerarioId);
            return Ok(dias);
        }

        [HttpGet("dias/{id:guid}")]
        public async Task<ActionResult<DiaItinerarioResponseDTO>> ObterPorId(Guid id)
        {
            var dia = await _diaItinerarioService.ObterPorIdAsync(id);
            return dia is null ? NotFound() : Ok(dia);
        }
    }
}