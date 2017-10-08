using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Client_win.ViewModels;
using Client_win.Views;

namespace Client_win.Misc
{
    
    static class WindowService
    {
        private static Dictionary<BaseViewModel, Window> windowsDict = new Dictionary<BaseViewModel, Window>();

        [STAThread]
        public static void ShowWindow(BaseViewModel viewModel)
        {
            if (windowsDict.ContainsKey(viewModel))
            {
                windowsDict[viewModel].Show();
                return;
            }
            
            var newWindow = Activator.CreateInstance(GetWindowInstance(viewModel)) as Window;
            if (newWindow == null) return;
            newWindow.DataContext = viewModel;
            newWindow.Show();
            windowsDict[viewModel] = newWindow;
        }

        public static void CloseWindow(BaseViewModel viewModel)
        {
            if (!windowsDict.ContainsKey(viewModel)) return;
            (windowsDict[viewModel])?.Close();
        }

        private static Type GetWindowInstance(BaseViewModel viewModel)
        {
            if (viewModel is AuthViewModel) return typeof(Auth);
            if (viewModel is DrawingRoomViewModel) return typeof(DrawingRoom);
            if (viewModel is MainViewModel) return typeof(MainWindow);
            return typeof(Window);
        }
    }
}
