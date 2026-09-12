using TravelAI.DTOs;

namespace TravelAI.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegistarAsync(RegistarRequestDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO dto);
    }
}
