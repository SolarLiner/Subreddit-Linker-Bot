using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SubredditLinkerBot
{
    static class Misc
    {
        public static void Log(string text)
        {
            string result = "[{0:yyy-MM-dd hh:mm:ss}]{1}".Format(DateTime.UtcNow, text);
            
            Console.WriteLine(result);
            File.AppendAllText("stdout.log", result);
        }

        public static void Log(string text, params object[] args)
        {
            Log(text.Format(args));
        }

        static string Format(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
    }
}
