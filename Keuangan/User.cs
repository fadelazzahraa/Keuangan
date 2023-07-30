using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keuangan
{
    public class User
    {
        public string Username { get; set; }
        public string Token { get; set; }

        public User(string username, string token)
        {
            this.Username = username;
            this.Token = token;
        }
    }
}
