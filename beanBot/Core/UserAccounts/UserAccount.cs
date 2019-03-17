using System;

namespace beanBot.Core.Account
{
    public class UserAccount
    {
        public ulong ID { get; set; }

        public uint Cash { get; set; }

        public uint XP { get; set; }

        public ulong LastMessageUnix { get; set; }

        public uint DailyStreak { get; set; }

        public ulong LastDailyUnix { get; set; }

        public uint LevelNum
        {
            get
            {
                return (uint)Math.Sqrt(XP / 25);
            }
        }
    }
}
