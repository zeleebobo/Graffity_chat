using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Models
{
    class TcpChatServer : ChatServer
    {
        private TcpListener tcpListener;
        public TcpChatServer(int port) : base(port)
        {

        }

        public override void Disconnect()
        {
            IsRunning = false;
            foreach (var chatClient in Clients)
            {
                chatClient.Disconnect();
            }
            tcpListener.Stop();
            Console.WriteLine("Server was stoped");
        }

        public override void StartListen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Port);
                tcpListener.Start();
                Console.WriteLine("Server started");
                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Client connected");
                    var tcpChatClient = new TcpChatClient(tcpClient, this);
                    var clientThread = new Thread(tcpChatClient.Process);
                    clientThread.Start();
                    clientThread.Name = "Client: " + tcpChatClient.EndPoint;
                    clientsThreads[tcpChatClient] = clientThread;
                    //Clients.Add(tcpChatClient);
                }

            }
            catch (Exception e) // TODO: Fix this
            {
                Console.WriteLine(e);
                Disconnect();
            }
        }
    }
}
