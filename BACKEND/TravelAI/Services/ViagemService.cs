using TravelAI.Data;
using TravelAI.DTOs;
using TravelAI.Models;
using TravelAI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TravelAI.Services
{
    public class ViagemService : IViagemService
    {
        private readonly TravelAIContext _context;
        private readonly ILogger<ViagemService> _logger;
        private readonly IUserService _currentUser;

        public ViagemService(TravelAIContext context, ILogger<ViagemService> logger, IUserService currentUser)
        {
            _context = context;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<ViagemResponseDTO?> ObterPorIdAsync(Guid id)
        {
            var viagem = await _context.Viagens.FindAsync(id);
            if (viagem is null) return null;

            // Viagens sem dono (legado, anteriores a esta funcionalidade) continuam
            // acessíveis a qualquer utilizador autenticado; com dono, só o dono vê.
            if (viagem.UtilizadorId is not null && viagem.UtilizadorId != _currentUser.UtilizadorId)
                return null;

            return MapToDto(viagem);
        }

        public async Task<IEnumerable<ViagemResponseDTO>> ObterTodasAsync()
        {
            var utilizadorId = _currentUser.UtilizadorId;

            var query = _context.Viagens.AsQueryable();
            if (utilizadorId is not null)
            {
                query = query.Where(v => v.UtilizadorId == utilizadorId);
            }

            var viagens = await query
                .OrderByDescending(v => v.CriadoEm)
                .ToListAsync();

            return viagens.Select(MapToDto);
        }

        public async Task<ViagemResponseDTO> CriarAsync(ViagemCreateDTO dto)
        {
            var agora = DateTime.UtcNow;
            var viagem = new Viagem
            {
                Id = Guid.NewGuid(),
                Titulo = dto.Titulo,
                Destino = dto.Destino,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                NumViajantes = dto.NumViajantes,
                Orcamento = dto.Orcamento,
                Estado = EstadoViagem.Planeamento,
                CriadoEm = agora,
                AtualizadoEm = agora,
                UtilizadorId = _currentUser.UtilizadorId
            };

            _context.Viagens.Add(viagem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Viagem {Id} criada para destino {Destino} (utilizador {UtilizadorId})",
                viagem.Id, viagem.Destino, viagem.UtilizadorId);
            return MapToDto(viagem);
        }

        public async Task<ViagemResponseDTO?> AtualizarAsync(Guid id, ViagemUpdateDTO dto)
        {
            var viagem = await _context.Viagens.FindAsync(id);
            if (viagem is null) return null;
            if (viagem.UtilizadorId is not null && viagem.UtilizadorId != _currentUser.UtilizadorId) return null;

            if (dto.Titulo is not null) viagem.Titulo = dto.Titulo;
            if (dto.Destino is not null) viagem.Destino = dto.Destino;
            if (dto.DataInicio.HasValue) viagem.DataInicio = dto.DataInicio.Value;
            if (dto.DataFim.HasValue) viagem.DataFim = dto.DataFim.Value;
            if (dto.NumViajantes.HasValue) viagem.NumViajantes = dto.NumViajantes.Value;
            if (dto.Orcamento.HasValue) viagem.Orcamento = dto.Orcamento.Value;
            if (dto.Estado.HasValue) viagem.Estado = dto.Estado.Value;

            viagem.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(viagem);
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var viagem = await _context.Viagens.FindAsync(id);
            if (viagem is null) return false;
            if (viagem.UtilizadorId is not null && viagem.UtilizadorId != _currentUser.UtilizadorId) return false;

            _context.Viagens.Remove(viagem);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ViagemResponseDTO MapToDto(Viagem v) => new(
            v.Id, v.Titulo, v.Destino, v.DataInicio, v.DataFim,
            v.NumViajantes, v.Orcamento, v.Estado, v.CriadoEm, v.AtualizadoEm);
    }
}