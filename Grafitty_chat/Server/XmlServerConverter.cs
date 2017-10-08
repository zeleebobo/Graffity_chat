using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Server.Models;

namespace Server
{
    static class XmlServerConverter
    {
        public static XDocument ToXml(this ChatMessage message)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("message", 
                         new XElement("text", message.Text),
                         new XElement("time", message.Time.ToString()),
                         new XElement("author", message.Author));
            doc.Add(root);
            return doc;
        }

        public static XDocument ToXml(this IEnumerable<string> users)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("users_list");
            foreach (var user in users)
            {
                var userEntry = new XElement("login", user);
                root.Add(userEntry);
            }
            doc.Add(root);
            return doc;
        }

        public static string ToStr(this IEnumerable<string> users)
        {
            return string.Join("\n\t", users);
        }

        public static XDocument UserLogoutResponse(string user)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("user_logout",
                            new XElement("login", user));
            doc.Add(root);
            return doc;
        }

        public static XDocument NewUserResponse(string user)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("new_user",
                            new XElement("login", user));
            doc.Add(root);
            return doc;
        }


        public static string GetMessage(XDocument doc)
        {
            var root = doc.Element("message");
            return root?.Element("text")?.Value;
        }

        public static XDocument CreateSuccessLoginResponse(User user)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("login_response",
                            new XElement("login_success", 
                                new XElement("login", user.Login)));
            doc.Add(root);
            return doc;
        }

        public static XDocument CreatteFailureLoginResponse()
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var root = new XElement("login_response",
                            new XElement("login_failure"));
            doc.Add(root);
            return doc;
        }
    }
}
