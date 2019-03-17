using beanBot.Core.Account;
using beanBot.Core.UserAccounts;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Globalization;
using beanBot.Core;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace beanBot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '0' && str[i] <= '9')
                    || (str[i] >= 'A' && str[i] <= 'z'
                        || (str[i] == '.' || str[i] == '_' || str[i] == '-' || str[i] == '@' || str[i] == ' ')))
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }

        public static void EmbedRndColour(EmbedBuilder input)
        {
            Random rnd = new Random();
            int int1 = rnd.Next(256);
            int int2 = rnd.Next(256);
            int int3 = rnd.Next(256);
            input.WithColor(int1, int2, int3);
        }

        public static IEnumerable<string> SplitAlpha(string input)
        {
            var words = new List<string> { string.Empty };
            for (var i = 0; i < input.Length; i++)
            {
                words[words.Count - 1] += input[i];
                if (i + 1 < input.Length && char.IsLetter(input[i]) != char.IsLetter(input[i + 1]))
                {
                    words.Add(string.Empty);
                }
            }
            return words;
        }

        public static void Populate<T>(T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = value;
            }
        }

        //

        [Command("help")]
        public async Task Help()
        {
            string p = Config.bot.cmdPrefix;
            IDMChannel channel = Context.User.GetOrCreateDMChannelAsync().Result;
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Commands");
            embed.WithDescription("Guide: [] for required parameters, () for non-required parameters.");
            embed.AddField("Profile", 
                $"**{p}profile (user)** - Displays the profile of a user, defaults to yourself\n" +
                $"**{p}daily** - Recieve your daily credits. A bigger streak means more credits.\n" +
                $"**{p}coinflip [h/t] [bet]** - Bets either 'h' or 't', for the bet specified.\n");
            embed.AddField("Misc",
                $"**{p}count** - Adds 1 to the global count, see how high it can get!\n" +
                $"**{p}dice (low) [high]** - Rolls a dice between the low and the high, low defaults to 1.\n" +
                $"**{p}person** - Generates a random person using https://randomuser.me/. \n" +
                $"**{p}remind [time] [reminder]** - Reminds you in DM. Time must be given in the format '0s', '0m' or '0h'\n" +
                $"**{p}rate [to rate]** - Rates the thing from 0 to 10.\n" +
                $"**{p}echo [message]** - Echos something back in a fancy embed.\n" +
                $"**{p}pick [option1|option2|etc.]** - Picks one option out of all the ones given.\n");
            embed.WithFooter("Commands for this bot.");
            EmbedRndColour(embed);
            await channel.SendMessageAsync("", false, embed);
        }

        [Command("daily")]
        public async Task Daily()
        {
            ulong Time = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            UserAccount account = UserAccounts.GetAccount(Context.User);
            ulong timepassed = Time - account.LastDailyUnix;
            if (timepassed < 86400) // If less than a day passed
            {
                ulong timeleft = 86400 - timepassed;
                TimeSpan t = TimeSpan.FromSeconds(timeleft);
                await Context.Channel.SendMessageAsync($"Too soon. You need to wait {t.Hours} hours, {t.Minutes} minutes and {t.Seconds} seconds.");
                return;
            }
            account.LastDailyUnix = Time;
            account.DailyStreak = timepassed < 172800 ? account.DailyStreak + 1 : 0; // Ternary operator to decide daily streak
            uint money = 100 + (50 * account.DailyStreak);

            EmbedBuilder embed = new EmbedBuilder();
            await Context.Channel.SendMessageAsync($":money_with_wings: You have recieved your daily credits of ${money}!");
        }

        [Command("coinflip")]
        public async Task Coinflip(char bet, uint amount)
        {
            // h = 0, t = 1
            string[] bets = { "heads", "tails" };
            UserAccount account = UserAccounts.GetAccount(Context.User);
            int betnum;
            switch (bet)
            {
                case 'h':
                    betnum = 0;
                    break;
                case 't':
                    betnum = 1;
                    break;
                default:
                    await Context.Channel.SendMessageAsync("Please make sure your bet is either 'h' or 't'.");
                    return;
            }
            if(account.Cash < amount)
            {
                await Context.Channel.SendMessageAsync($"You don't have enough for that! You only have ${account.Cash}!");
                return;
            }
            Random rnd = new Random();
            int correct = rnd.Next(2);

            if(betnum == correct)
            {
                await Context.Channel.SendMessageAsync($"You guessed {bets[correct]}, which is correct! {amount} has been added to your balance.");
                account.Cash += amount;
            }
            else
            {
                await Context.Channel.SendMessageAsync($"You guessed {bets[betnum]}, but the coin had {bets[correct]}. {amount} has been taken from your balance.");
            }
        }

        [Command("person")]
        public async Task Person()
        {
            TextInfo textInfo = new CultureInfo("en-US").TextInfo;
            string json = "";
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString("https://randomuser.me/api/?nat=gb,us");
            }

            var dataObject = JsonConvert.DeserializeObject<dynamic>(json);
            var result = dataObject.results[0];
            var embed = new EmbedBuilder();

            string genderEmote = result.gender == "male" ? ":man:" : ":woman:";
            string firstName = textInfo.ToTitleCase(RemoveSpecialCharacters(result.name.first.ToString()));
            string lastName = textInfo.ToTitleCase(RemoveSpecialCharacters(result.name.last.ToString()));
            string avatarURL = result.picture.large.ToString();
            string street = textInfo.ToTitleCase(RemoveSpecialCharacters(result.location.street.ToString()));
            string email = RemoveSpecialCharacters(result.email.ToString());
            string age = result.dob.age.ToString();

            EmbedRndColour(embed);
            embed.WithThumbnailUrl(avatarURL);
            embed.WithTitle($"{genderEmote} - Randomly generated person");
            embed.AddInlineField("First Name", firstName);
            embed.AddInlineField("Last Name", lastName);
            embed.AddInlineField("Street", street);
            embed.AddInlineField("Email", email);
            embed.AddInlineField("Age", age);

            embed.WithFooter("Person generated by https://randomuser.me/");

            await Context.Channel.SendMessageAsync("", false, embed);
        }


        [Command("remind")]
        public async Task Remind(string time, [Remainder]string reminder)
        {
            Dictionary<string, int> timeSuffix = new Dictionary<string, int>()
            {
                { "s", 1 },
                { "m", 60 },
                { "h", 3600 }
            };


            string[] str = SplitAlpha(time).ToArray();
            string suffix = str[str.Length - 1];
            if (!timeSuffix.ContainsKey(suffix))
            {
                await Context.Channel.SendMessageAsync("Invalid suffix. Please use either `s`, `m`, or `h`");
            }
            else if (str.Length != 2)
            {
                await Context.Channel.SendMessageAsync($"Please use the format {Config.bot.cmdPrefix}remind [num][suffix] [reminder], e.g. {Config.bot.cmdPrefix}remind 10m Hello World!");
            }
            else
            {
                async void Remind()
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Reminder");
                    EmbedRndColour(embed);
                    embed.WithDescription(reminder);
                    embed.WithThumbnailUrl(Context.User.GetAvatarUrl());
                    var DMchannel = await Context.User.GetOrCreateDMChannelAsync();
                    await DMchannel.SendMessageAsync("", false, embed);
                }

                int time_int = int.Parse(str[0]);
                await RepeatingTimer.StartTimer((time_int * timeSuffix[suffix]), false, Remind);
            }
        }

        [Command("rate")]
        public async Task Rate([Remainder]string toRate)
        {
            Random rnd = new Random();
            if (!int.TryParse(
                DataStorage.GetValue($"r_{toRate}", rnd.Next(11).ToString()),
                out int rating))
            {
                await Context.Channel.SendMessageAsync("Internal error. Please contact the developer.");
                return;
            }
            await Context.Channel.SendMessageAsync($"{Context.User.Username}, I'd give {toRate} a {rating}/10");
        }

        [Command("profile")]
        public async Task Profile()
        {
            UserAccount account = UserAccounts.GetAccount(Context.User);
            var embed = new EmbedBuilder();

            string[] progressBar = new string[10];
            Populate(progressBar, ":black_large_square:");

            uint level = account.LevelNum;
            uint XP = account.XP;
            uint xpReached = level * level * 25;
            uint xpInNextLevel = (level + 1) * (level + 1) * 25; // xp = (level^2)*25 // level = sqrt(xp/25)
            uint xpLeft = xpInNextLevel - XP;
            uint xpSinceLastLevel = XP - xpReached;
            double fracOfLevelDone = XP / xpInNextLevel;
            double roundedFrac = Math.Round(fracOfLevelDone, 1);
            int size = (int)(roundedFrac * 10);
            int index = 0;
            foreach (string h in progressBar)
            {
                if (size > 0)
                {
                    progressBar[index] = ":ballot_box_with_check:";
                    index++;
                    size--;
                }
                else break;
            }

            string progBar = string.Join("", progressBar);

            EmbedRndColour(embed);
            embed.WithTitle($"Profile of {Context.User.Username}");
            embed.WithThumbnailUrl(Context.User.GetAvatarUrl());
            embed.AddInlineField("Cash", account.Cash.ToString());
            embed.AddField($"Level {level.ToString()} - {xpLeft.ToString()} XP to level {(level + 1).ToString()}", progBar);

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("profile")]
        public async Task Profile(SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            var embed = new EmbedBuilder();
            EmbedRndColour(embed);
            embed.WithTitle($"Profile of {user.Username}");
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            await Context.Channel.SendMessageAsync($" has {account.XP} xp and {account.Cash}", false, embed);
        }

        [Command("addXP")]
        [RequireOwner]
        public async Task AddXp(SocketUser user, uint xp)
        {
            var account = UserAccounts.GetAccount(user);
            uint oldxp = account.XP;
            account.XP += xp;
            await Context.Channel.SendMessageAsync($"{user.Username} had their XP changed from {oldxp.ToString()} to {account.XP.ToString()}");
        }

        [Command("echo")]
        public async Task Echo([Remainder]string s)
        {
            string user = Context.User.Username;
            var embed = new EmbedBuilder();
            embed.WithTitle(user + " says");
            embed.WithDescription(s);
            embed.WithThumbnailUrl(Context.User.GetAvatarUrl());
            EmbedRndColour(embed);
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("dice")]
        public async Task Dice(int high)
        {
            Random rnd = new Random();
            int roll = rnd.Next(1, high + 1);
            var embed = new EmbedBuilder();
            embed.WithTitle("Dice roll!");
            embed.WithThumbnailUrl("https://images-na.ssl-images-amazon.com/images/I/61b-7%2BcowML.png"); // Dice roll image
            EmbedRndColour(embed);
            embed.WithDescription("A dice from 1 to " + high.ToString() + " was rolled, the result is " + roll.ToString());
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("dice")]
        public async Task Dice(int low, int high)
        {
            Random rnd = new Random();
            int roll = rnd.Next(low, high + 1);
            var embed = new EmbedBuilder();
            embed.WithTitle("Dice roll!");
            embed.WithThumbnailUrl("https://images-na.ssl-images-amazon.com/images/I/61b-7%2BcowML.png"); // Dice roll image
            EmbedRndColour(embed);
            embed.WithDescription("A dice from " + low.ToString() + " to " + high.ToString() + " was rolled, the result is " + roll.ToString());
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("pick")]
        public async Task Pick([Remainder]string msg)
        {
            string[] options = msg.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string sel = options[r.Next(0, options.Length)];
            await Context.Channel.SendMessageAsync("I would pick " + sel + ".");

        }

        [Command("count")]
        public async Task Count()
        {
            string count = DataStorage.GetValue("count", "1");
            int countnum = int.Parse(count);
            await Context.Channel.SendMessageAsync($"You have counted to {countnum.ToString()}!");
            DataStorage.AddPairToStorage("count", (countnum + 1).ToString());
        }
    }
}
