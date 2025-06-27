using GameServer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class EFUserDB
    {
        public static (bool success, User user) TryLogin(string email, string password)
        {
            using var db = new AppDbContext();

            var user = db.User.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return (false, null);

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return (isPasswordCorrect, isPasswordCorrect ? user : null);
        }

    }
}
