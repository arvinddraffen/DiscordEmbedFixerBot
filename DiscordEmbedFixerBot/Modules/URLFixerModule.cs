using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordEmbedFixerBot.Modules
{
    [Discord.Interactions.RequireBotPermission(ChannelPermission.ManageMessages)]
    internal class URLFixerModule : InteractionModuleBase<SocketInteractionContext>
    {
        List<(IMessage orignalMessage, IMessage fixedMessage)> messages = new List<(IMessage, IMessage)>();
        internal async Task FixMessage(IMessage message, int runCount = 0)
        {
            if (message.Content.Contains("$IGNORE$"))
            {
                var prevMesssage = messages.FirstOrDefault(msg => msg.orignalMessage.Author.Id == message.Author.Id && msg.orignalMessage.Channel.Id == message.Channel.Id);
                if (prevMesssage != default)
                {
                    messages.Remove(prevMesssage);
                    await message.Channel.SendMessageAsync($"{(message.Author as SocketGuildUser).DisplayName} Sent: " + prevMesssage.orignalMessage.Content, messageReference: prevMesssage.orignalMessage.Reference);
                    await message.DeleteAsync();
                    await prevMesssage.fixedMessage.DeleteAsync();
                }
                return;
            }
            if (!message.Embeds.Any())
            {
                if (runCount < 5)
                {
                    await Task.Delay(1000);
                    await FixMessage(await message.Channel.GetMessageAsync(message.Id), ++runCount);
                }
                return;
            }
            try
            {
                var twitterMatch = message.Embeds.Where(url => (url.Url.Contains("x.com", StringComparison.CurrentCultureIgnoreCase) || url.Url.Contains("twitter.com", StringComparison.CurrentCultureIgnoreCase))
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

                var eBayMatch = message.Embeds.Where(url => url.Url.Contains("ebay.com", StringComparison.CurrentCultureIgnoreCase));
                var eBayURLs = new List<string>();
                if (eBayMatch.Any())
                {
                    foreach (var match in eBayMatch)
                    {
                        eBayURLs.Add(match.Url.Substring(0, match.Url.IndexOf('?')));
                    }
                }

                var instagramMatch = message.Embeds.Where(url => url.Url.Contains("instagram.com", StringComparison.CurrentCultureIgnoreCase));
                var instagramURLs = new List<string>();
                if (instagramMatch.Any())
                {
                    foreach (var match in instagramMatch)
                    {
                        instagramURLs.Add(match.Url.Replace("instagram.com", "instagramez.com"));
                    }
                }

                if (redditMatch.Any() || twitterMatch.Any() || eBayMatch.Any() || instagramMatch.Any())
                {
                    var spaceSplit = message.Content.Split(' ');
                    var newSplitList = new List<string>();
                    foreach (var space in spaceSplit)
                    {
                        var possibleURL = space;
                        if (space.EndsWith('\\'))
                        {
                            possibleURL = possibleURL.Remove(space.Length);
                        }
                        newSplitList.AddRange(possibleURL.Split('\n'));
                    }
                    var urlToIgnore = newSplitList.Where(url => url.Contains("twitter.com", StringComparison.CurrentCultureIgnoreCase) || url.Contains("x.com", StringComparison.CurrentCultureIgnoreCase)).Distinct();
                    var newMessageContent = message.Content;
                    foreach (var url in urlToIgnore)
                    {
                        var replaceContent = twitterURLs.FirstOrDefault(twitterURL => twitterURL.Split(".com")[1] == url.Split(".com")[1]) + $"\n[Original Link](<{url}>) \n";
                        if (replaceContent != default)
                        {
                            newMessageContent = newMessageContent.Replace(url, replaceContent);
                        }
                    }
                    var redditToIngore = newSplitList.Where(url => url.Contains("reddit.com", StringComparison.CurrentCultureIgnoreCase)).Distinct();
                    foreach (var url in redditToIngore)
                    {
                        var replaceContent = redditURLs.FirstOrDefault(redditURL => redditURL.Split(".com")[1] == url.Split(".com")[1]) + $"\n[Original Link](<{url}>) \n";
                        if (replaceContent != default)
                        {
                            newMessageContent = newMessageContent.Replace(url, replaceContent);
                        }
                    }
                    var eBayToIgnore = newSplitList.Where(url => url.Contains("ebay.com", StringComparison.CurrentCultureIgnoreCase));
                    foreach (var url in eBayToIgnore)
                    {
                        var replaceContent = eBayURLs.FirstOrDefault(eBayURL => url.Contains(eBayURL));
                        if (replaceContent != default)
                        {
                            newMessageContent = newMessageContent.Replace(url, replaceContent);
                        }
                    }
                    var instagramToIgnore = newSplitList.Where(url => url.Contains("instagram.com", StringComparison.CurrentCultureIgnoreCase)).Distinct();
                    foreach (var url in instagramToIgnore)
                    {
                        var replaceContent = instagramURLs.FirstOrDefault(instagramURL => instagramURL.Split(".com")[1] == url.Split(".com")[1]) + $"\n[Original Link](<{url}>) \n";
                        if (replaceContent != default)
                        {
                            newMessageContent = newMessageContent.Replace(url, replaceContent);
                        }
                    }

                    var messageToRemove = messages.FirstOrDefault(msg => msg.orignalMessage.Author.Id == message.Author.Id && msg.orignalMessage.Channel.Id == message.Channel.Id);
                    if (messageToRemove != default)
                    {
                        messages.Remove(messageToRemove);
                    }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var fixedMessage = await message.Channel.SendMessageAsync($"{(message.Author as SocketGuildUser).DisplayName} Sent: " + newMessageContent, messageReference: message.Reference);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    messages.Add((message, fixedMessage));
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
