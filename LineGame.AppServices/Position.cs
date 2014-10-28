using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineGame.AppServices;

namespace LineGame.AppServices
{
    public class Position
    {
        public sbyte X { get; set; }
        public sbyte Y { get; set; }

        public Position(sbyte x, sbyte y)
        {
            X = x;
            Y = y;
        }

        public Position()
        {

        }

        public bool IsEqual(Position comparedPosition)
        {
            if (comparedPosition == null) return false;

            return this.X == comparedPosition.X
                && this.Y == comparedPosition.Y;
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null) return false;
        //    var comparedPosition = obj as Position;

        //    return this.X == comparedPosition.X
        //        && this.Y == comparedPosition.Y;
        //}

        //public override int GetHashCode()
        //{
        //    return X.GetHashCode() + Y.GetHashCode();
        //}

        public NeighbourPostion IsNeighbourPosition(Position position)
        {
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.Left))) return NeighbourPostion.Left;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.Up))) return NeighbourPostion.Up;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.Right))) return NeighbourPostion.Right;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.Down))) return NeighbourPostion.Down;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.UpperLeft))) return NeighbourPostion.UpperLeft;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.UpperRight))) return NeighbourPostion.UpperRight;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.BottomLeft))) return NeighbourPostion.BottomLeft;
            if (position.IsEqual(GetNeighbourPosition(NeighbourPostion.BottomRight))) return NeighbourPostion.BottomRight;
            
            return NeighbourPostion.None;
        }

        public Position GetNeighbourPosition(NeighbourPostion neighbour)
        {
            Position position = null;

            switch(neighbour)
            {
                case NeighbourPostion.Left:
                    if (X > 0)
                        position = new Position((sbyte)(X - 1), Y);
                    break;
                case NeighbourPostion.Up:
                     if (Y > 0)
                        position = new Position(X, (sbyte)(Y - 1));
                    break;
                case NeighbourPostion.Right:
                    if (X < AppConfig.COLUMN_COUNT - 1)
                        position = new Position((sbyte)(X + 1), Y);
                    break;
                case NeighbourPostion.Down:
                    if (Y < AppConfig.ROW_COUNT - 1)
                        position = new Position(X, (sbyte)(Y + 1));
                    break;
                case NeighbourPostion.UpperLeft:
                    if (X > 0 && Y > 0)
                        position = new Position((sbyte)(X - 1), (sbyte)(Y - 1));
                    break;
                case NeighbourPostion.UpperRight:
                    if (X < AppConfig.COLUMN_COUNT - 1 && Y > 0)
                        position = new Position((sbyte)(X + 1), (sbyte)(Y - 1));
                    break;
                case NeighbourPostion.BottomLeft:
                    if (X > 0 && Y < AppConfig.ROW_COUNT - 1)
                        position = new Position((sbyte)(X - 1), (sbyte)(Y + 1));
                    break;
                case NeighbourPostion.BottomRight:
                    if (X < AppConfig.COLUMN_COUNT - 1 && Y < AppConfig.ROW_COUNT - 1)
                        position = new Position((sbyte)(X + 1), (sbyte)(Y + 1));
                    break;
            }

            return position;
        }
    }
}
