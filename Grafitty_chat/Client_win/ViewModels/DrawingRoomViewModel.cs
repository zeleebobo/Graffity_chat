using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Client_win.Misc;
using Client_win.Models;

namespace Client_win.ViewModels
{
    class DrawingRoomViewModel : BaseViewModel
    {
        private Chat chat;

        private StrokeCollection strokes;
        public DrawingRoomViewModel(Chat chat)
        {
            this.chat = chat;
            strokes = new StrokeCollection();
            chat.OnStrokeRecieved += StrokeHandle;

            NewStrokeCommand = new RelayCommand(x =>
            {
                var stroke = Strokes.LastOrDefault();
                Strokes.Remove(stroke);
                chat.Send(stroke);
                //MessageBox.Show(stroke?.ToString());
            });

            InkCanvasDrawingAttributes = new DrawingAttributes();
            InkCanvasDrawingAttributes.Color = Color.FromArgb(255,0,0,0);
            InkCanvasDrawingAttributes.Height = 3;
            InkCanvasDrawingAttributes.Width = 3;
            OnPropertyChanged("InkCanvasDrawingAttributes");
        }

        private void StrokeHandle(Stroke stroke)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Strokes.Add(stroke);
            });
        }

        public DrawingAttributes InkCanvasDrawingAttributes { get; set; }

        public StrokeCollection Strokes
        {
            get { return strokes; }
        }

        public RelayCommand NewStrokeCommand { get; set; }
    }
}
