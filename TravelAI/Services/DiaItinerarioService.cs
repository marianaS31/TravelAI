using Microsoft.EntityFrameworkCore;
using TravelAI.Data;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Services
{
    public class DiaItinerarioService : IDiaItinerarioService
    {
        private readonly TravelAIContext _context;

        public DiaItinerarioService(TravelAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DiaItinerarioResponseDTO>> ObterPorItinerarioIdAsync(Guid itinerarioId)
        {
            var dias = await _context.DiasItinerario
                .Include(d => d.Atividades)
                .Include(d => d.PrevisaoTempo)
                .Where(d => d.ItinerarioId == itinerarioId)
                .OrderBy(d => d.NumeroDia)
                .ToListAsync();

            return dias.Select(MapToDto);
        }

        public async Task<DiaItinerarioResponseDTO?> ObterPorIdAsync(Guid diaId)
        {
            var dia = await _context.DiasItinerario
                .Include(d => d.Atividades)
                .Include(d => d.PrevisaoTempo)
                .FirstOrDefaultAsync(d => d.Id == diaId);

            return dia is null ? null : MapToDto(dia);
        }

     
        private static DiaItinerarioResponseDTO MapToDto(DiaItinerario d)
        {
            var previsaoDto = d.PrevisaoTempo is null
                ? null
                : new PrevisaoTempoResponseDTO(
                    d.PrevisaoTempo.TempMax,
                    d.PrevisaoTempo.TempMin,
                    d.PrevisaoTempo.Condicao,
                    d.PrevisaoTempo.ProbabilidadePrecipitacao);

            var atividadesDto = d.Atividades
                .OrderBy(a => a.Ordem)
                .Select(a => new AtividadeResponseDTO
                {
                    Id = a.Id,
                    Ordem = a.Ordem,
                    Nome = a.Nome,
                    Tipo = a.Tipo,
                    HoraInicio = a.HoraInicio,
                    HoraFim = a.HoraFim,
                    Local = a.Local,
                    Detalhes = a.Detalhes
                })
                .ToList();

            return new DiaItinerarioResponseDTO(d.Id, d.NumeroDia, d.Data, atividadesDto, previsaoDto);
        }

    }
}