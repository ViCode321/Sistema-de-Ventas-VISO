using project_DBA_VISO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Services.Contract
{
    public interface IUserService
    {
        Task<User> GetUser(string username, string key);

        Task<User> SaveUser(User user);
    }
}
