using Discord;
using Microsoft.Extensions.Logging;

namespace DiscordEmbedFixerBot.Utilities
{
    /// <summary>
    /// Handles logging for Discord.Net's log events.
    /// </summary>
    internal class LogUtility
    {
        public static Task Log(ILogger logger, LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Verbose:
                    logger.LogInformation(msg.ToString());
                    break;

                case LogSeverity.Info:
                    logger.LogInformation(msg.ToString());
                    break;

                case LogSeverity.Warning:
                    logger.LogWarning(msg.ToString());
                    break;

                case LogSeverity.Error:
                    logger.LogError(msg.ToString());
                    break;

                case LogSeverity.Critical:
                    logger.LogCritical(msg.ToString());
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
