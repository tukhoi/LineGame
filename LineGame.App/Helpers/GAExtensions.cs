using Davang.Utilities.Log;
using LineGame.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.App.Helpers
{
    public static class GAExtensions
    {
        public static void LogNewGame()
        {
            var tracker = GA.GetTracker();
            if (tracker != null)
                tracker.SendEvent("Games", "NewGame", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), 0);
        }

        public static void LogEndGame(int score, TimeSpan duration)
        {
            var tracker = GA.GetTracker();
            if (tracker != null)
                tracker.SendEvent("Games", "EndGame", duration.ToString(), score);
        }

        public static void LogAdsLoad(AdsProvider provider, long totalLoaded)
        {
            var tracker = GA.GetTracker();
            if (tracker != null)
                tracker.SendEvent("AdsLoad", provider.ToString(), DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), totalLoaded);
        }
    }
}
