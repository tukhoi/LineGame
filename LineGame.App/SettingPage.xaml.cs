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
using LineGame.App.Resources;
using LineGame.AppServices;
using Microsoft.Phone.Tasks;
using Davang.Utilities.Extensions;

namespace LineGame.App
{
    public partial class SettingPage : LinePage
    {
        LineMatrix _matrix;

        public SettingPage()
        {
            InitializeComponent();
            Initialize();
            BindList();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var selected = AppConfig.Backgrounds.FirstOrDefault(b => b.Name.Equals(AppConfig.Background.Name));
            lpkBackground.SelectedItem = selected;
            lpkBackground.SelectionChanged += lpkBackground_SelectionChanged;

            chkSoundEnabled.IsChecked = AppConfig.SoundEnabled;
            chkUseNumber.IsChecked = AppConfig.UseNumber;
        }

        void lpkBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null || e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            var background = lpkBackground.SelectedItem as Background;
            if (background == null) return;
            AppConfig.Background = background;
            if (_matrix != null)
                _matrix.SetBackground(background.FileName);
            
        }

        private void Initialize()
        {
            InitializeMatrix();

            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var version = string.Format(AppResources.VersionMessage, assemblyVersion.ToString());
            txtVersion.Text = version;
            txtClientId.Text = AppConfig.ClientId.ToString();
        }

        private void InitializeMatrix()
        {
            _matrix = new LineMatrix(3, 3, 2, 1, 3, 4, AppConfig.UseNumber);
            _matrix.SetBackground(AppConfig.Background.FileName);
            cvMatrix.Children.Clear();
            cvMatrix.Children.Add(_matrix);
            _matrix.Start(false);
        }

        private void chkSoundEnabled_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.SoundEnabled = chkSoundEnabled.IsChecked.HasValue ?
                chkSoundEnabled.IsChecked.Value : true;
        }

        private void chkUseNumber_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.UseNumber = chkUseNumber.IsChecked.HasValue ?
                chkUseNumber.IsChecked.Value : true;

            InitializeMatrix();
        }

        private void btnRating_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask oRateTask = new MarketplaceReviewTask();
            oRateTask.Show();
        }

        private void BindList()
        {
            lpkBackground.ItemsSource = AppConfig.Backgrounds;
        }
    }
}