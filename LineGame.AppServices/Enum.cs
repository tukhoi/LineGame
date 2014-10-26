using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices
{
    public enum State
    {
        Empty,
        Proposed,
        Filled,
        Selected
    }

    public enum BallColor
    {
        None,
        SteelBlue,
        PaleVioletRed,
        OliveDrab,
        LightSeaGreen,
        IndianRed,
        DarkGoldenrod,
    }

    public enum NeighbourCell
    {
        LeftCell,
        UpCell,
        RightCell,
        DownCell
    }

    public enum NeighbourPostion
    {
        None,
        Left,
        Up,
        Right,
        Down,
        UpperLeft,
        UpperRight,
        BottomLeft,
        BottomRight
    }

    public enum PlayingMode
    {
        Initialized,
        Playing,
        Paused,
        Stopped,
        Lost
    }

    public enum ConfigKey
    {
        ClientId,
        Games,
        SoundEnabled,
        LastUsedAdsProvider,
        Background,
        UseNumber
    }

    public enum AdsProvider
    { 
        AdDuplex,
        Smaato
    }

    public enum ScoreResult
    { 
        Basic,
        Ordered,
        ExactOrdered
    }
}
