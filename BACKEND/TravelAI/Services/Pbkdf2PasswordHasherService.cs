using System.Security.Cryptography;
using TravelAI.Interfaces;

namespace TravelAI.Services
{
    // PBKDF2 com salt aleatório por password. Formato armazenado:
    // "iterações.salt(base64).chave(base64)" — auto-contido, sem depender
    // de configuração externa para verificar hashes antigos no futuro.
    public class Pbkdf2PasswordHasherService : IPasswordHasher
    {
        private const int Iterations = 350_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool Verify(string password, string hash)
        {
            var partes = hash.Split('.');
            if (partes.Length != 3) return false;

            if (!int.TryParse(partes[0], out var iterations)) return false;

            var salt = Convert.FromBase64String(partes[1]);
            var chaveEsperada = Convert.FromBase64String(partes[2]);
            var chaveCalculada = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, HashAlgorithmName.SHA256, chaveEsperada.Length);

            return CryptographicOperations.FixedTimeEquals(chaveCalculada, chaveEsperada);
        }
    }
}
