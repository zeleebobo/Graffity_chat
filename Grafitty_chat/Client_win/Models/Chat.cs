using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Linq;
using Client_win.Misc;

namespace Client_win.Models
{
    abstract class Chat : IDisposable
    {

        // TODO: 
        // 1. XML Parser
        // 2. ViewModels
        // 3. Server

        #region Events

        public event Action<string> OnLoginComplete = x => {};
        public event Action OnLoginFailure = () => {};

        public event Action<ChatMessage> OnMessageRecieved = x => {};
        public event Action<Stroke> OnStrokeRecieved = x => {};
        public event Action<IEnumerable<string>> OnUsersListReceived = x => { };

        public event Action<string> OnUserLogout = x => { };
        public event Action<string> OnUserConnected = x => { };

        #endregion

        protected Chat(IPAddress ip, int port)
        {
            ServerIp = ip;
            ServerPort = port;
            Connect(ip, port);
            Messages = new ObservableCollection<ChatMessage>();
            Users = new ObservableCollection<string>();

            OnLoginComplete += user => { CurrentUser = user; };
        }

        #region Absrtract Methods

        protected abstract void SendData(string stringData);

        protected abstract void Connect(IPAddress ip, int port);

        protected abstract void StartRecievingMessages();

        protected abstract void Disconnect();

        #endregion


        public void Authorize(string login, string password)
        {
            var xmlrequest = XmlConverter.CreateLoginRequest(login, password);
            SendData(xmlrequest.ToString());
        }

        public bool Register(string login, string password)
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            var xmlMessage = new ChatMessage(message, DateTime.Today, null).ToXml();
            SendData(xmlMessage.ToString());
        }

        public void Send(Stroke stroke)
        {
            if (stroke == null) return;
            var xmlStroke = stroke.ToXml();
            //MessageBox.Show(xmlStroke.ToString());
            SendData(xmlStroke.ToString());
        }
        
        public void HandleMessage(string messageFromServer)
        {
            var messages = messageFromServer.Split(new[] {"<<EOFS>>"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var message in messages)
            {
                if (message.Length < 7) continue;
                XDocument xDoc = null;
                try
                {
                    xDoc = XDocument.Parse(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                var rootName = xDoc?.Root?.Name.ToString();
                switch (rootName)
                {
                    case "stroke":
                        OnStrokeRecieved?.Invoke(XmlConverter.GetStroke(xDoc));
                        break;
                    case "message":
                        OnMessageRecieved?.Invoke(XmlConverter.GetMessage(xDoc));
                        break;
                    case "new_user":
                        OnUserConnected?.Invoke(XmlConverter.GetUser(xDoc));
                        break;
                    case "user_logout":
                        OnUserLogout?.Invoke(XmlConverter.GetUser(xDoc));
                        break;
                    case "users_list":
                        OnUsersListReceived?.Invoke(XmlConverter.GetUsers(xDoc));
                        break;
                    case "login_response":
                        var user = XmlConverter.GetAuthResult(xDoc);
                        if (string.IsNullOrEmpty(user))
                        {
                            OnLoginFailure?.Invoke();
                        }
                        else
                        {
                            OnLoginComplete?.Invoke(user);
                        }
                        break;
                    default:
                        break;
                }
            }
            
        }

        public void ReConnect()
        {
            Connect(ServerIp, ServerPort);
        }

        public void Dispose()
        {
            Disconnect();
        }

        public IEnumerable<string> Users { get; protected set; }

        public IPAddress ServerIp { get; }

        public int ServerPort { get; }

        public bool IsConnected { get; protected set; }

        public ICollection<ChatMessage> Messages { get; protected set; }

        public string CurrentUser { get; protected set; }

    }
}
