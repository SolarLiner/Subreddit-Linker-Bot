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
            Misc.Log("Logging...");
            var User = R.LogIn("SubredditLinkBot", "BotBot30");
            Misc.Log("Logged!");
            
            var all = R.RSlashAll;
            //var all = R.GetSubreddit("/r/bottest");
            Misc.Log("Got the subreddits!\n\tNow parsing ...");

            while (1<2) // Deadmau5 to run on the wheel
            {
                try
                {
                    string curID = "";
                    
                    foreach (var post in all.New.Take(50))
                    {
                        try
                        {
                            curID = post.Id;
                            
                            if (IDs.Any(x => x == curID)) continue; // If post already parsed, skip it
                            IDs.Add(curID); // Add to the list of parsed posts
                            
                            File.AppendAllLines("IDs.cfg", new string[] { post.Id }); // Same, but file-based, for not loosing info :P

                            // Do not process blacklisted subreddits' posts !
                            if(Settings.SubredditBlacklist.Any(x => x.ToLowerInvariant() == post.Subreddit.ToLowerInvariant())) continue;

                            // Parse for subreddits and "Xposts" in titles
                            var reg = new Regex(@"[^a-zA-z0-9]r\/\w+", RegexOptions.IgnoreCase);
                            var reg2 = new Regex(@"\/r\/w+", RegexOptions.IgnoreCase);
                            bool xposted = new Regex("(x.?post|cross.?posted)", RegexOptions.IgnoreCase).IsMatch(post.Title);
                            StringBuilder sb = new StringBuilder();

                            Misc.Log("({1}) - {0}", post.Title, post.Id);
                            if (!reg.IsMatch(post.Title)) continue; // Stop here if nothing is found

                            switch (xposted)
                            {
                                case true:
                                    sb.AppendLine("This is where the post was Xpost'd:\n"); break;
                                case false:
                                    sb.AppendLine("This is a link to the post's subreddit for the lazy:\n"); break;
                            }
                            Console.WriteLine("\tXposted: {0}", xposted);

                            List<string> subs = new List<string>(); // array of parsed subs

                            foreach (Match m in reg.Matches(post.Title))
                            {
                                bool hasDesc = Settings.Descriptions.Any(x => x.Key.ToLowerInvariant() == m.Value.ToLowerInvariant().Trim()); // Check if description is defined
                                string sub = m.Value.ToLower().Trim();
                                sub = sub.StartsWith("/") ? sub : "/" +sub;


                                if (Settings.LinkSubBlacklist.Any(x => x == sub)) continue; // Only post for allowed subreddits!
                                if (sub == "/r/"+post.Subreddit.ToLowerInvariant()) continue; // Do not parse post's subreddit !
                                if (subs.Contains(sub)) continue; // Do not parse same subreddits multiple times !

                                switch (hasDesc)
                                {
                                    case true:
                                        string desc = Settings.Descriptions[m.Value];
                                        sb.AppendLine(string.Format("{0}: {1}", sub, desc));
                                        break;
                                    case false:
                                        sb.AppendLine(sub);
                                        break;
                                }
                                Console.WriteLine("\t Parsed: {0}", sub);

                                subs.Add(sub);
                            }

                            if (subs.Count < 1) continue; // Don't wanna post empty comments lol

                            // Append footer
                            sb.AppendLine(" * * * \n" +
                                          "*I'm a bot.* - [FAQ](https://github.com/SolarLiner/Subreddit-Linker-Bot#subreddit-linker-bot) | " +
                                          "[Source](https://github.com/SolarLiner/Subreddit-Linker-Bot) | " + 
                                          "[PayPal Donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=V2LJY4HCKW3FE)");

                            try { post.Comment(sb.ToString()); }
                            catch (RateLimitException re)
                            {
                                Misc.Log(" ---- WHOOPS! BEEN BLOCKED BY REDDIT ---- ");
                                Misc.Log("      Let's wait {0} minute{1} ...", Math.Ceiling(re.TimeToReset.TotalMinutes), re.TimeToReset.TotalMinutes > 1 ? "s" : "");
                                System.Threading.Thread.Sleep(re.TimeToReset);
                                Misc.Log("The nap felt good, restarting now ...");
                                System.Threading.Thread.Sleep(1500);
                            }

                            sb.AppendLine("-------------------------------------\n\n");
                            File.AppendAllText("posting.log", sb.ToString()); // Add to log

                            System.Threading.Thread.Sleep(350); // Let him not work too hard lol
                        }
                        catch (Exception ex)
                        {
                            IDs.Remove(curID);
                            continue;
                        }
                    }
                }
                catch { }
            }
        }
    }
}
