using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace Server.Models
{
    class User
    {
        private string password;

        public User(string login, string password)
        {
            Login = login;
            this.password = password;
        }
        public string Login { get; }

        public bool CheckPassword(string pass)
        {
            return password.Equals(pass);
        }
    }
}
