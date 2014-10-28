using Davang.Utilities.Helpers;
using Davang.Utilities.Helpers.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LineGame.AppServices
{
    public class AppConfig
    {
        public const sbyte COLUMN_COUNT = 9;
        public const sbyte ROW_COUNT = 10;
        public const byte WINNING_LINE = 5;

        public static sbyte CELL_WIDTH = 50;
        public static sbyte CELL_HEIGHT = 50;
        public static sbyte BALL_WIDTH = 20;
        public static sbyte BALL_HEIGHT = 20;
        public static TimeSpan MOVING_ANIMATE_TIME = TimeSpan.FromSeconds(0.001);
        private static Uri PLAY_BUTTON = new Uri("/Resources/play.png", UriKind.Relative);
        private static Uri PAUSE_BUTTON = new Uri("/Resources/pause.png", UriKind.Relative);
        private static Uri STOP_BUTTON = new Uri("/Resources/stop.png", UriKind.Relative);

        public static BitmapImage PLAY_IMAGE = new BitmapImage(PLAY_BUTTON);
        public static BitmapImage PAUSE_IMAGE = new BitmapImage(PAUSE_BUTTON);
        public static BitmapImage STOP_IMAGE = new BitmapImage(STOP_BUTTON);

        public static sbyte SAVED_GAMES_COUNT = 10;

        private static SerializationHelperManager _serializationManager = new SerializationHelperManager();
        private static IDictionary<ConfigKey, object> _memConfigs;
        public static IList<Background> Backgrounds;

        public static string GA_ID = "UA-52115271-2";
        public static string GA_APP_NAME = "linegame";
        
        static AppConfig()
        {
            _memConfigs = new Dictionary<ConfigKey, object>();
            Backgrounds = new List<Background>();
            Backgrounds.Add(new Background("wooden", "/Resources/background3.png"));
            Backgrounds.Add(new Background("red cloud", "/Resources/background4.png"));
            Backgrounds.Add(new Background("modern blue", "/Resources/background5.png"));
            Backgrounds.Add(new Background("light blue", "/Resources/background6.png"));
            Backgrounds.Add(new Background("bright wooden", "/Resources/background7.png"));
            Backgrounds.Add(new Background("dark blue", "/Resources/background8.png"));
            Backgrounds.Add(new Background("rain", "/Resources/background9.png"));
        }

        #region Property

        public static Guid ClientId
        {
            get
            {
                var clientId = GetConfig<Guid>(ConfigKey.ClientId, default(Guid));
                if (default(Guid).Equals(clientId))
                {
                    clientId = Guid.NewGuid();
                    SetConfig<Guid>(ConfigKey.ClientId, clientId);
                }

                return clientId;
            }
        }

        public static IList<Game> Games
        {
            get
            {
                return GetConfig<IList<Game>>(ConfigKey.Games, null);
            }
            set
            {
                SetConfig<IList<Game>>(ConfigKey.Games, value);
            }
        }

        public static bool SoundEnabled
        {
            get
            {
                return GetConfig<bool>(ConfigKey.SoundEnabled, true);
            }
            set
            {
                SetConfig<bool>(ConfigKey.SoundEnabled, value);
            }
        }

        public static AdsProvider LastUsedAdsProvider
        {
            get
            {
                return GetConfig<AdsProvider>(ConfigKey.LastUsedAdsProvider, AdsProvider.AdDuplex);
            }
            set
            {
                SetConfig<AdsProvider>(ConfigKey.LastUsedAdsProvider, value);
            }
        }

        public static Background Background
        {
            get
            {
                var background = GetConfig<Background>(ConfigKey.Background, null);
                if (background == null || string.IsNullOrEmpty(background.Name) || string.IsNullOrEmpty(background.FileName))
                {
                    background = Backgrounds.FirstOrDefault(b => b.Name.Equals("rain"));
                    SetConfig<Background>(ConfigKey.Background, background);
                }

                return background;
            }
            set
            {
                SetConfig<Background>(ConfigKey.Background, value);
            }
        }

        public static bool UseNumber
        {
            get
            {
                return GetConfig<bool>(ConfigKey.UseNumber, false);
            }
            set
            {
                SetConfig<bool>(ConfigKey.UseNumber, value);
            }
        }

        public static IDictionary<AdsProvider, long> AdsLoaded
        {
            get
            {
                return GetConfig<IDictionary<AdsProvider, long>>(ConfigKey.AdsLoaded, new Dictionary<AdsProvider, long>());
            }
            set
            {
                SetConfig<IDictionary<AdsProvider, long>>(ConfigKey.AdsLoaded, value);
            }
        }

        #endregion
                
        #region Private

        private static T GetConfig<T>(ConfigKey key, T defaultValue)
        {
            if (!_memConfigs.ContainsKey(key))
            {
                var persistentValue = GetPersistentConfig<T>(key, defaultValue);
                _memConfigs[key] = persistentValue;
            }

            return (T)_memConfigs[key];
        }

        private static void SetConfig<T>(ConfigKey key, T value)
        {
            if (!_memConfigs.ContainsKey(key))
                _memConfigs.Add(key, value);
            else
                _memConfigs[key] = value;
            SetPersistentConfig(key, value);
        }

        private static T GetPersistentConfig<T>(ConfigKey key, T defaultValue)
        {
            object value = StorageHelper.LoadConfig(key.ToString());
            if (value != null)
                return (T)value;
            return defaultValue;
        }

        private static void SetPersistentConfig<T>(ConfigKey key, T value)
        {
            StorageHelper.SaveConfig(key.ToString(), value);
        }

        #endregion
    }
}
