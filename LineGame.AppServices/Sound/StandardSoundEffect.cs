using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace LineGame.AppServices.Sound
{
    public class StandardSoundEffect : ISoundEffect
    {
        private const string SELECTED_WAV = "selected.wav";
        private const string DISAPPEAR_WAV = "disappear.wav";
        private const string UNABLED_MOVE_WAV = "unabledmove.wav";
        private const string GROWUP_WAV = "growup.wav";
        private const string LOSE_WAV = "lose.wav";
        private const string START_WAV = "start.wav";

        private const string ASSEMBLY = "LineGame.App";

        public void SelectedSound()
        {
            PlaySound(SELECTED_WAV);
        }

        public void UnabledToMoveSound()
        {
            PlaySound(UNABLED_MOVE_WAV);
        }

        public void DisappearSound()
        {
            PlaySound(DISAPPEAR_WAV);
        }

        public void GrowUpSound()
        {
            PlaySound(GROWUP_WAV);
        }

        public void LoseSound()
        {
            PlaySound(LOSE_WAV);
        }

        public void StartSound()
        {
            PlaySound(START_WAV);
        }

        private void PlaySound(string soundUri)
        {
            if (!AppConfig.SoundEnabled) return;

            try
            {
                var resourceStreamUri = string.Format("/{0};component/Resources/sounds/{1}", ASSEMBLY, soundUri);
                StreamResourceInfo _stream = Application.GetResourceStream(new Uri(resourceStreamUri, UriKind.Relative));
                SoundEffect _soundeffect = SoundEffect.FromStream(_stream.Stream);
                SoundEffectInstance soundInstance = _soundeffect.CreateInstance();
                FrameworkDispatcher.Update();
                soundInstance.Play();
            }
            catch (Exception ex)
            {
                int i = 1;
            }
        }
    }
}
