using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.BusinessLogic.AuthManagers.Contracts
{
    public interface IValidationManager
    {
        bool ValidatePassword(string password);
        bool ValidateEmail(string email);
    }
}
