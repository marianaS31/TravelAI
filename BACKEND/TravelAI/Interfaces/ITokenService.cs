namespace TravelAI.Interfaces
{
    public interface ITokenService
    {
        string GerarToken(Guid utilizadorId, string email, string nome);
    }
}
