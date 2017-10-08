using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Client_win.Models
{
    class ChatMessage
    {
        public ChatMessage(string text, DateTime time, string author)
        {
            Text = text;
            Time = time;
            Author = author;
        }

        public DateTime Time { get; }
        public string Author { get; }
        public string Text { get; }
        
    }
}
