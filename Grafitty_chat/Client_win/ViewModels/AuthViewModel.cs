using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Client_win.Misc;
using Client_win.Models;

namespace Client_win.ViewModels
{
    class AuthViewModel : BaseViewModel
    {
        private Chat chat;
        public AuthViewModel()
        {
            try
            {
                chat = new TcpChat(IPAddress.Parse("192.168.1.151"), 8888);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                CloseApp();
            }
            
            chat.OnLoginComplete += LoginComplete;
            chat.OnLoginFailure += LoginFailure;

            IsEnableLoginButton = true;
            OnPropertyChanged("IsEnabledLoginButton");

            LoginCommand = new RelayCommand(x =>
            {
                chat.Authorize(LoginField, PasswordField);
                IsEnableLoginButton = false;
                OnPropertyChanged("IsEnableLoginButton");
                //WindowService.ShowWindow(new MainViewModel(chat));
            });
        }

        
        private void LoginComplete(string user)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainViewModel = new MainViewModel(chat);
                WindowService.ShowWindow(mainViewModel);
                WindowService.CloseWindow(this);
            });
        }

        private void LoginFailure()
        {
            IsEnableLoginButton = true;
            OnPropertyChanged("IsEnableLoginButton");
            MessageBox.Show("Authorization was failed", "Auth fail!");
        }

        public bool IsEnableLoginButton { get; set; }

        public string LoginField { get; set; }

        public string PasswordField { get; set; }

        public RelayCommand LoginCommand { get; set; }
    }
}
