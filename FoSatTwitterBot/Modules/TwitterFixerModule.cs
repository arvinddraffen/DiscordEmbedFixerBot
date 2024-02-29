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
        internal async Task FixTwitterMessage(SocketMessage message)
        {
            if (!message.Embeds.Any()) { return; }
            try
            {
                var twitterMatch = message.Embeds.FirstOrDefault(url => url.Url.Contains("x.com", StringComparison.CurrentCultureIgnoreCase) || url.Url.Contains("twitter.com", StringComparison.CurrentCultureIgnoreCase) 
                    && !url.Url.Contains("fixupx.com", StringComparison.CurrentCultureIgnoreCase) && !url.Url.Contains("fxtwitter.com", StringComparison.CurrentCultureIgnoreCase));
                if (twitterMatch != default)
                {
                    var twitterURL = new string(twitterMatch.Url);
                    if (twitterURL.Contains("x.com"))
                    {
                        twitterURL = twitterURL.Replace("x.com", "fixupx.com");
                    }
                    else
                    {
                        twitterURL = twitterURL.Replace("twitter.com", "fxtwitter.com");
                    }

                    if (message.Embeds.Count == 1 && !message.Content.Contains(" ", StringComparison.CurrentCulture))
                    {
                        await message.Channel.SendMessageAsync(twitterURL);
                    }
                    else
                    {
                        var spaceSplit = message.Content.Split(' ');
                        var newSplitList = new List<string>();
                        foreach (var space in spaceSplit)
                        {
                            newSplitList.AddRange(space.Split('\n'));
                        }
                        var urlToIgnore = newSplitList.First(url => url.Contains("twitter.com", StringComparison.CurrentCultureIgnoreCase) || url.Contains("x.com", StringComparison.CurrentCultureIgnoreCase));
                        await message.Channel.SendMessageAsync($"{(message.Author as SocketGuildUser).Nickname} Sent: " + message.Content.Replace(urlToIgnore, twitterURL));
                    }

                    await message.DeleteAsync();
                }
            }
            catch (System.NullReferenceException)   // invalid URL schema (aka not a URL)
            {
                Console.WriteLine("Not a valid URL schema to convert.");
            }
        }
    }
}
