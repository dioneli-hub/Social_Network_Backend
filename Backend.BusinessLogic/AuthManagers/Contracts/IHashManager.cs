using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.BusinessLogic.AuthManagers.Models;

namespace Backend.BusinessLogic.AuthManagers.Contracts
{
    public interface IHashManager
    {
        HashModel Generate(string password);
        byte[] HashPassword(string password, byte[] salt);
    }
}
