using LineGame.App.Resources;
using LineGame.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LineGame.App.ViewModel
{
    public class GameModel
    {
        public Guid Id { get; set; }
        public string Score { get; set; }
        public string Ball { get; set; }
        public string Move { get; set; }
        public string Duration { get; set; }
        public string EntryDate { get; set; }
        public byte HighestScore { get; set; }
        public Brush ForegroundColor { get; set; }
        public Visibility UseNumber { get; set; }
        public byte Rank { get; set; }

        public GameModel(Game game)
        {
            this.Id = game.Id;
            this.Score = AppResources.HistoryScoreTitle + ": " + game.Score;
            this.Ball = AppResources.HistoryBallTitle + ": " + game.Ball;
            this.Move = AppResources.HistoryMoveTitle + ": " + game.Move;
            this.Duration = AppResources.HistoryDurationTitle + ": " + game.Duration.ToString();
            this.EntryDate = game.Mode != PlayingMode.Lost ?
                AppResources.HistoryEntryDateTitle + ": " + game.EntryDate.ToString("dd/MM/yyyy hh:mm:ss tt")
                : AppResources.HistoryEntryDateLostTitle + ": " + game.EntryDate.ToString("dd/MM/yyyy hh:mm:ss tt");

            this.HighestScore = game.HighestScore;

            this.UseNumber = game.UseNumber ? Visibility.Visible : Visibility.Collapsed;

            this.ForegroundColor = game.Mode == PlayingMode.Paused ? new SolidColorBrush(Davang.Utilities.Helpers.Colors.Gold) : new SolidColorBrush(Colors.White);
            
        }
    }
}
