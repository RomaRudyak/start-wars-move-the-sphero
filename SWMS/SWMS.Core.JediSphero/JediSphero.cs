using System;
using System.Threading;
using System.Timers;
using System.Windows;
using Sphero.Communication;
using Sphero.Devices;
using Timer = System.Timers.Timer;

namespace SWMS.Core.JediSphero
{
    public sealed class JediSphero : SpheroDevice
    {
        public JediSphero(SpheroConnection connection)
            : base(connection)
        {
            double frequency = 15.0;
            _msDelay = (int)(1000.0 / frequency);

            _connection = connection;
            _currentX = 0.0;
            _currentY = 0.0;

            _speedScale = 0.5;
            _spheroSpeed = 255.0 * _speedScale;
            _timer = new Timer(_msDelay);
            _timer.Elapsed += IterationHandler;
            _timer.Start();
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

        public void StopMove()
        {
            var currentPosition = _nextIterationPoint;
            SetCurrentPosition(currentPosition);
            SetDestinationPosition(currentPosition);
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
            SetNextIterationPosition(point);
            SetCurrentPosition(point);
            SetDestinationPosition(point);
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
        }

        public void MoveTo(double x, double y)
        {
            MoveTo(new Point(x, y));
        }

        public void MoveTo(Point point)
        {
            SetDestinationPosition(point);
        }

        public void Disconnect()
        {
            _timer.Stop();
            _connection.Disconnect();
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

        private void IterationHandler(object sender, ElapsedEventArgs arhs)
        {
            if (!IsInitialized)
            {
                return;
            }

            Point currentPosition = _nextIterationPoint;
            SetCurrentPosition(currentPosition);
            Point destinationPosition = GetDestinationPosition();
            double spheroAngle = GetSpheroAngle(currentPosition, destinationPosition);
            double realDistance = GetDistance(currentPosition, destinationPosition);

            if (realDistance >= _iterationDistance)
            {
                _lastAngle = (int)spheroAngle;
                Roll(_lastAngle, (int)_spheroSpeed);
                double realAngle = GetAngle(currentPosition, destinationPosition);
                var nextPoint = CalculateNextIterationPosition(currentPosition, realAngle, _iterationDistance);
                SetNextIterationPosition(nextPoint);
            }
            else
            {
                Roll(_lastAngle, 0);
            }
        }

        private void SetCurrentPosition(Point point)
        {
            Interlocked.Exchange(ref _currentX, point.X);
            Interlocked.Exchange(ref _currentY, point.Y);
        }

        private void SetNextIterationPosition(Point point)
        {
            _nextIterationPoint = point;
        }

        private Point CalculateNextIterationPosition(Point a, double angle, double distance)
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

        private readonly SpheroConnection _connection;

        private Timer _timer;
        private Point _nextIterationPoint;
        private double _iterationDistance;
        private volatile int _lastAngle;
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