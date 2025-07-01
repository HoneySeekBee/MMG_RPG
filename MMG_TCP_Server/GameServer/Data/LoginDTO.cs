using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class LoginDTO
    {
        public class JwtValidationResult
        {
            public bool IsValid { get; set; }
            public string Reason { get; set; }
            public int UserId { get; set; }
            public string Email { get; set; }
            public string Nickname { get; set; }
        }
    }
}
