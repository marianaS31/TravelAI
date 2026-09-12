using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TravelAI.Interfaces;
using TravelAI.Options;

namespace TravelAI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwt;

        public TokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwt = jwtOptions.Value;
        }

        public string GerarToken(Guid utilizadorId, string email, string nome)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, nome)
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
