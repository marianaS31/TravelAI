namespace TravelAI.DTOs
{
    public record RegistarRequestDTO(string Nome, string Email, string Password);
    public record LoginRequestDTO(string Email, string Password);
    public record AuthResponseDTO(string Token, Guid UtilizadorId, string Nome, string Email);
}
