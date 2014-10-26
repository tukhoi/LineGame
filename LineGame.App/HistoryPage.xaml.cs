using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LineGame.App.Helpers;
using System.Threading.Tasks;
using LineGame.AppServices;
using LineGame.App.ViewModel;
using Davang.Utilities.Extensions;
using Davang.WP.Utilities;

namespace LineGame.App
{
    public partial class HistoryPage : LinePage
    {
        public HistoryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadHistory();
        }

        private void LoadHistory()
        {
            var games = AppConfig.Games;
            if (games == null || games.Count == 0)
            {
                Messenger.ShowToast("no history game found...");
                return;
            }

            var gameModels = new List<GameModel>();
            games.OrderByDescending(g => g.Score)
                .ThenBy(g => g.Move)
                .ThenBy(g => g.Ball)
                .ThenBy(g => g.Duration)
                .ThenBy(g => g.EntryDate)
                .Where(g => g != null)
                .ForEach(g => gameModels.Add(new GameModel(g)));

            gameModels.ForEach(g => g.Rank = (byte)(gameModels.IndexOf(g) + 1));

            lstHistory.ItemsSource = gameModels;
        }
    }
}