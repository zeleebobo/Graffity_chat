using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    abstract class ChatClient
    {
        private ChatServer chatServer;

        public ChatClient(ChatServer chatServer)
        {
            LoginedAs = "";
            this.chatServer = chatServer;
        }

        public void SendStringData(string data)
        {
            SendData(Encoding.UTF8.GetBytes(data + "<<EOFS>>"));
        }

        public void LogIn(User user)
        {
            LoginedAs = user.Login;
        }

        protected abstract void SendData(byte[] data);

        public abstract void Disconnect();

        public abstract void Process();

        public abstract EndPoint EndPoint { get; }

        public string LoginedAs { get; private set; }

        public bool IsLogined => LoginedAs != "";

    }
}
