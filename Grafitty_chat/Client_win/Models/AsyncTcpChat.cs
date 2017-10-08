using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client_win.Models
{
    public class StateObject
    {
        public NetworkStream netStream;
        public const int BufferSize = 4096;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class AsyncTcpChat : Chat
    {
        private TcpClient tcpClient;

        private Thread receiveThread;
        // private NetworkStream netStream;

        private readonly ManualResetEvent connectDone;
        private readonly ManualResetEvent sendDone;
        private readonly ManualResetEvent recieveDone;

        public AsyncTcpChat(IPAddress ip, int port) : base(ip, port)
        {
            connectDone = new ManualResetEvent(false);
            sendDone = new ManualResetEvent(false);
            recieveDone = new ManualResetEvent(false);

            Connect(ip, port);

            StartRecievingMessages();
        }

        protected override void SendData(string stringData)
        {
            var data = Encoding.UTF8.GetBytes(stringData);
            var netStream = tcpClient.GetStream();
            netStream.BeginWrite(data, 0, data.Length, SendCallback, tcpClient);
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                var client = (TcpClient) result.AsyncState;
                int bytesSent = client.Client.EndSend(result);
                sendDone.Set();
                //MessageBox.Show($"Bytes send: {bytesSent}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void Connect(IPAddress ip, int port)
        {
            IsConnected = true;

            try
            {
                tcpClient = new TcpClient(AddressFamily.InterNetwork);
                connectDone.Reset();
                tcpClient.BeginConnect(ip, port, ConnectCallback, tcpClient);
                connectDone.WaitOne();

                /*SendData("test<EOF>");
                sendDone.WaitOne();*/
            }
            catch (SocketException socketException)
            {
                Disconnect();
                Console.WriteLine(socketException);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Disconnect();
                Console.WriteLine(objectDisposedException);
            }
            catch (Exception e)
            {
                Disconnect();
                Console.WriteLine(e);
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            var client = (TcpClient) result.AsyncState;
            client.EndConnect(result);

            Console.WriteLine("Socket connected to {0}", client.Client.RemoteEndPoint);//StartRecievingMessages();
            
            connectDone.Set();
        }

        protected override void StartRecievingMessages()
        {
            receiveThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (!tcpClient.GetStream().DataAvailable) continue;
                    Receive();
                    recieveDone.WaitOne();
                }
            });
            receiveThread.Start();
        }

        private void Receive()
        {
            var state = new StateObject();
            try
            {
                state.netStream = tcpClient.GetStream();
                state.netStream.BeginRead(state.buffer, 0, StateObject.BufferSize, RecieveCallback, state);
            }
            catch (ObjectDisposedException disposedException)
            {
                MessageBox.Show(disposedException.Message);
                Disconnect();
            }
            catch (IOException io)
            {
                MessageBox.Show(io.Message, "Server is dead!");
                Disconnect();
            }
            catch (Exception e)
            {
                Disconnect();
                Console.WriteLine(e);
            }
        }

        private void RecieveCallback(IAsyncResult result)
        {
            var state = (StateObject) result.AsyncState;
            var netStream = state.netStream;

            int bytesRead = netStream.EndRead(result);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                if (netStream.DataAvailable)
                    netStream.BeginRead(state.buffer, 0, StateObject.BufferSize, RecieveCallback, state);
            }
            if (bytesRead != 0 && netStream.DataAvailable) return;
            if (state.sb.Length > 1)
                HandleMessage(state.sb.ToString());
            recieveDone.Set();
        }

        protected override void Disconnect()
        {
            IsConnected = false;
            try
            {
                receiveThread?.Abort();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //tcpClient.GetStream().Close();
            tcpClient.Close();
        }
    }
}
