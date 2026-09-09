using TravelAI.Models;

namespace TravelAI.DTOs
{
    public class ViagemCreateDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int NumViajantes { get; set; }
        public decimal Orcamento { get; set; }
    }

    public class ViagemUpdateDTO
    {
        public string? Titulo { get; set; }
        public string? Destino { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? NumViajantes { get; set; }
        public decimal? Orcamento { get; set; }
        public EstadoViagem? Estado { get; set; }
    }

    public class ViagemResponseDTO
    {
        public ViagemResponseDTO(Guid id, string titulo, string destino, DateTime dataInicio, DateTime dataFim, int numViajantes, decimal orcamento, EstadoViagem estado, DateTime criadoEm, DateTime atualizadoEm)
        {
            Id = id;
            Titulo = titulo;
            Destino = destino;
            DataInicio = dataInicio;
            DataFim = dataFim;
            NumViajantes = numViajantes;
            Orcamento = orcamento;
            Estado = estado;
            CriadoEm = criadoEm;
            AtualizadoEm = atualizadoEm;
        }

        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Destino { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int NumViajantes { get; set; }
        public decimal Orcamento { get; set; }
        public EstadoViagem Estado { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}