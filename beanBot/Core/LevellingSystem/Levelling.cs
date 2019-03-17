using Discord.Commands;
using System;
using System.Net;
using Newtonsoft.Json;
using Discord;


namespace beanBot.Core.LevellingSystem
{
    internal static class Levelling
    {
        internal static async void UserSentMessageAsync(SocketCommandContext context)
        {
            var userAccount = UserAccounts.UserAccounts.GetAccount(context.User);

            ulong Time = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            ulong lastTime = userAccount.LastMessageUnix;
            if (Time - lastTime < 5) return;

            userAccount.LastMessageUnix = Time;
            uint oldLevel = userAccount.LevelNum;
            userAccount.XP += 5;
            UserAccounts.UserAccounts.SaveAccounts();
            userAccount = UserAccounts.UserAccounts.GetAccount(context.User);
            if (oldLevel != userAccount.LevelNum)
            {

                string json = "";
                Random rnd = new Random();
                using (WebClient client = new WebClient())
                {
                    json = client.DownloadString(Utilities.GetAlert("unsplash"));
                }
                if (json == "") return;
                var dataObject = JsonConvert.DeserializeObject<dynamic>(json);
                var result = dataObject[rnd.Next(0, 30)];

                string url = result.urls.raw.ToString();
                string avatar = context.User.GetAvatarUrl();

                var embed = new EmbedBuilder();
                embed.WithColor(0, 255, 0);
                embed.WithTitle(":tada: LEVEL UP! :tada:");
                embed.WithThumbnailUrl(context.User.GetAvatarUrl());
                embed.WithDescription($"{context.User.Username} has levelled up!");
                embed.AddInlineField("Levelling", $"{oldLevel.ToString()} -> {userAccount.LevelNum.ToString()}");
                embed.AddInlineField("XP", userAccount.XP.ToString());
                embed.WithFooter("Congratulations!");

                await context.Channel.SendMessageAsync("", false, embed);


                //$"**LEVEL UP!** {context.User.Mention} has just levelled up from level {oldLevel.ToString()} to level {userAccount.LevelNum.ToString()}! :tada:");

            }

        }
    }
}
