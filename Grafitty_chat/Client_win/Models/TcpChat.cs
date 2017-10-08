using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Client_win.Misc;

namespace Client_win.Models
{
    class TcpChat : Chat
    {
        private TcpClient tcpClient;
        private NetworkStream netStream;
        private Thread recievingThread;

        public TcpChat(IPAddress ip, int port) : base(ip, port)
        {
            //Connect(ip, port);
        }

        protected override void Disconnect()
        {
            IsConnected = false;
            ((IDisposable)tcpClient)?.Dispose();
            netStream?.Dispose();
            try
            {
                recievingThread?.Abort();
            }
            catch (ThreadAbortException abortException)
            {
                MessageBox.Show("Recieving thread stopped!");
            }
        }

        
        protected override void Connect(IPAddress ip, int port)
        {
            IsConnected = true;
            if (recievingThread != null && recievingThread.IsAlive)
                return;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(ip, port));
                netStream = tcpClient.GetStream();

                recievingThread = new Thread(StartRecievingMessages);
                //recievingThread.Name = "Recieving Thread";
                recievingThread.Start();
            }
            
            catch( Exception e)
            {
                Disconnect();
            }
            
        }

        protected override void SendData(string stringData)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(stringData);
                netStream.Write(data, 0, data.Length);
            }
            catch (ObjectDisposedException disposedException)
            {
                Disconnect();
                ReConnect();
                SendData(stringData);
                MessageBox.Show(disposedException.Message);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Disconnect();
                ReConnect();
                SendData(stringData);
                MessageBox.Show(invalidOperationException.Message);
            }
        }

        protected override void StartRecievingMessages()
        {
            Task.Run(() => { Receive();});
        }

        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    StringBuilder strBuilder = new StringBuilder();
                    do
                    {
                        var bytes = netStream.Read(buffer, 0, buffer.Length);
                        strBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                    } while (netStream.DataAvailable);
                    HandleMessage(strBuilder.ToString());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Disconnect();
            }
        }

    }
}
