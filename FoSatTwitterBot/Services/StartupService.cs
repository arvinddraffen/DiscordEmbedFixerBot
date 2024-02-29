using Discord;
using Discord.WebSocket;
using FoSatTwitterBot.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoSatTwitterBot.Services
{
    internal class StartupService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscordSocketClient> _logger;

        public StartupService(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordSocketClient> logger)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;

            _client.Log += msg => LogUtility.Log(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
