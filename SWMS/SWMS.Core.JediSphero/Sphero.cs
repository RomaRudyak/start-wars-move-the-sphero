using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Sphero.Communication;
using Sphero.Devices;

namespace SWMS.Core
{
    public class Sphero : SpheroDevice, INotifyPropertyChanged
    {
        public Sphero(SpheroConnection connection)
            : base(connection)
        {
            _connection = connection;
            IsInitialized = false;
            Scale = 0.5;
            Friquency = 7.5;
        }

        private readonly SpheroConnection _connection;
        private int _configAngle = 0;

        private double _currentX = 0;
        private double _currentY = 0;

        public double Friquency { get; private set; }
        public double Scale { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsConnected { get; private set; }
        public double CurrentX
        {
            get { return _currentX; }
            private set
            {
                if (value != _currentX)
                {
                    _currentX = value;
                    OnPropertyChanged();
                }
            }
        }
        public double CurrentY
        {
            get { return _currentY; }
            private set
            {
                if (value != _currentY)
                {
                    _currentY = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (IsConnected)
            {
                throw new Exception("Sphero already connected");
            }
            else
            {
                IsConnected = await _connection.Connect();
                return IsConnected;
            }
        }

        public bool Disconnect()
        {
            if (!IsConnected)
            {
                throw new Exception("Sphero already disconnected");
            }
            else
            {
                IsConnected = false;
                return !IsConnected;
            }
        }

        #region Configuration

        public void SetFriquency(double friquency)
        {
            Friquency = friquency;
        }

        public void ChangeSpeedScale(double scale)
        {
            Scale = scale;
        }

        public void BeginConfigure()
        {
            IsInitialized = false;
            SetBackLED(255);
        }

        public void SetConfigurePosition(double x, double y)
        {
            if (IsInitialized)
            {
                return;
            }

            CurrentX = x;
            CurrentY = y;
        }

        public void SetConfigureAngle(int angle)
        {
            if (IsInitialized)
            {
                return;
            }

            Debug.WriteLine(_configAngle);
            _configAngle = angle % 360;
            base.Roll(angle, 0);
        }

        public void EndConfigure()
        {
            if (IsInitialized)
            {
                return;
            }

            SetHeading(_configAngle);
            SetBackLED(0);
            IsInitialized = true;
        }

        #endregion

        //internal roll
        private void Roll(double angle, double speed)
        {
            if (IsInitialized)
            {
                base.Roll((int)angle, (int)(speed));
            }
        }

        //default Roll
        public new void Roll(int angle, int velosity)
        {
            if (IsInitialized)
            {
                base.Roll(angle, 128);
                Thread.Sleep(10);
                base.Roll(angle, 0);
            }
        }

        public void MoveTo(double x, double y)
        {
            var dx = x - CurrentX;
            var dy = y - CurrentY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            double realSpeed = Scale * 255.0;
            var iterations = (distance / realSpeed) * 1000;

            var angle = Math.Atan2(dx, dy) * (180 / Math.PI);
            angle = angle < 0 ? angle + 360 : angle;

            var dxPerIteration = dx / iterations;
            var dyPerIteration = dy / iterations;


            for (var i = 0; i < iterations - 1; i++)
            {
                Roll(angle, realSpeed);
                CurrentX += dxPerIteration;
                CurrentY += dyPerIteration;
            }

            Roll(angle, realSpeed / 2);
            CurrentX += dxPerIteration;
            CurrentY += dyPerIteration;

            Roll(angle, 0.0);

            var finish = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
