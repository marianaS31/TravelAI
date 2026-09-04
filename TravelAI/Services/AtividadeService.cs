using Microsoft.EntityFrameworkCore;
using TravelAI.Controllers;
using TravelAI.Data;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Services
{
    public class AtividadeService : IAtividadeService
    {
        private readonly TravelAIContext _context;

        public AtividadeService(TravelAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AtividadeResponseDTO>> ObterPorDiaIdAsync(Guid diaId)
        {
            var atividades = await _context.Atividades
                .Where(a => a.DiaItinerarioId == diaId)
                .OrderBy(a => a.Ordem)
                .ToListAsync();

            return atividades.Select(MapToDto);
        }

        public async Task<AtividadeResponseDTO> AdicionarAsync(Guid diaId, AtividadeCreateDTO dto)
        {
            var maiorOrdem = await _context.Atividades
                .Where(a => a.DiaItinerarioId == diaId)
                .Select(a => (int?)a.Ordem)
                .MaxAsync() ?? -1;

            var atividade = new Atividade
            {
                Id = Guid.NewGuid(),
                DiaItinerarioId = diaId,
                Ordem = maiorOrdem + 1,
                Nome = dto.Nome,
                Tipo = dto.Tipo,
                HoraInicio = dto.HoraInicio,
                HoraFim = dto.HoraFim,
                Local = dto.Local,
                Detalhes = dto.Detalhes
            };

            _context.Atividades.Add(atividade);
            await _context.SaveChangesAsync();

            return MapToDto(atividade);
        }

        public async Task<AtividadeResponseDTO?> AtualizarAsync(Guid atividadeId, AtividadeUpdateDTO dto)
        {
            var atividade = await _context.Atividades.FindAsync(atividadeId);
            if (atividade is null) return null;

            if (dto.Nome is not null) atividade.Nome = dto.Nome;
            if (dto.Tipo.HasValue) atividade.Tipo = dto.Tipo.Value;
            if (dto.HoraInicio is not null) atividade.HoraInicio = dto.HoraInicio;
            if (dto.HoraFim is not null) atividade.HoraFim = dto.HoraFim;
            if (dto.Local is not null) atividade.Local = dto.Local;
            if (dto.Detalhes is not null) atividade.Detalhes = dto.Detalhes;

            await _context.SaveChangesAsync();
            return MapToDto(atividade);
        }

        public async Task<bool> RemoverAsync(Guid atividadeId)
        {
            var atividade = await _context.Atividades.FindAsync(atividadeId);
            if (atividade is null) return false;

            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReordenarAsync(Guid diaId, ReordenarAtividadesDTO dto)
        {
            var atividades = await _context.Atividades
                .Where(a => a.DiaItinerarioId == diaId)
                .ToListAsync();

            if (atividades.Count != dto.OrdemAtividadeIds.Count) return false;

            for (int i = 0; i < dto.OrdemAtividadeIds.Count; i++)
            {
                var atividade = atividades.FirstOrDefault(a => a.Id == dto.OrdemAtividadeIds[i]);
                if (atividade is null) return false;
                atividade.Ordem = i;
            }

            await _context.SaveChangesAsync();
            return true;
        }

  
        private static AtividadeResponseDTO MapToDto(Atividade a) => new()
        {
            Id = a.Id,
            Ordem = a.Ordem,
            Nome = a.Nome,
            Tipo = a.Tipo,
            HoraInicio = a.HoraInicio,
            HoraFim = a.HoraFim,
            Local = a.Local,
            Detalhes = a.Detalhes
        };

    }
}