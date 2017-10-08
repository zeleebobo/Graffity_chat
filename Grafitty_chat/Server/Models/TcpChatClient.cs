using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    class TcpChatClient : ChatClient
    {
        private NetworkStream netStream;
        private TcpClient tcpClient;

        private ChatServer chatServer;

        public TcpChatClient(TcpClient tcpClient, TcpChatServer chatServer) : base(chatServer)
        {
            this.tcpClient = tcpClient;
            this.chatServer = chatServer;
            chatServer.AddConnection(this);
        }

        protected override void SendData(byte[] data)
        {
            if (!netStream.CanWrite) return;
            netStream.Write(data, 0, data.Length);
        }

        public override void Disconnect()
        {
            
            netStream?.Close();
            tcpClient?.Close();
            chatServer.RemoveConnection(this);
            Console.WriteLine("Client Disconected");
        }

        public override void Process()
        {
            try
            {
                netStream = tcpClient.GetStream();

                while (true)
                {
                    var clientMessage = GetMessage();
                    
                    chatServer.HandleMessage(this, clientMessage);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("IOException on " + EndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Disconnect();
            }
        }

        private string GetMessage()
        {
            var buffer = new byte[128];
            var stringBuilder = new StringBuilder();
            do
            {
                var bytes = netStream.Read(buffer, 0, buffer.Length);
                stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
            } while (netStream.DataAvailable);

            return stringBuilder.ToString();
        }

        public override EndPoint EndPoint => tcpClient.Client.RemoteEndPoint;
    }
}
