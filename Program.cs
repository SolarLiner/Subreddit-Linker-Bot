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
            List<string> IDs = new List<string>(File.ReadAllLines("IDs.txt")); // Getting already parsed posts

            Settings.Load();
            
            // Connecting the bot
            Reddit R = new Reddit(); 
            Console.WriteLine("Logging...");
            var User = R.LogIn("SubredditLinkBot", "BotBot30");
            Console.WriteLine("Logged!");
            
            var all = R.RSlashAll;
            //var all = R.GetSubreddit("/r/bottest");
            Console.WriteLine("Got the subreddits!\nNow parsing ...");

            while (true) // THE BOT SHALL NEVER STOP MUAHAHAHAHAHAHAHA
            {
                foreach (var post in all.New.Take(50))
                {
                    try
                    {
                        if (IDs.Any(x => x == post.Id)) continue; // If post already parsed, skip it
                        IDs.Add(post.Id); // Add to the list of parsed posts
                        File.AppendAllLines("IDs.cfg", new string[] { post.Id }); // Same, but file-based, for not loosing info :P

                        // Parse for subreddits and "Xposts" in titles
                        var reg = new Regex(@"\/r\/\w+", RegexOptions.IgnoreCase);
                        bool xposted = new Regex("(x.?post|cross.?posted)", RegexOptions.IgnoreCase).IsMatch(post.Title);
                        StringBuilder sb = new StringBuilder();

                        Console.WriteLine(post.Title);
                        if (!reg.IsMatch(post.Title)) continue; // Stop here if nothing is found

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
                            bool hasDesc = Settings.Descriptions.Any(x => x.Key.ToLowerInvariant() == m.Value.ToLowerInvariant()); // Chack if description is defined

                            switch (hasDesc)
                            {
                                case true:
                                    string desc = Settings.Descriptions[m.Value];
                                    sb.AppendLine(string.Format("{0}: {1}", m.Value.ToLower(), desc));
                                    break;
                                case false:
                                    sb.AppendLine(m.Value.ToLower());
                                    break;
                            }
                            Console.WriteLine("\t Parsed: {0}", m.Value);
                        }

                        // Append footer
                        sb.AppendLine(" * * * \n" +
                                      "*I'm a bot.* - [FAQ](https://github.com/SolarLiner/Subreddit-Linker-Bot#subreddit-linker-bot) | " +
                                      "[Source](https://github.com/SolarLiner/Subreddit-Linker-Bot) | [Paypal donation link]");

                        try { /*post.Comment(sb.ToString());*/ }
                        catch (RateLimitException re)
                        {
                            Console.WriteLine(" ---- WHOOPS! BEEN BLOCKED BY REDDIT ---- ");
                            Console.WriteLine("      Let's wait a couple minutes ...");
                            System.Threading.Thread.Sleep(re.TimeToReset);
                            Console.WriteLine("The nap felt good, restarting now ...");
                            System.Threading.Thread.Sleep(1500);
                        }

                        sb.AppendLine("-------------------------------------\n\n");
                        File.AppendAllText("posting.log", sb.ToString()); // Add to log

                        System.Threading.Thread.Sleep(350); // Let him not work too hard lol
                    }
                    catch { continue; }
                }
            }
        }
    }
}
