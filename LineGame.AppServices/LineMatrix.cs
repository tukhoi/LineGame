using LineGame.AppServices;
using LineGame.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Davang.Utilities.Extensions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LineGame.AppServices.Sound;
using LineGame.AppServices.Scoring;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace LineGame.AppServices
{
    public class LineMatrix : Grid, IDisposable
    {
        CellData[] _nextCells = new CellData[3];
        Random _random = new Random();
        Cell _lastSelectedCell;
        State[] _availableStates = new State[] { State.Empty, State.Proposed };

        private IList<Cell> _cells;
        public PlayingMode Mode { get; set; }
        private int _score;
        private int _ballCount;
        private int _moveCount;
        private bool _useNumber;
        private byte _highestScore;

        private ISoundEffect _player;
        private IScoring _scorer; 

        public delegate void ScoreDelegate(int ballCount, int score);
        public delegate void MoveDelegate(int moveCount);
        public delegate void NextCellsDelegate(CellData[] colors);
        public delegate void NotifyDelegate(PlayingMode gameMode);

        public event ScoreDelegate ScoreEvent;
        public event MoveDelegate MoveEvent;
        public event NextCellsDelegate NextCellsEvent;
        public event NotifyDelegate NotifyEvent;

        public int Score 
        { 
            get { return _score; }
            set { _score = value; }
        }

        public int BallCount 
        { 
            get { return _ballCount; }
            set { _ballCount = value; }
        }

        public int MoveCount 
        { 
            get { return _moveCount; }
            set { _moveCount = value; }
        }

        public bool UseNumber
        {
            get { return _useNumber; }
            set { _useNumber = value; }
        }

        public CellData[] NextCells
        {
            get { return _nextCells; }
            set { _nextCells = value; }
        }

        public byte HighestScore
        {
            get { return _highestScore; }
            set { _highestScore = value; }
        }

        public LineMatrix(sbyte rowCount = AppConfig.ROW_COUNT, sbyte columnCount = AppConfig.COLUMN_COUNT, byte startBalls = 10, byte proposedCells = 3, byte winningBalls = AppConfig.WINNING_LINE, byte colorsLimit = 0, bool useNumber=false, CellData[] nextCells = null)
        {
            _rowCount = rowCount;
            _columnCount = columnCount;
            _startBalls = startBalls;
            _proposedCells = proposedCells;
            _winningBalls = winningBalls;
            _colorsLimit = colorsLimit == 0 ? (byte)(Enum.GetNames(typeof(BallColor)).Length) : colorsLimit;
            _useNumber = useNumber;
            _nextCells = nextCells;
            _highestScore = 0;

            _cells = new List<Cell>();
            DrawMatrix();
            //this.IsHitTestVisible = false;
            Mode = PlayingMode.Initialized;
            _player = new StandardSoundEffect();
            _scorer = new BasicScoring(_winningBalls, _useNumber);
        }

        private sbyte _rowCount;
        private sbyte _columnCount;
        private byte _startBalls;
        private byte _proposedCells;
        private byte _winningBalls;
        private byte _colorsLimit;

        #region Import/Export

        public IList<CellData> ExportData()
        {
            var cellData = new List<CellData>();
            _cells.ForEach(c => cellData.Add(c.Data));

            return cellData;
        }

        public void ImportData(IList<CellData> cellData)
        {
            cellData.ForEach(cd =>
                {
                    var myCell = _cells.FirstOrDefault(mc => mc.Data.Id.Equals(cd.Id));
                    if (myCell != null)
                        myCell.Data = cd;
                });
        }

        #endregion

        #region Control

        public void Start(bool playSound = true)
        {
            _score = 0;
            _ballCount = 0;
            
            _cells.ForEach(c => c.Data.State = State.Empty);
            GenerateFilledCells(_startBalls);
            GenerateProposedCells(_proposedCells);
            _nextCells = PickNextCells(_random, 3);
            //this.IsHitTestVisible = true;
            Mode = PlayingMode.Playing;
            if (playSound)
                _player.StartSound();
        }

        public void Resume()
        {
            //this.IsHitTestVisible = true;
            this.Mode = PlayingMode.Playing;
            _player.StartSound();
        }

        public async Task Stop()
        {
            await Sleep(PlayingMode.Stopped);
            _player.LoseSound();
        }

        public async Task Lost()
        {
            await Sleep(PlayingMode.Lost);
            _player.LoseSound();
        }

        public async Task Pause()
        {
            await Sleep(PlayingMode.Paused);
            _player.StartSound();
        }

        private async Task Sleep(PlayingMode sleepMode)
        {
            _lastSelectedCell = null;
            //this.IsHitTestVisible = false;
            this.Mode = sleepMode;
            foreach (var cell in _cells.Where(c => c.Data.State == State.Selected))
            {
                cell.Data.State = State.Filled;
                await cell.SetBounce(false);
            };
        }

        public void Dispose()
        {
            if (_cells != null && _cells.Count > 0)
                _cells.ForEach(cell =>
                    {
                        cell.SelectedEvent -= cell_TapEvent;
                        cell.Dispose();
                    });

            this._cells.Clear();
            this._nextCells = null;
            this._lastSelectedCell = null;
        }

        #endregion

        #region EventHandler

        async Task cell_TapEvent(sbyte cellId)
        {
            if (Mode != PlayingMode.Playing)
            {
                if (NotifyEvent != null)
                    NotifyEvent(Mode);
                return;
            }

            foreach (var cell in _cells.Where(c => !c.Data.Id.Equals(cellId) && c.Data.State == State.Selected).ToList())
            {
                cell.Data.State = State.Filled;
                await cell.SetBounce(false);
            }
            var currentCell = _cells.Where(c => c.Data.Id.Equals(cellId)).FirstOrDefault();

            switch (currentCell.Data.State)
            {
                case State.Empty:
                case State.Proposed:
                    if (_lastSelectedCell != null)
                    {
                        this.IsHitTestVisible = false;
                        if (await Moving(currentCell, _lastSelectedCell))
                        {
                            if (!await RemoveLinedBalls(AppConfig.UseNumber))
                            {
                                await GrowUp();
                                await RemoveLinedBalls(AppConfig.UseNumber);
                                GenerateProposedCells(_proposedCells);
                                _moveCount++;
                                if (MoveEvent != null)
                                    MoveEvent(_moveCount);
                            }
                        }
                        _lastSelectedCell = null;
                        this.IsHitTestVisible = true;
                    }
                    break;
                case State.Filled:
                    await currentCell.SetBounce(false);
                    break;
                case State.Selected:
                    _lastSelectedCell = _cells.Where(c => c.Data.Id.Equals(cellId)).FirstOrDefault();
                    _player.SelectedSound();
                    await currentCell.SetBounce(true);
                    break;
            }

            if (IsLost())
            {
                //this.Mode = PlayingMode.Lost;
                await Lost();
                //this.IsHitTestVisible = false;
                _player.LoseSound();
                if (NotifyEvent != null)
                    NotifyEvent(Mode);
            }
        }

        private async Task<bool> Moving(Cell currentCell, Cell lastSelectedCell)
        {
            if (lastSelectedCell != null && currentCell != null)
            {
                var path = WideFirstSearch(lastSelectedCell, currentCell);
                if (path != null)
                {
                    //_player.MovingEffect();
                    await MoveBall(path);
                    _moveCount++;
                    
                    return true;
                }
            }
            _player.UnabledToMoveSound();
            return false;
        }

        private async Task GrowUp()
        {
            _player.GrowUpSound();
            foreach (var cell in _cells.Where(c => c.Data.State == State.Proposed)
                                                    .OrderBy(c => c.Data.Id)
                                                    .ToList())
            {
                await cell.GrowUp();
            }
        }

        #endregion

        #region Moving ball

        private IList<Cell> WideFirstSearch(Cell startCell, Cell endCell)
        {
            startCell.PreviousCellInPath = startCell;
            var aroundCells = GetAroundAvailableCells(startCell);
            if (aroundCells != null && aroundCells.Count > 0)
                aroundCells.ForEach(c => c.PreviousCellInPath = startCell);
            IList<Cell> path = null;
            var reached = Reach(endCell, aroundCells);
            if (reached)
            {
                path = new List<Cell>() { endCell };
                CollectPath(endCell, startCell, path);
                path = path.Reverse().ToList();
            }
            _cells.ForEach(c => c.PreviousCellInPath = null);
            return path;
        }

        private async Task MoveBall(IList<Cell> path)
        {
            var currentData = path[0].Data.Clone();
            path[0].Data.State = State.Empty;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var fromCenter = i == 0;
                await path[i].MoveTo(path[i + 1], currentData, fromCenter, false);
            }

            await path[path.Count - 1].MoveFrom(path[path.Count - 2], currentData, false, true);
            var trend = path[path.Count - 2].Data.Position.IsNeighbourPosition(path[path.Count - 1].Data.Position);
            path[path.Count - 1].Data.State = State.Filled;
            path[path.Count - 1].Data.BallColor = currentData.BallColor;
            path[path.Count - 1].Data.Number = currentData.Number;
            //await KickNeighbourBall(currentColor, path[path.Count - 1], trend);
        }

        //private async Task KickNeighbourBall(Cell stopCell, NeighbourPostion trend)
        //{
        //    switch (trend)
        //    { 
        //        case NeighbourPostion.Right:
        //            await stopCell.MoveRight(stopCell.Data.BallColor, false, true);
                    
        //            var rightPosition = stopCell.Data.Position.GetNeighbourPosition(trend);
        //            var rightCell = GetCell(rightPosition);
        //            if (rightCell != null && rightCell.Data.State == State.Filled)
        //                await KickNeighbourBall(rightCell, trend);    
        //            else
        //                return;
        //            break;
        //        case NeighbourPostion.Left:
        //            //var leftPosition = stopCell.Data.Position.GetNeighbourPosition(trend);
        //            //var leftCell = GetCell(leftPosition);
        //            //if (leftCell != null && leftCell.Data.State == State.Filled)
        //            //    await stopCell.MoveLeft(leftCell.Data.BallColor, false, true);
        //            //else
        //            //    return;
        //            //await KickNeighbourBall(leftCell, trend);
        //            //break;
        //        case NeighbourPostion.Up:
        //            //var upPosition = stopCell.Data.Position.GetNeighbourPosition(trend);
        //            //var upCell = GetCell(upPosition);
        //            //if (upCell != null && upCell.Data.State == State.Filled)
        //            //    await stopCell.MoveUp(upCell.Data.BallColor, false, true);
        //            //else
        //            //    return;
        //            //await KickNeighbourBall(upCell, trend);
        //            //break;
        //        case NeighbourPostion.Down:
        //            var downPosition = stopCell.Data.Position.GetNeighbourPosition(trend);
        //            var downCell = GetCell(downPosition);
        //            if (downCell != null && downCell.Data.State == State.Filled)
        //                await stopCell.MoveDown(downCell.Data.BallColor, false, true);
        //            else
        //                return;
        //            await KickNeighbourBall(downCell, trend);
        //            break;
        //        case NeighbourPostion.UpperRight:
        //            var upperRightPosition = stopCell.Data.Position.GetNeighbourPosition(trend);
        //            var upperRightCell = GetCell(upperRightPosition);
        //            if (upperRightCell != null && upperRightCell.Data.State == State.Filled)
        //                await stopCell.MoveDown(downCell.Data.BallColor, false, true);
        //            else
        //                return;
        //            await KickNeighbourBall(upperRightCell, trend);
        //            break;
        //    }
        //}

        private async Task KickLinedBalls(IList<Cell> cells)
        {
            var trend = cells[0].Data.Position.IsNeighbourPosition(cells[1].Data.Position);
            switch (trend)
            { 
                case NeighbourPostion.Right:
                    foreach (var cell in cells)
                        await cell.MoveRight(cell.Data, toCenter: true);
                    break;
                case NeighbourPostion.Down:
                case NeighbourPostion.BottomRight:
                case NeighbourPostion.BottomLeft:
                    foreach (var cell in cells)
                        await cell.MoveUp(cell.Data, toCenter: true);
                    break;
            }
        }

        private async Task<bool> RemoveLinedBalls(bool checkOrder = false)
        {
            var linedBalls = SearchForLinedBalls(checkOrder);
            if (linedBalls != null && linedBalls.Count > 0)
            {
                //linedBalls = linedBalls.Distinct().OrderBy(b => b.Data.Number).ThenBy(b => b.Data.Id).ToList();
                //var trend = linedBalls[0].Data.Position.IsNeighbourPosition(linedBalls[1].Data.Position);
                //await KickLinedBalls(linedBalls);
                linedBalls = linedBalls.Distinct().OrderBy(b => b.Data.Id).ToList();

                var scoreResult = _scorer.Score(linedBalls);
                foreach (var ball in linedBalls)
                {
                    _player.DisappearSound();
                    await ball.Disappear();
                    ball.Data.State = State.Empty;
                }

                await linedBalls[linedBalls.Count - 1].ShowScore(scoreResult.Value, scoreResult.Value > _highestScore);
                _highestScore = scoreResult.Value > _highestScore ? scoreResult.Value : _highestScore;

                _score += scoreResult.Value;
                _ballCount += linedBalls.Count;

                if (ScoreEvent != null)
                    ScoreEvent(_ballCount, _score);
                _highestScore = scoreResult.Value > _highestScore ? scoreResult.Value : _highestScore;
                
                return true;
            }
            return false;
        }
        
        #endregion

        #region GetCell

        private IList<Cell> GetAroundAvailableCells(Cell cell)
        {
            var aroundCells = new List<Cell>();

            var leftCell = GetAvailableCell((sbyte)(cell.Data.Position.X - 1), cell.Data.Position.Y);
            var upCell = GetAvailableCell(cell.Data.Position.X, (sbyte)(cell.Data.Position.Y - 1));
            var rightCell = GetAvailableCell((sbyte)(cell.Data.Position.X + 1), cell.Data.Position.Y);
            var downCell = GetAvailableCell(cell.Data.Position.X, (sbyte)(cell.Data.Position.Y + 1));

            if (leftCell != null) aroundCells.Add(leftCell);
            if (upCell != null) aroundCells.Add(upCell);
            if (rightCell != null) aroundCells.Add(rightCell);
            if (downCell != null) aroundCells.Add(downCell);

            return aroundCells;
        }

        private Cell GetCell(sbyte columnIndex, sbyte rowIndex)
        {
            return GetCell(new Position(columnIndex, rowIndex));
        }

        private Cell GetCell(Position position)
        {
            return _cells.Where(c => c.Data.Position.Equals(position)).FirstOrDefault();
        }

        private Cell GetAvailableCell(sbyte columnIndex, sbyte rowIndex)
        {
            return GetAvailableCell(new Position(columnIndex, rowIndex));
        }

        private Cell GetAvailableCell(Position position)
        {
            var cell = GetCell(position);
            if (cell != null
                && _availableStates.Contains(cell.Data.State)
                && cell.PreviousCellInPath == null)
                return cell;

            return null;
        }

        #endregion

        #region Interface

        private void DrawMatrix()
        {
            this.Children.Clear();

            this.Height = AppConfig.CELL_HEIGHT * _rowCount;
            this.Width = AppConfig.CELL_WIDTH * _columnCount;

            for (sbyte i = 0; i < _rowCount; i++)
                this.RowDefinitions.Add(new RowDefinition());
            for (sbyte j = 0; j < _columnCount; j++)
                this.ColumnDefinitions.Add(new ColumnDefinition());

            for (sbyte i = 0; i < _rowCount; i++)
                for (sbyte j = 0; j < _columnCount; j++)
                {
                    var position = new Position(j, i);
                    var cell = new Cell(position, transparentBackground:true, useNumber:_useNumber);
                    cell.Data.Id = AxisHelper.ConvertToLinear(position);
                    cell.Margin = new Thickness(0, 0, 0, 0);
                    cell.SelectedEvent += cell_TapEvent;
                    _cells.Add(cell);
                    this.Children.Add(cell);

                    Grid.SetColumn(cell, j);
                    Grid.SetRow(cell, i);
                }
        }

        public void SetBackground(string backgroundImage)
        {
            var background = new ImageBrush();
            background.ImageSource = new BitmapImage(new Uri(backgroundImage, UriKind.Relative))
            {
                CreateOptions = BitmapCreateOptions.BackgroundCreation,
            };
            this.Background = background;
        }

        private void GenerateFilledCells(byte expectedCount = 3)
        {
            GenerateCells(State.Filled, expectedCount: expectedCount);
        }

        private void GenerateProposedCells(byte expectedCount = 3)
        {
            GenerateCells(State.Proposed, _nextCells, expectedCount);
            _nextCells = PickNextCells(_random, 3);
            if (NextCellsEvent != null)
                NextCellsEvent(_nextCells);
        }

        private void GenerateCells(State state, CellData[] _nextCells = null, byte expectedCount = 3)
        {
            var cells = PickupRandomCellsByState(_random, State.Empty, expectedCount);
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Data.State = state;
                if (_nextCells != null)
                {
                    cells[i].Data.BallColor = _nextCells[i].BallColor;
                    if (_useNumber)
                        cells[i].Data.Number = _nextCells[i].Number;
                }
                else
                {
                    cells[i].Data.BallColor = PickRandomColors(_random, 1)[0];
                    if (_useNumber)
                        cells[i].Data.Number = GetNextNumber(_random, cells[i].Data.BallColor);
                }
            }
        }

        private byte GetNextNumber(Random random, BallColor ballColor)
        {
            var count = _cells.Count(c => c.Data.BallColor == ballColor && c.Data.Number != 0) + 1;
            var theyare = _cells.Where(c => c.Data.BallColor == ballColor).ToList();
            if (count > _winningBalls)
                count = random.Next(1, _winningBalls + 1);
            return (byte)(count) ;
        }

        private IList<Cell> PickupRandomCellsByState(Random random, State state, byte expectedCount = 3, bool useNumber = false)
        {
            var availableCells = _cells.Where(c => c.Data.State == state).ToList();
            if (availableCells.Count <= expectedCount)
                return availableCells;

            var cells = new List<Cell>();
            for (byte i = 0; i< expectedCount; i++)
            {
                var cell = availableCells.PlayDice(c => 1, random);
                cells.Add(cell);
                availableCells.Remove(cell);
            }

            return cells;
        }

        private CellData[] PickNextCells(Random random, byte expectedCount = 3)
        {
            var cells = new CellData[expectedCount];
            byte i = 0;
            while (i < expectedCount)
            {
                var cellData = new CellData();
                cellData.BallColor = PickRandomColors(random, 1)[0];
                if (_useNumber)
                    cellData.Number = GetNextNumber(random, cellData.BallColor);
                cells[i] = cellData;
                i++;
            }

            return cells;
        }

        private BallColor[] PickRandomColors(Random random, byte expectedCount = 3)
        {
            var colors = new BallColor[expectedCount];
            byte i = 0;
            while (i < expectedCount)
            { 
                var color = random.Next(1, _colorsLimit);
                //color = 3;
                colors[i] = (BallColor)color;
                i++;
            }

            return colors;
        }

        #endregion

        #region Private

        private bool Reach(Cell endCell, IList<Cell> cellsToReach)
        {
            if (cellsToReach == null || cellsToReach.Count == 0)
                return false;

            if (cellsToReach.Contains(endCell)) return true;

            var newCellsToReach = new List<Cell>();
            foreach (var cell in cellsToReach)
            {
                var aroundCells = GetAroundAvailableCells(cell);
                if (aroundCells != null && aroundCells.Count > 0)
                {
                    aroundCells.ForEach(c => c.PreviousCellInPath = cell);
                    //if (aroundCells.Contains(endCell)) return true;
                    newCellsToReach.AddRange(aroundCells);
                }
            }

            return Reach(endCell, newCellsToReach);
        }

        private void CollectPath(Cell cell, Cell startCell, IList<Cell> path)
        {
            if (cell.PreviousCellInPath.Equals(startCell))
            {
                path.Add(startCell);
                return;
            }

            if (cell.PreviousCellInPath != null)
            {
                path.Add(cell.PreviousCellInPath);
                CollectPath(cell.PreviousCellInPath, startCell, path);
            }
        }

        private IList<Cell> SearchForLinedBalls(bool checkOrder = false)
        {
            var cells = new List<Cell>();

            _cells.Where(c => c.Data.State == State.Filled || c.Data.State == State.Selected).ForEach(c =>
                {
                    var rightCells = SearchForWiningCells(c, NeighbourPostion.Right, checkOrder);
                    if (rightCells != null && rightCells.Count > 0)
                        cells.AddRange(rightCells);

                    var leftCells = SearchForWiningCells(c, NeighbourPostion.Left, checkOrder);
                    if (leftCells != null && leftCells.Count > 0)
                        cells.AddRange(leftCells);

                    var upCells = SearchForWiningCells(c, NeighbourPostion.Up, checkOrder);
                    if (upCells != null && upCells.Count > 0)
                        cells.AddRange(upCells);

                    var downCells = SearchForWiningCells(c, NeighbourPostion.Down, checkOrder);
                    if (downCells != null && downCells.Count > 0)
                        cells.AddRange(downCells);

                    var upperLeftCells = SearchForWiningCells(c, NeighbourPostion.UpperLeft, checkOrder);
                    if (upperLeftCells != null && upperLeftCells.Count > 0)
                        cells.AddRange(upperLeftCells);

                    var upperRightCells = SearchForWiningCells(c, NeighbourPostion.UpperRight, checkOrder);
                    if (upperRightCells != null && upperRightCells.Count > 0)
                        cells.AddRange(upperRightCells);

                    var bottomLeftCells = SearchForWiningCells(c, NeighbourPostion.BottomLeft, checkOrder);
                    if (bottomLeftCells != null && bottomLeftCells.Count > 0)
                        cells.AddRange(bottomLeftCells);

                    var bottomRightCells = SearchForWiningCells(c, NeighbourPostion.BottomRight, checkOrder);
                    if (bottomRightCells != null && bottomRightCells.Count > 0)
                        cells.AddRange(bottomRightCells);
                });

            return cells;
        }

        private IList<Cell> SearchForWiningCells(Cell startCell, NeighbourPostion neighbourPosition, bool checkOrder = false)
        {
            var cells = new List<Cell>();
            byte i = 1;
            var currentPosition = startCell.Data.Position;
            var startNumber = startCell.Data.Number;
            while (currentPosition != null)
            {
                var nextPosition = currentPosition.GetNeighbourPosition(neighbourPosition);
                if (nextPosition == null) break;

                var nextCell = _cells.Where(c => c.Data.Position.Equals(nextPosition)).FirstOrDefault();
                if (nextCell != null
                    && nextCell.Data.BallColor == startCell.Data.BallColor
                    && (nextCell.Data.State == State.Filled || nextCell.Data.State == State.Selected))
                {
                    cells.Add(nextCell);
                    currentPosition = nextPosition;
                    i++;
                }
                else
                    break;
            }

            if (i >= _winningBalls)
            {
                cells.Insert(0, startCell);
                return cells;
            }

            return null;
        }
        
        private bool IsLost()
        {
            var emptyCells = _cells.Count(c => c.Data.State == State.Empty);
            return emptyCells < _proposedCells;
        }

        #endregion
    }
}
