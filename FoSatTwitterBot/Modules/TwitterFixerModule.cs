using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace FoSatTwitterBot.Modules
{
    [Discord.Interactions.RequireBotPermission(ChannelPermission.ManageMessages)]
    internal class TwitterFixerModule : InteractionModuleBase<SocketInteractionContext>
    { 
        internal async Task FixTwitterMessage(IMessage message, int runCount = 0)
        {
            if (!message.Embeds.Any()) 
            { 
                if (runCount < 5) 
                {
                    await Task.Delay(1000);
                    await FixTwitterMessage(await message.Channel.GetMessageAsync(message.Id), ++runCount); 
                }
                return;
            }
            try
            {
                var twitterMatch = message.Embeds.Where(url => url.Url.Contains("x.com", StringComparison.CurrentCultureIgnoreCase) || url.Url.Contains("twitter.com", StringComparison.CurrentCultureIgnoreCase) 
                    && !url.Url.Contains("fixupx.com", StringComparison.CurrentCultureIgnoreCase) && !url.Url.Contains("fxtwitter.com", StringComparison.CurrentCultureIgnoreCase));
                var twitterURLs = new List<string>();
                if (twitterMatch.Any())
                {
                    foreach (var match in twitterMatch)
                    {
                        var twitterURL = new string(match.Url);
                        if (twitterURL.Contains("x.com"))
                        {
                            twitterURL = twitterURL.Replace("x.com", "fixupx.com");
                        }
                        else
                        {
                            twitterURL = twitterURL.Replace("twitter.com", "fxtwitter.com");
                        }
                        twitterURLs.Add(twitterURL);
                    }
                }
                var redditMatch = message.Embeds.Where(url => url.Url.Contains("reddit.com", StringComparison.CurrentCultureIgnoreCase));
                var redditURLs = new List<string>();
                if (redditMatch.Any())
                {
                    foreach (var match in redditMatch)
                    {
                        redditURLs.Add(match.Url.Replace("reddit.com", "rxddit.com"));
                    }
                }

                if (redditMatch.Any() || twitterMatch.Any())
                {
                    var spaceSplit = message.Content.Split(' ');
                    var newSplitList = new List<string>();
                    foreach (var space in spaceSplit)
                    {
                        newSplitList.AddRange(space.Split('\n'));
                    }
                    var urlToIgnore = newSplitList.Where(url => url.Contains("twitter.com", StringComparison.CurrentCultureIgnoreCase) || url.Contains("x.com", StringComparison.CurrentCultureIgnoreCase));
                    var newMessageContent = message.Content;
                    foreach (var url in urlToIgnore)
                    {
                        newMessageContent = newMessageContent.Replace(url, twitterURLs.First(twitterURL => twitterURL.Split(".com")[1] == url.Split(".com")[1]));
                    }
                    var redditToIngore = newSplitList.Where(url => url.Contains("reddit.com", StringComparison.CurrentCultureIgnoreCase));
                    foreach (var url in redditToIngore)
                    {
                        newMessageContent = newMessageContent.Replace(url, redditURLs.First(redditURL => redditURL.Split(".com")[1] == url.Split(".com")[1]));
                    }

                    // if original message was replying to a message, have the bot reply to the same message
                    if (message.Reference != null)
                    {
                        message.Channel.SendMessageAsync($"{(message.Author as SocketGuildUser).Nickname} Sent: " + newMessageContent, messageReference: message.Reference);
                    }
                    else
                    {
                        message.Channel.SendMessageAsync($"{(message.Author as SocketGuildUser).Nickname} Sent: " + newMessageContent);
                    }

                    message.DeleteAsync();
                }
            }
            catch (System.NullReferenceException)   // invalid URL schema (aka not a URL)
            {
                Console.WriteLine("Not a valid URL schema to convert.");
            }
        }
    }
}
