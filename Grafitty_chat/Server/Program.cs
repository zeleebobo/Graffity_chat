using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Models;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new TcpChatServer(8888);
            server.StartListen();

            Console.Read();

            // TODO: 
            // 1. add xml handle messages
            // 2. online time
            // 3/ update users
        }
    }
}
