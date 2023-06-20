using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.BusinessLogic.AuthManagers.Contracts
{
    public interface IJwtManager
    {
        string GenerateJwtToken(int userId);
        bool IsValidAuthToken(string token);
    }
}
