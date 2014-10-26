using Davang.Utilities.Log;
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
                tracker.SendEvent("Games", "NewGame", DateTime.Now.ToLongDateString(), 0);
        }

        public static void LogEndGame(int score, TimeSpan duration)
        {
            var tracker = GA.GetTracker();
            if (tracker != null)
                tracker.SendEvent("Games", "EndGame", duration.ToString(), score);
        }
    }
}
