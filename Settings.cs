using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SubredditLinkerBot
{
    static class Settings
    {
        public static string[] SubredditBlacklist;
        public static string[] LinkSubBlacklist;

        public static Dictionary<string, string> Descriptions;

        public static void Load()
        {
            Descriptions = new Dictionary<string, string>();
            XmlDocument xml = new XmlDocument();
            xml.Load("Settings.xml");

            List<string> subBlack = new List<string>();
            List<string> subLinkBlack = new List<string>();

            foreach(XmlNode node in xml.ChildNodes[1].ChildNodes)
            {
                if (node.Attributes["blacklisted"].Value.ToLowerInvariant() == "true")
                    subBlack.Add(node.InnerText);
                if (node.Attributes["LinkBlacklisted"].Value.ToLowerInvariant() == "true")
                    subLinkBlack.Add(node.InnerText);

                try
                {
                    Descriptions.Add(node.InnerText, node.Attributes["Description"].Value);
                }
                catch { }
            }

            SubredditBlacklist = subBlack.ToArray();
            LinkSubBlacklist = subLinkBlack.ToArray();
        }
    }
}
