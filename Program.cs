using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RedditSharp;
using System.IO;

namespace SubredditLinkerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> IDs = new List<string>(File.ReadAllLines("IDs.txt"));
            
            Reddit R = new Reddit();
            Console.WriteLine("Logging...");
            var User = R.LogIn("SubredditLinkerBot", "Botbot10");
            
            //var all = R.RSlashAll;
            var all = R.GetSubreddit("/r/bottest"); Console.WriteLine("Got the subreddits!\n Now parsing ...");
            while (true)
            {
                foreach (var post in all.New.Take(50))
                {
                    if (IDs.Any(x => x == post.Id)) continue;

                    IDs.Add(post.Id);
                    File.AppendAllLines("IDs.txt", new string[] { post.Id });
                    
                    var reg = new Regex(@"\/r\/\w+", RegexOptions.IgnoreCase);
                    bool xposted = new Regex("(xpost|cross.?posted)", RegexOptions.IgnoreCase).IsMatch(post.Title);
                    StringBuilder sb = new StringBuilder();

                    Console.WriteLine("{0} posts {1}", post.Author, post.Title);
                    if (!reg.IsMatch(post.Title)) continue;

                    switch (xposted)
                    {
                        case true:
                            sb.AppendLine("This is where the post was Xpost'd:\n"); break;
                        case false:
                            sb.AppendLine("This is a link to the post's subreddit for the lazy:\n"); break;
                    }
                    Console.WriteLine("\t Xposted: {0}", xposted);

                    foreach (Match m in reg.Matches(post.Title))
                    {
                        sb.AppendLine(m.Value + ": Test description");
                        Console.WriteLine("\t Parsed: {0}", m.Value);
                    }

                    sb.AppendLine(" * * * \n" +
                                  "*I'm a bot.* - [FAQ](https://github.com/SolarLiner/Subreddit-Linker-Bot#subreddit-linker-bot) | " +
                                  "[Source](https://github.com/SolarLiner/Subreddit-Linker-Bot) | [Paypal donation link]");

                    post.Comment(sb.ToString());
                   
                    sb.AppendLine("-------------------------------------\n\n");
                    File.AppendAllText("lolol.txt", sb.ToString());

                    System.Threading.Thread.Sleep(2000);
                }
            }
        }
    }
}
