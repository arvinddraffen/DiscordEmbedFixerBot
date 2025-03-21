﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordEmbedFixerBot.Modules;
using DiscordEmbedFixerBot.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DiscordEmbedFixerBot.Services
{
    internal class InteractionHandlingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<InteractionService> _logger;
        private readonly URLFixerModule _twitterFixerModule;

        public InteractionHandlingService(
            DiscordSocketClient discord,
            InteractionService interactions,
            IServiceProvider services,
            IConfiguration config,
            ILogger<InteractionService> logger)
        {
            _discord = discord;
            _interactions = interactions;
            _services = services;
            _config = config;
            _logger = logger;
            _twitterFixerModule = new URLFixerModule();

            _interactions.Log += msg => LogUtility.Log(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
            _discord.InteractionCreated += OnInteractionAsync;
            _discord.MessageReceived += MessageReceivedAsync;

            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _interactions.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_discord, interaction);
                var result = await _interactions.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
            catch
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync()
                        .ContinueWith(msg => msg.Result.DeleteAsync());
                }
            }
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)   // ignore other bot messages (including this bot) for now
            {
                return;
            }
            else
            {
                await _twitterFixerModule.FixMessage(message);
            }
        }
    }
}
