using Microsoft.EntityFrameworkCore;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Services.Inplementation
{
    public class UserService : IUserService
    {
        private readonly DBContext _context;

        public UserService(DBContext context)
        {
            _context = context;
        }

        public async Task<User> GetUser(string username, string key)
        {
            User user_find = await _context.Users
                .Include(u => u.User_Rol)
                .Where(u => u.Nombre == username && u.Contraseña == key && u.Status == true)
                .FirstOrDefaultAsync();
            return user_find;
        }


        public async Task<User> SaveUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
