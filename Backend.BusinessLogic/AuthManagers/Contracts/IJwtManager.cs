using Backend.ApiModels;

namespace Backend.BusinessLogic.AuthManagers.Contracts
{
    public interface IJwtManager
    {
        TokenModel GenerateJwtToken(int userId);
        bool IsValidAuthToken(string token);
    }
}
