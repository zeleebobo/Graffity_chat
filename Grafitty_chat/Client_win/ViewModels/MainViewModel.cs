using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using Client_win.Misc;
using Client_win.Models;

namespace Client_win.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private Chat chat;
        public MainViewModel(Chat tcpChat)
        {
            chat = tcpChat;
            chat.OnMessageRecieved += HandleRecievedMessage;
            chat.OnStrokeRecieved += HandleRecievedStroke;
            chat.OnUserConnected += user => 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (OnlineUsers.Contains(user)) return;
                    OnlineUsers.Add(user);
                });  
            };
            chat.OnUserLogout += user =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!OnlineUsers.Contains(user)) return;
                    OnlineUsers.Remove(user);
                });
            };
            chat.OnUsersListReceived += users =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var user in users)
                    {
                        if (OnlineUsers.Contains(user)) continue;
                        OnlineUsers.Add(user);
                    }
                });
            };

            Messages = new ObservableCollection<ChatMessage>();
            Strokes = new ObservableCollection<Stroke>();
            OnlineUsers = new ObservableCollection<string>();

            OpenDrawRoomCommand = new RelayCommand(x => { WindowService.ShowWindow(new DrawingRoomViewModel(chat));});
            SendMessageCommand = new RelayCommand(x =>
            {
                if (!chat.IsConnected)
                {
                    MessageBox.Show("Connection Failed");
                    return;
                }
                if (string.IsNullOrWhiteSpace(TextField)) return;
                chat.Send(TextField);
                TextField = "";
                OnPropertyChanged("TextField");
            });
        }

        public RelayCommand OpenDrawRoomCommand { get; set; }
        public ObservableCollection<ChatMessage> Messages { get; }
        public ObservableCollection<Stroke> Strokes { get; }

        public string UserName => chat.CurrentUser;

        private void HandleRecievedMessage(ChatMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }

        private void HandleRecievedStroke(Stroke stroke)
        {
            Strokes.Add(stroke);
        }

        public RelayCommand SendMessageCommand { get; set; }

        public string TextField { get; set; }

        public ObservableCollection<string> OnlineUsers { get; set; }
    }
}
