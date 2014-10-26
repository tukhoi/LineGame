using LineGame.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices.Scoring
{
    interface IScoring
    {
        KeyValuePair<ScoreResult, byte> Score(IList<Cell> linedBalls);
    }


}
