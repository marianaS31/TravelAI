using Microsoft.EntityFrameworkCore;
using TravelAI.Data;
using TravelAI.DTOs;
using TravelAI.Interfaces;
using TravelAI.Models;

namespace TravelAI.Services
{
    public class AuthService : IAuthService
    {
        private readonly TravelAIContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(TravelAIContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDTO> RegistarAsync(RegistarRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Email e password são obrigatórios.");

            var existe = await _context.Utilizadores.AnyAsync(u => u.Email == dto.Email);
            if (existe)
                throw new InvalidOperationException("Já existe uma conta com este email.");

            var utilizador = new Utilizador
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                CriadoEm = DateTime.UtcNow
            };

            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();

            var token = _tokenService.GerarToken(utilizador.Id, utilizador.Email, utilizador.Nome);
            return new AuthResponseDTO(token, utilizador.Id, utilizador.Nome, utilizador.Email);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO dto)
        {
            var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (utilizador is null || !_passwordHasher.Verify(dto.Password, utilizador.PasswordHash))
                throw new InvalidOperationException("Email ou password inválidos.");

            var token = _tokenService.GerarToken(utilizador.Id, utilizador.Email, utilizador.Nome);
            return new AuthResponseDTO(token, utilizador.Id, utilizador.Nome, utilizador.Email);
        }
    }
}
