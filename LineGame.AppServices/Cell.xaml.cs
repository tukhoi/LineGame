using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Davang.WP.Utilities.Extensions;

namespace LineGame.AppServices
{
    public partial class Cell : UserControl, IDisposable
    {
        public delegate Task TapDelegate(sbyte cellId);
        public event TapDelegate SelectedEvent;
        private CellData _cellData;

        public CellData Data 
        {
            get { return _cellData; }
            set
            {
                _cellData = value;
                Data.StateChange += SetState;
                Data.BallColorChange += SetBallColor;
                Data.IdChange += Data_IdChange;
                Data.NumberChange += Data_NumberChange;

                SetState(_cellData.State);
                SetBallColor(_cellData.BallColor);
                Data_NumberChange(_cellData.Number);
            }
        }

        public void Dispose()
        {
            this.Data.StateChange -= SetState;
            this.Data.BallColorChange -= SetBallColor;
            this.Data.IdChange -= Data_IdChange;
            this.Data.NumberChange -= Data_NumberChange;

            _cellData = null;
            this.CellLayoutRoot.Children.Clear();
        }

        public Cell PreviousCellInPath { get; set; }

        //public Cell()
        //    : this(null)
        //{   
        //}

        public Cell(Position position, bool transparentBackground = false, bool border = true, bool useNumber=false)
        {
            InitializeComponent();

            Data = new CellData();

            this.Data.Position = position;
            Data.State = AppServices.State.Empty;
            Data.BallColor = BallColor.None;
            this.Width = AppConfig.CELL_WIDTH;
            this.Height = AppConfig.CELL_HEIGHT;
            ball.RadiusX = AppConfig.BALL_WIDTH;
            ball.RadiusY = AppConfig.BALL_HEIGHT;

            translate.X = AxisHelper.BallCenterPoint.X;
            translate.Y = AxisHelper.BallCenterPoint.Y;

            animationToRight.KeyTime = KeyTime.FromTimeSpan(AppConfig.MOVING_ANIMATE_TIME);
            animationToLeft.KeyTime = KeyTime.FromTimeSpan(AppConfig.MOVING_ANIMATE_TIME);
            animationToTop.KeyTime = KeyTime.FromTimeSpan(AppConfig.MOVING_ANIMATE_TIME);
            animationToBottom.KeyTime = KeyTime.FromTimeSpan(AppConfig.MOVING_ANIMATE_TIME);

            if (transparentBackground)
                CellLayoutRoot.Background = new SolidColorBrush(Colors.Transparent);

            if (!border) brdCell.BorderThickness = new Thickness(0, 0, 0, 0);

            Data.StateChange += SetState;
            Data.BallColorChange += SetBallColor;
            Data.IdChange += Data_IdChange;
            if (useNumber)
            {
                Data.NumberChange += Data_NumberChange;
                txtNumber.Visibility = System.Windows.Visibility.Visible;
            }
            else
                txtNumber.Visibility = System.Windows.Visibility.Collapsed;
        }

        //public override bool Equals(object obj)
        //{   
        //    var comparedCell = obj as Cell;
        //    //if (comparedCell == null || comparedCell.Data == null) return false;

        //    if (comparedCell == null || comparedCell.Data == null)
        //    {
        //        int i = 1;
        //    }

        //    return this.Data.Position.Equals(comparedCell.Data.Position);
        //}

        public bool IsEqual(Cell comparedCell)
        {
            if (comparedCell == null || comparedCell.Data == null) return false;
            
            return this.Data.Position.Equals(comparedCell.Data.Position);
        }

        //public override int GetHashCode()
        //{
        //    return this.Data.Position.GetHashCode();
        //}

        private void Data_IdChange(sbyte id)
        {
            this.txtId.Text = id.ToString();
        }

        void Data_NumberChange(byte number)
        {
            if (number > 0)
            {
                this.txtNumber.Visibility = System.Windows.Visibility.Visible;
                this.txtNumber.Text = number.ToString();
            }
            else
                this.txtNumber.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void SetState(State state)
        {
            pBall.Opacity = 1;
            switch (state)
            {
                case AppServices.State.Empty:
                    grdBall.Visibility = System.Windows.Visibility.Collapsed;
                    Data.BallColor = BallColor.None;
                    break;
                case AppServices.State.Filled:
                    ball.RadiusX = AppConfig.BALL_WIDTH;
                    ball.RadiusY = AppConfig.BALL_HEIGHT;
                    translate.X = AxisHelper.BallCenterPoint.X;
                    translate.Y = AxisHelper.BallCenterPoint.Y;
                    scale.CenterX = 1;
                    scale.CenterY = 1;
                    grdBall.Visibility = System.Windows.Visibility.Visible;
                    break;
                case AppServices.State.Proposed:
                    ball.RadiusX = AppConfig.BALL_WIDTH / 2 ;
                    ball.RadiusY = AppConfig.BALL_HEIGHT / 2;
                    translate.X = AxisHelper.BallCenterPoint.X;
                    translate.Y = AxisHelper.BallCenterPoint.Y;
                    grdBall.Visibility = System.Windows.Visibility.Visible;
                    break;
                case AppServices.State.Selected:
                    grdBall.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        public async Task SetBounce(bool bounce)
        {
            if (!bounce)
            {
                sbBallBouncing.Stop();
                translate.Y = AxisHelper.BallCenterPoint.Y;
                translate.X = AxisHelper.BallCenterPoint.X;
            }
            else
            {
                translate.Y = AxisHelper.BallTopPoint.Y;
                translate.X = AxisHelper.BallCenterPoint.X;
                await sbBallBouncing.BeginAsync();
            }
        }

        public async Task GrowUp()
        {
            if (this.Data.State == State.Proposed)
                await sbGrowUp.BeginAsync();

            this.Data.State = State.Filled;
        }

        public async Task ShowScore(byte score, bool highestScore = false)
        {
            Canvas.SetZIndex(this, 100);
            imgHighestScore.Visibility = highestScore ? Visibility.Visible : Visibility.Collapsed;
            txtScore.Visibility = Visibility.Visible;
            txtScore.Text = "+" + score.ToString();
            await sbShowScore.BeginAsync();
            txtScore.Visibility = Visibility.Collapsed;
            imgHighestScore.Visibility = Visibility.Collapsed;
            Canvas.SetZIndex(this, 0);
        }

        private void SetBallColor(BallColor ballColor)
        {
            pBall.Visibility = System.Windows.Visibility.Visible;
            switch (ballColor)
            {
                case AppServices.BallColor.None:
                    pBall.Visibility = System.Windows.Visibility.Collapsed;
                    txtNumber.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case AppServices.BallColor.SteelBlue:
                    pBallColor.Color = Davang.Utilities.Helpers.Colors.DeepSkyBlue;
                    pBallColorOffset1.Color = Davang.Utilities.Helpers.Colors.SteelBlue;
                    break;
                case AppServices.BallColor.PaleVioletRed:
                    pBallColor.Color = Davang.Utilities.Helpers.Colors.Plum;
                    pBallColorOffset1.Color = Davang.Utilities.Helpers.Colors.PaleVioletRed;
                    break;
                case AppServices.BallColor.OliveDrab:
                    pBallColor.Color = Davang.Utilities.Helpers.Colors.DarkGray;
                    pBallColorOffset1.Color = Davang.Utilities.Helpers.Colors.DimGray;

                    break;
                case AppServices.BallColor.LightSeaGreen:
                    pBallColor.Color = Davang.Utilities.Helpers.Colors.LightBlue;
                    pBallColorOffset1.Color = Davang.Utilities.Helpers.Colors.LightSeaGreen;
                    break;
                case AppServices.BallColor.IndianRed:
                    pBallColor.Color = Davang.Utilities.Helpers.Colors.LightCoral;
                    pBallColorOffset1.Color = Davang.Utilities.Helpers.Colors.IndianRed;
                    break;
                case AppServices.BallColor.DarkGoldenrod:
                    pBallColor.Color = Davang.Utilities.Helpers.Colors.BurlyWood;
                    pBallColorOffset1.Color = Davang.Utilities.Helpers.Colors.DarkGoldenrod;
                    break;
            }
        }

        #region BallMoving

        public async Task MoveRight(CellData data, bool fromCenter = false, bool toCenter = false)
        {
            translate.X = fromCenter ? AxisHelper.BallCenterPoint.X : AxisHelper.BallLeftPoint.X;
            translate.Y = fromCenter ? AxisHelper.BallCenterPoint.Y : AxisHelper.BallLeftPoint.Y;
            animationToRight.Value = toCenter ? AxisHelper.BallCenterPoint.X : AxisHelper.BallRightPoint.X;

            await Move(toCenter ? sbStopRight : sbLeftRight, data);
        }

        public async Task MoveLeft(CellData data, bool fromCenter = false, bool toCenter = false)
        {
            translate.X = fromCenter ? AxisHelper.BallCenterPoint.X : AxisHelper.BallRightPoint.X;
            translate.Y = fromCenter ? AxisHelper.BallCenterPoint.Y : AxisHelper.BallRightPoint.Y;
            animationToLeft.Value = toCenter ? AxisHelper.BallCenterPoint.X : AxisHelper.BallLeftPoint.X;

            await Move(toCenter ? sbStopLeft : sbRightLeft, data);
        }

        public async Task MoveDown(CellData data, bool fromCenter = false, bool toCenter = false)
        {
            translate.X = fromCenter ? AxisHelper.BallCenterPoint.X : AxisHelper.BallTopPoint.X;
            translate.Y = fromCenter ? AxisHelper.BallCenterPoint.Y : AxisHelper.BallTopPoint.Y;
            animationToBottom.Value = toCenter ? AxisHelper.BallCenterPoint.Y : AxisHelper.BallBottomPoint.Y;

            await Move(toCenter ? sbStopDown : sbTopBottom, data);
        }

        public async Task MoveUp(CellData data, bool fromCenter = false, bool toCenter = false)
        {
            translate.X = fromCenter ? AxisHelper.BallCenterPoint.X : AxisHelper.BallBottomPoint.X;
            translate.Y = fromCenter ? AxisHelper.BallCenterPoint.Y : AxisHelper.BallBottomPoint.Y;
            animationToTop.Value = toCenter ? AxisHelper.BallCenterPoint.Y : AxisHelper.BallTopPoint.Y;

            await Move(toCenter ? sbStopUp : sbBottomTop, data);
        }

        private async Task Move(Storyboard storyboard, CellData data)
        {
            var currentData = this.Data.Clone();

            this.Data.BallColor = data.BallColor;
            this.Data.Number = data.Number;
            this.Data.State = State.Filled;
            await storyboard.BeginAsync();

            this.Data.State = currentData.State;
            this.Data.BallColor = currentData.BallColor;
            this.Data.Number = currentData.Number;
        }

        //private async Task Move(Position startPosition, EasingDoubleKeyFrame animation, double value)
        //{ 
        //    translate.X = startPosition.X;
        //    translate.Y = startPosition.Y;
        //    animation.Value = value;
        //    var currentState = this.State;

        //    State = Models.State.Filled;
        //    await sbBottomTop.BeginAsync(null);

        //    this.State = currentState;
        //}

        public async Task MoveFrom(Cell previousCell, CellData data, bool fromCenter = false, bool toCenter = false)
        {
            var neighbourPosition = this.Data.Position.IsNeighbourPosition(previousCell.Data.Position);

            switch (neighbourPosition)
            {
                case NeighbourPostion.Left:
                    await MoveRight(data, fromCenter, toCenter);
                    break;
                case NeighbourPostion.Up:
                    await MoveDown(data, fromCenter, toCenter);
                    break;
                case NeighbourPostion.Right:
                    await MoveLeft(data, fromCenter, toCenter);
                    break;
                case NeighbourPostion.Down:
                    await MoveUp(data, fromCenter, toCenter);
                    break;
            }
        }

        public async Task MoveTo(Cell nextCell, CellData data, bool fromCenter = false, bool toCenter = false)
        {
            var nextPosition = this.Data.Position.IsNeighbourPosition(nextCell.Data.Position);
            switch (nextPosition)
            {
                case NeighbourPostion.Left:
                    await MoveLeft(data, fromCenter, toCenter);
                    break;
                case NeighbourPostion.Up:
                    await MoveUp(data, fromCenter, toCenter);
                    break;
                case NeighbourPostion.Right:
                    await MoveRight(data, fromCenter, toCenter);
                    break;
                case NeighbourPostion.Down:
                    await MoveDown(data, fromCenter, toCenter);
                    break;
            }
        }

        public async Task Disappear()
        {
            txtNumber.Visibility = System.Windows.Visibility.Collapsed;
            await sbDisappear.BeginAsync();
        }

        #endregion

        #region Event

        private void CellLayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            switch (Data.State)
            {
                case AppServices.State.Filled:
                    Data.State = AppServices.State.Selected;
                    break;
                case AppServices.State.Selected:
                    Data.State = AppServices.State.Filled;
                    break;
                case AppServices.State.Empty:
                case AppServices.State.Proposed:
                    break;
            }

            if (SelectedEvent != null)
                SelectedEvent(this.Data.Id);
        }

        #endregion
    }
}
