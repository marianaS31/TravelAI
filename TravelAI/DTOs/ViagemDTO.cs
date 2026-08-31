using TravelAI.Models;

namespace TravelAI.DTOs
{
    public class ViagemCreateDTO
    {
        public string Titulo { get; set; }
        public string Destino { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int NumViajantes { get; set; }
        public decimal Orcamento { get; set; }
    }
    public class ViagemUpdateDTO
    {
        public string Titulo { get; set; }  
        public string Destino { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int NumViajantes { get; set; }
        public decimal Orcamento { get; set; }
        public EstadoViagem? Estado { get; set; }
    }

    public class ViagemResponseDTO
    {
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
