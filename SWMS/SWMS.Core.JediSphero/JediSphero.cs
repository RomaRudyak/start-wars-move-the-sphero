using System;
using System.Diagnostics;
using Sphero.Communication;
using Sphero.Devices;
using Timer = System.Timers.Timer;

namespace SWMS.Core
{
    public class JediSphero : SpheroDevice
    {
        public JediSphero(SpheroConnection connection)
            : base(connection)
        {
            _connection = connection;
        }

        public string Name
        {
            get
            {
                return _connection.BluetoothName;
            }
        }
        public double Scale { get; private set; }
        public bool IsMoving
        {
            get { return _isMoving; }
            private set { _isMoving = value; }
        }
        public double CurrentX
        {
            get { return _currentX; }
            private set
            {
                if (value != _currentX)
                {
                    _currentX = value;
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
                }
            }
        }

        public bool IsInitialized { get; private set; }

        #region Configuration

        public void BeginConfigure()
        {
            SetRGBLED(255, 0, 0);
            SetBackLED(255);
        }

        public void SetConfigurePosition(double x, double y)
        {
            CurrentX = x;
            CurrentY = y;
        }

        public void SetConfigureAngle(int angle)
        {
            Debug.WriteLine(_configAngle);
            _configAngle = angle % 360;
            base.Roll(angle, 0);
        }

        public void SetSpeedScale(double speedScale)
        {
            Debug.WriteLine("Speed scale : {0}", speedScale);
            Scale = speedScale;
        }

        public void EndConfigure()
        {
            SetHeading(_configAngle);
            SetRGBLED(0, 127, 0);
            SetBackLED(0);
            IsInitialized = true;
        }

        #endregion

        public void MoveTo(double x, double y)
        {
            if (IsMoving)
            {
                return;
            }

            var distance = GetDistance(x, y);
            var angle = GetAngle(x, y);

            var realSpeed = MAX_SPEED * Scale;
            var time = (distance / realSpeed) * 1000;

            var str = ""
                      + string.Format("time(ms): {0}", time)
                      + string.Format("\nangle: {0}", angle)
                      + string.Format("\ndistance: {0}", distance)
                      + string.Format("\nspeed: {0}", realSpeed)
                      + string.Format("\nbefore: x: {0}  y: {1}", CurrentX, CurrentY)
                      + string.Format("\ndestination: x:{0}, y:{1}", x, y)
                      + string.Format("\nstart moving");

            Debug.WriteLine(str);

            var spheroSpeed = GetSpheroSpeed();
            SetTempDeltaXY(x, y);
            Roll(angle, spheroSpeed, time);
        }

        public void Roll(double angle, double speed, double miliseconds)
        {
            if (IsMoving)
            {
                return;
            }

            if (miliseconds == 0)
            {
                return;
            }


            var timer = new Timer(miliseconds);

            timer.Elapsed += (sender, args) =>
            {
                base.Roll((int)angle, 0);
                UpdateXY();
                IsMoving = false;
                Debug.WriteLine("\nstop moving\n-------------");
                timer.Stop();
            };

            IsMoving = true;
            timer.Start();
            base.Roll((int)angle, (int)(speed));
        }

        private double GetDistance(double x, double y)
        {
            var dx = x - CurrentX;
            var dy = y - CurrentY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance;
        }

        private double GetAngle(double x, double y)
        {
            var dx = x - CurrentX;
            var dy = y - CurrentY;
            var angle = Math.Atan2(dx, dy) * (180 / Math.PI);
            angle = angle < 0 ? angle + 360 : angle;
            return angle;
        }

        private double GetSpheroSpeed()
        {
            return 255.0 * Scale;
        }
        
        private void SetTempDeltaXY(double x, double y)
        {
            _tempdX = x - CurrentX;
            _tempdY = y - CurrentY;
        }

        private void UpdateXY()
        {
            CurrentX += _tempdX;
            CurrentY += _tempdY;
        }

        private const double MAX_SPEED = 2.0;

        private readonly SpheroConnection _connection;
        private int _configAngle = 0;
        private double _currentX = 0;
        private double _currentY = 0;
        private volatile bool _isMoving;

        private double _tempdX = 0.0;
        private double _tempdY = 0.0;
    }
}