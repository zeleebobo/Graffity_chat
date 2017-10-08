using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Client_win.Misc;
using Client_win.Models;
using Client_win.ViewModels;

namespace Client_win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            WindowService.ShowWindow(new AuthViewModel());
            //WindowService.ShowWindow(new MainViewModel(new TcpChat(IPAddress.Parse("127.0.0.1"), 8888)));
        }
    }
}
