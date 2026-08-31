using TravelAI.Data;
using TravelAI.DTOs;
using TravelAI.Models;
using TravelAI.Interfaces;

namespace TravelAI.Services
{
    public class ViagemService : IViagem
    {

        private readonly TravelAIContext _context;
        private readonly ILogger<ViagemService> _logger;

        public ViagemService(TravelAIContext context, ILogger<ViagemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ViagemResponseDTO?> ObterPorIdAsync(Guid id)
        {
            var viagem = await _context.Viagens.FindAsync(id);
            return viagem is null ? null : MapToDto(viagem);
        }

        public async Task<IEnumerable<ViagemResponseDTO>> ObterTodasAsync()
        {
            var viagens = await _context.Viagens
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
                AtualizadoEm = agora
            };

            _context.Viagens.Add(viagem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Viagem {Id} criada para destino {Destino}", viagem.Id, viagem.Destino);
            return MapToDto(viagem);
        }

        public async Task<ViagemResponseDTO?> AtualizarAsync(Guid id, ViagemUpdateDTO dto)
        {
            var viagem = await _context.Viagens.FindAsync(id);
            if (viagem is null) return null;

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

            _context.Viagens.Remove(viagem);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ViagemResponseDTO MapToDto(Viagem v) => new(
            v.Id, v.Titulo, v.Destino, v.DataInicio, v.DataFim,
            v.NumViajantes, v.Orcamento, v.Estado, v.CriadoEm, v.AtualizadoEm);
    }
}
}
