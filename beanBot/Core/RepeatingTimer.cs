using System;
using System.Threading.Tasks;
using System.Timers;

namespace beanBot.Core
{
    internal static class RepeatingTimer
    {
        private static Timer discordTimer;
        internal static Task StartTimer(int seconds, bool autoReset, Action method)
        {

            discordTimer = new Timer()
            {
                Interval = 1000 * seconds,
                AutoReset = autoReset,
                Enabled = true
            };

            discordTimer.Elapsed += Tick;
            void Tick(object sender, ElapsedEventArgs e)
            {
                method();
            }

            return Task.CompletedTask;
        }
    }
}