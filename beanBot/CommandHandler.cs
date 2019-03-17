using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Reflection;
using System;
using beanBot.Core.LevellingSystem;

namespace beanBot
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {

            if (!(s is SocketUserMessage msg)) return;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;

            // Level
            Levelling.UserSentMessageAsync(context);


            int argpos = 0;
            if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argpos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argpos))
            {
                var result = await _service.ExecuteAsync(context, argpos);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Error: " + result.ErrorReason);
                    Console.ResetColor();
                    await context.Channel.SendMessageAsync($"{context.User.Mention} / ERROR: {result.ErrorReason}. Please contact the owner");
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync("Unknown command.");
                }
            }
        }
    }
}
