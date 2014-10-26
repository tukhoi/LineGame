using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices
{
    public class CellData
    {
        public delegate void SetStateDelegate(State state);
        public delegate void SetBallColorDelegate(BallColor ballColor);
        public delegate void SetIdDelegate(sbyte id);
        public delegate void SetNumberDelegate(byte number);

        public event SetStateDelegate StateChange;
        public event SetBallColorDelegate BallColorChange;
        public event SetIdDelegate IdChange;
        public event SetNumberDelegate NumberChange;

        private State _state;
        private BallColor _ballColor;
        private sbyte _id;
        private byte _number;

        public CellData()
        {
            _state = AppServices.State.Empty;
            _ballColor = AppServices.BallColor.None;
        }
        
        public sbyte Id
        {
            get { return _id; }
            set
            {
                _id = value;
                if (IdChange != null)
                    IdChange(_id);
            }
        }

        public Position Position { get; set; }
        public State State
        {
            get { return _state; }
            set
            {
                _state = (State)value;
                if (StateChange != null)
                    StateChange(_state);
            }
        }

        public BallColor BallColor
        {
            get { return _ballColor; }
            set
            {
                _ballColor = value;
                if (BallColorChange != null)
                    BallColorChange(_ballColor);
            }
        }

        public byte Number
        {
            get { return _number; }
            set
            {
                _number = value;
                if (NumberChange != null)
                    NumberChange(_number);
            }
        }

        public CellData Clone()
        {
            return new CellData 
            {
                Id = this.Id,
                Position = this.Position,
                BallColor = this.BallColor,
                State = this.State,
                Number = this.Number
            };
        }
    }
}
