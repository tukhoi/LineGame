using LineGame.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices
{
    public class AxisHelper
    {
        #region BallPosition

        public static Position BallCenterPoint
        {
            get 
            {
                return new Position()
                {
                    X = (sbyte)(AppConfig.CELL_WIDTH / 2),
                    Y = (sbyte)(AppConfig.CELL_HEIGHT / 2)
                };
            }
        }

        public static Position BallLeftPoint
        {
            get 
            {
                return new Position()
                {
                    X = AppConfig.BALL_WIDTH,
                    Y = (sbyte)(AppConfig.CELL_HEIGHT / 2)
                };
            }
        }

        public static Position BallRightPoint
        {
            get 
            {
                return new Position()
                {
                    X = (sbyte)(AppConfig.CELL_WIDTH - AppConfig.BALL_WIDTH),
                    Y = (sbyte)(AppConfig.CELL_HEIGHT / 2)
                };
            }
        }

        public static Position BallTopPoint
        {
            get
            {
                return new Position()
                {
                    X = (sbyte)(AppConfig.CELL_WIDTH / 2),
                    Y = AppConfig.BALL_HEIGHT
                };
            }
        }

        public static Position BallBottomPoint
        {
            get 
            {
                return new Position()
                {
                    X = (sbyte)(AppConfig.CELL_WIDTH / 2),
                    Y = (sbyte)(AppConfig.CELL_HEIGHT - AppConfig.BALL_HEIGHT)
                };
            }
        }

        #endregion

        public static sbyte ConvertToLinear(Position position)
        {
            if (position.Y > 0)
                return (sbyte)((position.Y) * AppConfig.COLUMN_COUNT + position.X);
            else
                return position.X;
        }

        public static Position ConvertToXY(sbyte index)
        {
            var x = (sbyte)(index / AppConfig.COLUMN_COUNT);
            var y = (sbyte)(index % AppConfig.ROW_COUNT);

            return new Position
            {
                X = x,
                Y = y
            };
        }
    }
}
