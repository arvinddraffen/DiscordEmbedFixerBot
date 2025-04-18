﻿using Discord.Interactions;
using Discord.WebSocket;
using DiscordEmbedFixerBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var socketConfig = new DiscordSocketConfig
{
    GatewayIntents = Discord.GatewayIntents.AllUnprivileged | Discord.GatewayIntents.MessageContent
};

var client = new DiscordSocketClient(socketConfig);

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config.json", false);       // Add the config file to IConfiguration variables
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton(client);       // Add the discord client to services
        services.AddSingleton<InteractionService>();        // Add the interaction service to services
        services.AddHostedService<InteractionHandlingService>();    // Add the slash command handler
        services.AddHostedService<StartupService>();         // Add the discord startup service
    })
    .Build();

await host.RunAsync();

