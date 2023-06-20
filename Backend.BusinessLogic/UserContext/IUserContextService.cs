using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.BusinessLogic.UserContext
{
    public interface IUserContextService
    {
        int GetCurrentUserId();
        bool IsUserLoggedIn();
    }
}
