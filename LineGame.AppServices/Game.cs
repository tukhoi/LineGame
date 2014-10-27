using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices
{
    public class Game
    {
        public Guid Id { get; set; }
        public int Score { get; set; }
        public int Ball { get; set; }
        public int Move { get; set; }
        public bool UseNumber { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EntryDate { get; set; }
        public byte HighestScore { get; set; }
        public PlayingMode Mode { get; set; }
        public IList<CellData> CellData { get; set; }
        public CellData[] NextCells { get; set; }
    }
}
