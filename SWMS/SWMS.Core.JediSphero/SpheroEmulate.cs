using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Sphero.Communication;
using Sphero.Devices;
using Timer = System.Timers.Timer;

namespace SWMS.Core.JediSphero
{
    //public sealed class JediSphero 
    public sealed class JediSphero : SpheroDevice
    {
        //private void Roll(int spheroAngle, int spheroSpeed){}
        //private void SetBackLED(int p){}
        //private void SetRGBLED(int p1, int p2, int p3){}
        //private void SetHeading(int configAngle){}

        //public JediSphero(SpheroConnection connection)
        public JediSphero(SpheroConnection connection): base(connection)
        {
            double frequency = 10;
            _msDelay = (int)(1000.0 / frequency);

            _connection = connection;
            _currentX = 0.0;
            _currentY = 0.0;

            _speedScale = 0.5;
            _spheroSpeed = 255.0 * _speedScale;
            _timer = new Timer(_msDelay);
        }
        
        public void StartMove()
        {
            Point initPosition = GetCurrentPosition();
            SetIterationPoint(initPosition);
            SetDestinationPosition(initPosition);

            Debug.WriteLine(_iterationDistance);

            _timer.Elapsed += (sender, args) =>
            {
                Point currentPosition = _nextIterationPoint;
                SetCurrentPosition(currentPosition);
                Point destinationPosition = GetDestinationPosition();
                double spheroAngle = GetSpheroAngle(currentPosition, destinationPosition);
                double realAngle = GetAngle(currentPosition, destinationPosition);
                double realDistance = GetDistance(currentPosition, destinationPosition);

                Debug.WriteLine("Real distance {0}", realDistance);
                
                if (realDistance >= _iterationDistance)
                {
                    Debug.WriteLine("In move");
                    Roll((int)spheroAngle, (int)_spheroSpeed);
                    var nextPoint = GetNextPoint(currentPosition, realAngle, _iterationDistance);
                    SetIterationPoint(nextPoint);
                }
                else
                {
                    Debug.WriteLine("Not move");
                    Roll((int)spheroAngle, 0);
                }
            };

            _timer.Start();
        }

        public void StopMove()
        {
            if (_timer != null)
            {
                _timer.Stop();
                Roll(0, 0);
                SetCurrentPosition(_nextIterationPoint);
            }
        }

        public bool IsInitialized { get; private set; }

        public string Name
        {
            get { return _connection.BluetoothName; }
        }

        public double CurrentX
        {
            get { return CurrentPoint.X; }
        }

        public double CurrentY
        {
            get { return CurrentPoint.Y; }
        }

        public Point CurrentPoint
        {
            get { return GetCurrentPosition(); }
        }

        public void BeginConfiguration()
        {
            StopMove();
            IsInitialized = false;
            SetRGBLED(255, 0, 0);
            SetBackLED(255);
            _currentX = 0.0;
            _currentY = 0.0;

            _speedScale = 0.5;
            _spheroSpeed = 255.0 * _speedScale;
            _realSpeed = MAX_SPEED * _speedScale;
            _iterationDistance = _realSpeed * (_msDelay / 1000.0);
        }

        

        public void SetSpeedScale(double scale)
        {
            _speedScale = scale;
            _spheroSpeed = 255.0 * _speedScale;
        }

        public void SetPosition(double x, double y)
        {
            SetPosition(new Point(x, y));
        }

        public void SetPosition(Point point)
        {
            _timer.Stop();
            SetCurrentPosition(point);
            _nextIterationPoint = point;
            SetDestinationPosition(point);
            _timer.Start();
        }

        public void SetConfigurationAngle(int angle)
        {
            _configAngle = angle % 360;
            Roll(angle, 0);
        }

        public void EndConfiguration()
        {
            SetRGBLED(0, 127, 0);
            SetBackLED(0);
            SetHeading(_configAngle);
            IsInitialized = true;
            StartMove();
        }

        public void MoveTo(double x, double y)
        {
            MoveTo(new Point(x, y));
        }

        public void MoveTo(Point point)
        {
            SetDestinationPosition(point);
        }

        public Point GetNextPoint()
        {
            return _nextIterationPoint;
        }

        public Point GetDestinationPosition()
        {
            var x = Interlocked.Exchange(ref _destinationX, _destinationX);
            var y = Interlocked.Exchange(ref _destinationY, _destinationY);
            return new Point(x, y);
        }

        public Point GetCurrentPosition()
        {
            var x = Interlocked.Exchange(ref _currentX, _currentX);
            var y = Interlocked.Exchange(ref _currentY, _currentY);
            return new Point(x, y);
        }

        public void Disconnect()
        {
            _connection.Disconnect();
        }

        private Point GetNextPoint(Point a, double angle, double distance)
        {
            var dy = Math.Cos((angle * Math.PI / 180)) * distance;
            var dx = Math.Sin((angle * Math.PI / 180)) * distance;

            var newX = a.X + dx;
            var newY = a.Y + dy;
            return new Point(newX, newY);
        }

        private void SetDestinationPosition(Point point)
        {
            Interlocked.Exchange(ref _destinationX, point.X);
            Interlocked.Exchange(ref _destinationY, point.Y);
        }

        private double GetDistance(Point a, Point b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance;
        }

        private double GetSpheroAngle(Point a, Point b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;

            var angle = Math.Atan2(dx, dy) * (180 / Math.PI);
            angle = angle < 0 ? angle + 360 : angle;
            return angle;
        }

        private static double GetAngle(Point a, Point b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;

            var angle = Math.Atan2(dx, dy) * (180 / Math.PI);
            return angle;
        }

        private void SetCurrentPosition(Point point)
        {
            Interlocked.Exchange(ref _currentX, point.X);
            Interlocked.Exchange(ref _currentY, point.Y);
        }

        private readonly SpheroConnection _connection;

        private void SetIterationPoint(Point point)
        {
            _nextIterationPoint = point;
        }

        private Timer _timer;
        private Point _nextIterationPoint;
        private double _iterationDistance;
        private double _realSpeed;

        private double _currentX;
        private double _currentY;

        private double _destinationX;
        private double _destinationY;

        private int _configAngle;

        private double _speedScale;
        private double _spheroSpeed;

        private const double MAX_SPEED = 2.0;
        private readonly int _msDelay;
    }
}