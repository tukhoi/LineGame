
using LineGame.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices.Scoring
{
    public class BasicScoring : IScoring
    {
        private byte _winningBalls;
        private bool _useNumber;

        public BasicScoring(byte winningBalls = 5, bool useNumber = false)
        {
            _winningBalls = winningBalls;
            _useNumber = useNumber;
        }

        public KeyValuePair<ScoreResult, byte> Score(IList<Cell> linedBalls)
        {
            var basicScore = BallsToScore((byte)linedBalls.Count);
            ScoreResult scoreResult = ScoreResult.Basic;
            var orderedScore = 0;

            if (_useNumber)
            {
                var orderedBalls = InOrder(linedBalls, (x, y) => { return x == y - 1; });
                if (orderedBalls > 0)
                {
                    orderedScore = orderedBalls * 2;
                    scoreResult = ScoreResult.ExactOrdered;
                }
                else
                {
                    orderedBalls = InOrder(linedBalls, (x, y) => { return x <= y; });
                    orderedScore = orderedBalls;
                    scoreResult = ScoreResult.Ordered;
                }
            }

            return new KeyValuePair<ScoreResult, byte>(scoreResult, (byte)(basicScore + orderedScore));
        }

        private byte BallsToScore(byte balls)
        {
            if (balls >= _winningBalls)
                return (byte)(_winningBalls + (balls - _winningBalls) * 2);
            return 0;
        }

        private byte InOrder(IList<Cell> cells, Func<byte, byte, bool> func)
        {
            cells = cells.OrderBy(c => c.Data.Id).ToList();
            byte i = 0;
            while (i + 1 < cells.Count && func(cells[i].Data.Number, cells[i+1].Data.Number))
                i++;

            if (i >= _winningBalls - 1)
                return (byte)(i + 1);

            i = 0;
            while (i + 1 < cells.Count && func(cells[i+1].Data.Number, cells[i].Data.Number))
                i++;
            if (i >= _winningBalls - 1)
                return (byte)(i + 1);

            return 0;
        }
    }
}
