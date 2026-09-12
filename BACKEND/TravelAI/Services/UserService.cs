using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TravelAI.Interfaces;

namespace TravelAI.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UtilizadorId
        {
            get
            {
                var valor = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(valor, out var id) ? id : null;
            }
        }
    }
}
