using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Server.Models
{
    abstract class ChatServer
    {
        protected Dictionary<ChatClient, Thread> clientsThreads;
        private List<User> userStorage;

        public event Action<string> OnDisconectUser = x => { };

        // TODO: add disconect client events and broadcast it

        protected ChatServer(int port)
        {
            Clients = new List<ChatClient>();
            clientsThreads = new Dictionary<ChatClient, Thread>();
            OnlineUserLogins = new List<string>();
            Port = port;

            userStorage = new List<User>() {new User("l", "1"), new User("petya", "1234"), new User("nikita", "1234"), new User("lena", "1234")};

            OnDisconectUser += user => { Broadcast(XmlServerConverter.UserLogoutResponse(user).ToString());};

            
        }

        public ICollection<User> Users { get; }

        public ICollection<string> OnlineUserLogins { get; }

        public ICollection<ChatClient> Clients { get; }

        public void AddConnection(ChatClient chatClient)
        {
            if (Clients.Contains(chatClient)) return;
            Clients.Add(chatClient);
        }

        public void RemoveConnection(ChatClient chatClient)
        {
            if (!Clients.Contains(chatClient)) return;
            Clients.Remove(chatClient);
            if (!chatClient.IsLogined) return;
            Broadcast(XmlServerConverter.UserLogoutResponse(chatClient.LoginedAs).ToString());
            // stop thread
        }

        public void HandleMessage(ChatClient messageOwner, string messageText)
        {
            XDocument xDoc = null;
            if (messageText != "") 
                xDoc = XDocument.Parse(messageText);
            var rootName = xDoc?.Root?.Name.ToString();
            
            switch (rootName)
            {
                case "stroke":
                    Broadcast(messageText);
                    break;
                case "message":
                    if (!messageOwner.IsLogined) break;
                    Console.WriteLine(messageOwner.LoginedAs + ": " + XmlServerConverter.GetMessage(xDoc));
                    BroadCast(new ChatMessage(XmlServerConverter.GetMessage(xDoc), DateTime.Now, messageOwner.LoginedAs));
                    break;
                case "users_list_request":
                    break;
                case "login_request":
                    var user = Authorization(xDoc);
                    if (user == null)
                    {
                        messageOwner.SendStringData(XmlServerConverter.CreatteFailureLoginResponse().ToString());
                        Console.WriteLine("Icorect login from " + messageOwner.EndPoint);
                    }
                    else // Log in complete
                    {
                        messageOwner.LogIn(user);
                        messageOwner.SendStringData(XmlServerConverter.CreateSuccessLoginResponse(user).ToString()); // message about log in success
                        Console.WriteLine(messageOwner.LoginedAs + " is logined");

                        if (!OnlineUserLogins.Contains(messageOwner.LoginedAs))
                        {
                            Broadcast(XmlServerConverter.NewUserResponse(messageOwner.LoginedAs).ToString(), messageOwner); // new user message
                            //messageOwner.SendStringData(OnlineUserLogins.ToXml().ToString());
                            OnlineUserLogins.Add(messageOwner.LoginedAs);
                        }
                        var users = OnlineUserLogins.Where(x => x != messageOwner.LoginedAs).ToArray();
                        messageOwner.SendStringData(users.ToXml().ToString());
                        Console.WriteLine($"To {messageOwner.LoginedAs} sended: \n {users.ToStr()}");
                        
                    }
                    break;
                default:
                    break;
            }
        }

        private User Authorization(XDocument doc)
        {
            var pass = doc.Root?.Element("password")?.Value;
            var login = doc.Root?.Element("login")?.Value;
            return userStorage.FirstOrDefault(x => x.Login == login && x.CheckPassword(pass));
        }

        protected void StopThreads()
        {
            foreach (var clientsThread in clientsThreads)
            {
                clientsThread.Value?.Abort();
            }
        }

        public void StopClientThread(ChatClient chatClient)
        {
            if (!clientsThreads.ContainsKey(chatClient)) return;
            clientsThreads[chatClient].Abort();
        }

        public void BroadCast(ChatMessage message, ChatClient exceptClient = null)
        {
            var messageStringData = message.ToXml().ToString();
            Broadcast(messageStringData, exceptClient);
        }

        public void Broadcast(string data, ChatClient exceptClient = null)
        {
            foreach (var chatClient in Clients.Where(x => x.IsLogined && x != exceptClient))
            {
                chatClient.SendStringData(data);
            }
        }

        public bool IsRunning { get; protected set; }

        public int Port { get; }

        public abstract void Disconnect();

        public abstract void StartListen();

    }
}
