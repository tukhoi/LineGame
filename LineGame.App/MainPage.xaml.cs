using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LineGame.App.Resources;
using LineGame.AppServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using LineGame.App.Helpers;
using Davang.Utilities.Extensions;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using LineGame.AppServices.Sound;
using Davang.WP.Utilities;
using Davang.Utilities.Log;
using Davang.WP.Utilities.Extensions;

namespace LineGame.App
{
    public partial class MainPage : LinePage
    {
        LineMatrix _matrix;
        DispatcherTimer _playTimer;
        TimeSpan _playingTime;
        Cell[] _proposalCells;

        Game _currentGame;
        int _maxScore;

        public MainPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            PopulateAds();            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (_matrix.Mode != PlayingMode.Stopped)
                _matrix.Mode = AppServices.PlayingMode.Paused;
            _playTimer.Stop();
            WrapPlayingGame();
            SaveGame();
            smaato.Dispose();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.SetProgressIndicator(true, AppResources.LoadGameMessage);
            if (App.ShouldRefreshGame)
            {
                await Task.Run(() => LoadGames());
                //await Task.Factory.StartNew(() =>
                //    Deployment.Current.Dispatcher.BeginInvoke(new Action(() => Initialize(_currentGame))));

                Initialize(_currentGame);
            }

            this.SetProgressIndicator(false);
        }

        private void Initialize(Game game = null)
        {
            if (_matrix != null && _matrix.Mode != PlayingMode.Initialized)
            {
                _matrix.Dispose();
                _matrix = null;
            }

            _matrix = new LineMatrix(useNumber: game == null ? AppConfig.UseNumber : game.UseNumber,
                nextCells: game == null ? null : game.NextCells);
            _matrix.ScoreEvent += _matrix_ScoreEvent;
            _matrix.MoveEvent += _matrix_MoveEvent;
            _matrix.NextCellsEvent += _matrix_NextCellsEvent;
            _matrix.NotifyEvent += matrix_Notify;
            _matrix.SetBackground(AppConfig.Background.FileName);

            if (game != null) PopulateMatrixFromGame(_matrix, game);
            cvMatrix.Children.Clear();
            _matrix.Margin = new Thickness(0, 20, 0, 0);
            cvMatrix.Children.Add(_matrix);

            _playingTime = game == null ? new TimeSpan(0, 0, 0) : game.Duration;

            InitialzeProposalCells(_matrix.NextCells);

            _playTimer = new DispatcherTimer();
            _playTimer.Interval = new TimeSpan(0, 0, 1);
            _playTimer.Tick += _playTimer_Tick;
            _playTimer.Stop();
            btnStartImage.Source = AppConfig.PLAY_IMAGE;
            btnStopImage.Source = AppConfig.STOP_IMAGE;

            txtScore.Text = AppResources.ScoreTitle + ": " + _matrix.Score.ToString();
            txtBallCount.Text = AppResources.BallTitle + ": " + _matrix.BallCount.ToString();
            txtMoveCount.Text = AppResources.MoveCountTitle + ": " + _matrix.MoveCount.ToString();
            txtTime.Text = _playingTime.ToString();
        }

        private void PopulateMatrixFromGame(LineMatrix matrix, Game game)
        {
            if (game != null && game.CellData != null)
            {
                matrix.ImportData(game.CellData);
                matrix.Mode = game.Mode;
            }
            
            matrix.Score = game == null ? 0 : game.Score;
            matrix.BallCount = game == null ? 0 : game.Ball;
            matrix.MoveCount = game == null ? 0 : game.Move;
            matrix.HighestScore = game.HighestScore;
            SetTitle(matrix.Mode);
        }

        private void InitialzeProposalCells(CellData[] nextCells)
        {
            _proposalCells = new Cell[3];
            stkProposalCells.Children.Clear();
            for (int i = 0; i < 3; i++)
            {
                var cell = new Cell(new Position(0, 0), transparentBackground:true, border:false);
                cell.Data.State = AppServices.State.Filled;
                cell.Data.BallColor = (nextCells != null && nextCells[i] != null) ? nextCells[i].BallColor : BallColor.None;
                if (_matrix.UseNumber)
                    cell.Data.Number = (nextCells != null && nextCells[i] != null) ? nextCells[i].Number : (byte)0;
                cell.IsHitTestVisible = false;

                _proposalCells[i] = cell;
                stkProposalCells.Children.Add(cell);
            }
        }

        private async void btnStart_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_matrix.Mode == PlayingMode.Stopped || _matrix.Mode == PlayingMode.Lost || _matrix.Mode == PlayingMode.Initialized)
            {
                if (_matrix != null && _matrix.Mode != PlayingMode.Initialized)
                    Initialize();
                _matrix.Start();
                _playingTime = new TimeSpan(0, 0, 0);
                _playTimer.Start();
                _currentGame = new Game();
                _currentGame.Id = Guid.NewGuid();
                btnStartImage.Source = AppConfig.PAUSE_IMAGE;
                //txtTitle.Text = AppResources.PlayingTitle;

                GAExtensions.LogNewGame();
            }
            else if (_matrix.Mode == PlayingMode.Playing)
            {
                this.SetProgressIndicator(true, AppResources.PauseGameMessage);
                await _matrix.Pause();
                _playTimer.Stop();
                btnStartImage.Source = AppConfig.PLAY_IMAGE;
                //txtTitle.Text = AppResources.PausedTitle;
                this.SetProgressIndicator(false);
            }
            else if (_matrix.Mode == PlayingMode.Paused)
            {
                _matrix.Resume();
                _playTimer.Start();
                btnStartImage.Source = AppConfig.PAUSE_IMAGE;
                //txtTitle.Text = AppResources.PlayingTitle;
            }

            SetTitle(_matrix.Mode);
        }

        private void SetTitle(PlayingMode mode)
        {
            switch (mode)
            { 
                case PlayingMode.Initialized:
                    txtTitle.Text = AppResources.ApplicationTitle;
                    break;
                case PlayingMode.Lost:
                    txtTitle.Text = AppResources.LostTitle;
                    break;
                case PlayingMode.Paused:
                    txtTitle.Text = AppResources.PausedTitle;
                    break;
                case PlayingMode.Playing:
                    txtTitle.Text = AppResources.PlayingTitle;
                    break;
                case PlayingMode.Stopped:
                    txtTitle.Text = AppResources.StoppedTitle;
                    break;
            }
        }

        private async void btnStop_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_matrix.Mode != PlayingMode.Stopped)
            {
                this.SetProgressIndicator(true, AppResources.StartGameMessage);
                await StopGame();
                //txtTitle.Text = AppResources.StoppedTitle;
                this.SetProgressIndicator(false);
            }
            else
            {
                Messenger.ShowToast(AppResources.GameStoppedMessage);
            }

            SetTitle(_matrix.Mode);

            if (_currentGame != null)
            GAExtensions.LogEndGame(_currentGame.Score, _currentGame.Duration);
        }

        void _playTimer_Tick(object sender, EventArgs e)
        {
            _playingTime = _playingTime.Add(new TimeSpan(0, 0, 1));
            txtTime.Text = _playingTime.ToString();
        }

        void _matrix_ScoreEvent(int ballCount, int score)
        {
            txtBallCount.Text = AppResources.BallTitle + ": " + ballCount.ToString();
            txtScore.Text = AppResources.ScoreTitle + ": " + score.ToString();
            if (_maxScore != 0 && score > _maxScore)
            {
                sbScoreRotate.BeginAsync();
                _maxScore = score;
            }
        }

        void _matrix_MoveEvent(int moveCount)
        {
            txtMoveCount.Text = AppResources.MoveCountTitle + ": " + moveCount.ToString();
        }

        void _matrix_NextCellsEvent(CellData[] cellData)
        {
            for (int i = 0; i < cellData.Length; i++)
            {
                _proposalCells[i].Data.BallColor = cellData[i].BallColor;
                _proposalCells[i].Data.Number = cellData[i].Number;
            }
        }

        async void matrix_Notify(PlayingMode mode)
        {
            var message = string.Empty;
            switch (mode)
            { 
                case PlayingMode.Initialized:
                    message = AppResources.GameInitializedMessage;
                    break;
                case PlayingMode.Paused:
                    message = AppResources.GamePausedMessage;
                    break;
                case PlayingMode.Stopped:
                    await StopGame();
                    message = AppResources.GameStoppedMessage;
                    break;
                case PlayingMode.Lost:
                    await StopGame();
                    message = AppResources.GameLostMessage;
                    break;
            }

            Messenger.ShowToast(message);
        }

        private async Task StopGame()
        {
            await _matrix.Stop();
            _playTimer.Stop();

            _matrix.ScoreEvent -= _matrix_ScoreEvent;
            _matrix.MoveEvent -= _matrix_MoveEvent;
            _matrix.NextCellsEvent -= _matrix_NextCellsEvent;
            _matrix.NotifyEvent -= matrix_Notify;
            //_matrix.Dispose();

            SaveGame();
            btnStartImage.Source = AppConfig.PLAY_IMAGE;
            var message = string.Format(AppResources.LoseMessage, _matrix.Score);
            Messenger.ShowToast(message);
        }

        #region Private

        private void SaveGame()
        {
            if (_currentGame == null) return;

            var games = AppConfig.Games;

            if (games == null)
                games = new List<Game>();
            WrapPlayingGame();
            var lastPlayingGame = games.FirstOrDefault(g => g.Id.Equals(_currentGame.Id));
            games = games.Where(g => g.Mode == PlayingMode.Stopped).OrderByDescending(x => x.Score).Take(AppConfig.SAVED_GAMES_COUNT).ToList();
            if (lastPlayingGame != null)
                games.Remove(lastPlayingGame);
            games.Add(_currentGame);

            AppConfig.Games = games;
        }

        private void LoadGames()
        {
            var games = AppConfig.Games;

            if (games == null || games.Count == 0)
            {
                games = new List<Game>();
                _currentGame = null;
                _maxScore = 0;
            }
            else
            {
                _currentGame = games.FirstOrDefault(g => g != null && g.Mode == PlayingMode.Paused);
                _maxScore = games.Max(g => g.Score);
            }
        }

        private void WrapPlayingGame()
        {
            if (_currentGame == null) return;

            _currentGame.Score = _matrix.Score;
            _currentGame.Ball = _matrix.BallCount;
            _currentGame.Move = _matrix.MoveCount;
            _currentGame.Duration = _playingTime;
            _currentGame.EntryDate = DateTime.Now;
            _currentGame.HighestScore = _matrix.HighestScore;
            _currentGame.Mode = _matrix.Mode;
            _currentGame.UseNumber = _matrix.UseNumber;
            _currentGame.CellData = _matrix.ExportData();
            _currentGame.NextCells = _matrix.NextCells;
        }

        #endregion

        //Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.settings.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarButtonText;
            appBarButton.Click += appBarButton_Click;

            var historyButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/folder.png", UriKind.Relative));
            historyButton.Text = AppResources.HistoryAppBarButtonText;
            historyButton.Click += historyButton_Click;

            ApplicationBar.Buttons.Add(appBarButton);
            ApplicationBar.Buttons.Add(historyButton);
        }

        void historyButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/HistoryPage.xaml", UriKind.Relative));
        }

        void appBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        void PopulateAds()
        {
            if (App.AdsProvider == AdsProvider.Smaato)
            {
                smaato.Visibility = System.Windows.Visibility.Visible;
                adduplex.Visibility = System.Windows.Visibility.Collapsed;
                smaato.StartAds();
            }
            if (App.AdsProvider == AdsProvider.AdDuplex)
            {
                adduplex.Visibility = System.Windows.Visibility.Visible;
                smaato.Visibility = System.Windows.Visibility.Collapsed;
            }

            var adsDictionary = AppConfig.AdsLoaded;
            adsDictionary.AppendValue(App.AdsProvider, 1);
            if (adsDictionary.ContainsKey(App.AdsProvider))
                GAExtensions.LogAdsLoad(App.AdsProvider, adsDictionary[App.AdsProvider]);

            AppConfig.AdsLoaded = adsDictionary;
        }
    }
}