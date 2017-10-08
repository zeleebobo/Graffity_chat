using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Client_win.Models;

namespace Client_win.Misc
{
    static class XmlConverter
    {
        public static XDocument ToXml(this ChatMessage message)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("message", 
                            new XElement("text", message.Text));
            doc.Add(root);
            return doc;
        }

        public static XDocument CreateLoginRequest(string login, string password)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("login_request",
                            new XElement("login", login),
                            new XElement("password", password));
            doc.Add(root);
            return doc;
        }
        

        public static XDocument ToXml(this Stroke stroke)
        {
            var points = stroke.StylusPoints;
            var color = stroke.DrawingAttributes.Color;

            var pointsStr = string.Join(",", points.Select(x => x.X + " " + x.Y));
            var colorStr = color.A + " " + color.R + " " + color.G + " " + color.B;
            var lineWidth = stroke.DrawingAttributes.Width;
            var lineHeight = stroke.DrawingAttributes.Height;


            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("stroke", 
                            new XElement("points", pointsStr),
                            new XElement("color", colorStr),
                            new XElement("width", lineWidth),
                            new XElement("height", lineHeight));
            doc.Add(root);

            return doc;
        }

        public static Stroke GetStroke(XDocument doc)
        {

            var xmlStroke = doc.Root/*?.Element("stroke")*/;

            var pointsStr = xmlStroke?.Element("points")?.Value;
            var colorStr = xmlStroke?.Element("color")?.Value;

            var colorArgb = colorStr?.Split(' ').Select(x => Convert.ToByte(x)).ToArray();
            if (colorArgb?.Length != 4) throw new InvalidDataException();
            var color = Color.FromArgb(colorArgb[0], colorArgb[1], colorArgb[2], colorArgb[3]);

            var heightStr = xmlStroke?.Element("height")?.Value;
            var widthStr = xmlStroke?.Element("width")?.Value;

            double width = double.Parse(widthStr, CultureInfo.InvariantCulture);
            double height = double.Parse(heightStr, CultureInfo.InvariantCulture);

            if (pointsStr == null) throw new InvalidDataException();
            var pointsList = pointsStr.Split(',');

            var stroke = new Stroke(new StylusPointCollection(pointsList.Select(x =>
                {
                    var strPoint = x.Split(' ');
                    return new Point(Convert.ToDouble(strPoint[0]), Convert.ToDouble(strPoint[1]));
                })))
                { DrawingAttributes = { Color = color, FitToCurve = true, Width = width, Height = height } };

            return stroke;

        }

        public static ChatMessage GetMessage(XDocument doc)
        {
            var xmlMessage = doc.Root;
            if (xmlMessage?.Name != "message") return null;
            var text = xmlMessage.Element("text")?.Value;
            var time = DateTime.Parse(xmlMessage.Element("time")?.Value);
            var author = xmlMessage.Element("author")?.Value;
            return new ChatMessage(text, time, author);
        }

        public static string GetAuthResult(XDocument doc)
        {
            var xmlMessage = doc.Root;
            if (xmlMessage?.Name != "login_response") return null;
            var result = xmlMessage.Element("login_success");
            return result?.Element("login")?.Value;
        }

        public static IEnumerable<string> GetUsers(XDocument doc)
        {
            var usersList = doc.Root;
            if (usersList?.Name != "users_list") return null;
            return usersList.Elements("login").Select(x => x.Value).ToArray();
        }

        public static string GetUser(XDocument doc)
        {
            var user = doc.Root;
            if (user?.Name != "user_logout" && user?.Name != "new_user") return null;
            return user?.Element("login")?.Value;
        }

    }
}
