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
using System.Threading.Tasks;

namespace FoSatTwitterBot.Modules
{
    [Discord.Interactions.RequireBotPermission(ChannelPermission.ManageMessages)]
    internal class TwitterFixerModule : InteractionModuleBase<SocketInteractionContext>
    { 
        private List<string> twitterUrls = new List<string> { "twitter", "x" };
        internal async Task FixTwitterMessage(SocketMessage message)
        {
            var ruleProvider = new LocalFileRuleProvider("C:\\Users\\Arvind\\source\\repos\\FoSatTwitterBot\\FoSatTwitterBot\\Data\\public_suffix_list.dat");
            await ruleProvider.BuildAsync();
            var domainParser = new DomainParser(ruleProvider);

            try
            {
                var domainInfo = domainParser.Parse(message.Content);
                if (twitterUrls.Contains(domainInfo.Domain))
                {
                    if (Uri.TryCreate(message.Content, UriKind.RelativeOrAbsolute, out var twitterUri))
                    {
                        //await message.Channel.SendMessageAsync($"{domainInfo.Subdomain}.fixupx.{domainInfo.TopLevelDomain}");
                        UriBuilder uriBuilder = new UriBuilder(twitterUri);
                        uriBuilder.Host = uriBuilder.Host.Replace(domainInfo.Domain, "fixupx");
                        await message.Channel.SendMessageAsync(uriBuilder.ToString());
                        await message.DeleteAsync();
                    }
                }
            }
            catch (System.NullReferenceException)   // invalid URL schema (aka not a URL)
            {
                Console.WriteLine("Not a valid URL schema to convert.");
            }
        }
    }
}
