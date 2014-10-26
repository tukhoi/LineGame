using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices.Sound
{
    public interface ISoundEffect
    {
        void SelectedSound();
        void UnabledToMoveSound();
        void DisappearSound();
        void GrowUpSound();
        void LoseSound();
        void StartSound();
    }
}
