using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWMS.Core.Extentions;
using SWMS.Core.Helpers;
using System.Diagnostics;
using System.Windows;

namespace SWMS.Core
{
    public class JediGestureRecognizer : IDisposable
    {
        /// <summary>
        /// Fires when Jedi moving something with force
        /// </summary>
        public event Action<Object, Point> ForceApplying;

        /// <summary>
        /// Fires when Jedi stop using the force
        /// </summary>
        public event Action<Object> ForceDispel;

        public void Dispose()
        {
            Dispose(true);
        }

        public JediGestureRecognizer(KinectSensor sensor)
        {
            _sensor = sensor;
            _reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Depth);
            _reader.MultiSourceFrameArrived += ProcessMove;
        }

        ~JediGestureRecognizer()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        
        protected virtual void Dispose(Boolean dispossing)
        {
            if (dispossing)
            {
                _reader.MultiSourceFrameArrived -= ProcessMove;
                _reader.Dispose();
                _reader = null;
            }
        }


        private void ProcessMove(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            unchecked { _frameCount++; }
            Body[] bodys = null;
            var mFrame = e.FrameReference.AcquireFrame();
            Vector4 kinectHeight;

            using (var bodyframe = mFrame.BodyFrameReference.AcquireFrame())
            {
                if (bodyframe == null)
                {
                    return;
                }
                kinectHeight = bodyframe.FloorClipPlane;
                bodys = new Body[bodyframe.BodyCount];
                bodyframe.GetAndRefreshBodyData(bodys);
            }

            var body = bodys.FirstOrDefault(b=>b.IsTracked);

            if (body == null)
            {
                return;
            }

            var head = body.Joints[JointType.Head];
            var handLeft = body.Joints[JointType.HandLeft];
            var handRight = body.Joints[JointType.HandRight];

            if (head.TrackingState == TrackingState.NotTracked ||
                handLeft.TrackingState == TrackingState.NotTracked ||
                handRight.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            TrackForeMove(body, head, handLeft, handRight);
        }

        private void TrackForeMove(Body body, Joint head, Joint handLeft, Joint handRight)
        {
            CameraSpacePoint newPoint = default(CameraSpacePoint);

            if (IsHandInInitialGesture(body.HandRightState))
            {
                newPoint = handRight.Position;
                Debug.WriteLine("Force hand: Right");
            }
            else if (IsHandInInitialGesture(body.HandLeftState))
            {
                newPoint = handLeft.Position;
                Debug.WriteLine("Force hand: Left");
            }

            if (newPoint != default(CameraSpacePoint))
            {
                var distance = CoordinateHelper.GetDistance(newPoint, _lastHandPosition);
                if (_handTrackedCount > 0 && distance > 0.15F)
                {
                    Debug.WriteLine("oldPoint: x={0} y={1} z={2}", _lastHandPosition.X, _lastHandPosition.Y, _lastHandPosition.Z);
                    Debug.WriteLine("newPoint: x={0} y={1} z={2}", newPoint.X, newPoint.Y, newPoint.Z);
                    ResetTrackingCounters(1);
                    Debug.WriteLine("Reseted in order ot distance: {0}", distance);
                }

                _lastHeadPosition = _lastHeadPosition.GetAccumulatedAvarage(head.Position, ref _headTrackedCount);
                _lastHandPosition = _lastHandPosition.GetAccumulatedAvarage(newPoint, ref _handTrackedCount);

                Debug.WriteLine("Forse tracked: {0} Frame: {1}", _handTrackedCount, _frameCount);
            }


            if (CanFireForesMove())
            {
                var handPosition = _lastHandPosition;
                var headPosition = _lastHeadPosition;

                var x = CoordinateHelper.FindPointProjection(headPosition.GetProjectionForXZ(), handPosition.GetProjectionForXZ());
                var headZY = headPosition.GetProjectionForZY();
                var y = CoordinateHelper.FindPointProjection(headZY, handPosition.GetProjectionForZY());

                var point = new Point { X = x, Y = y };
                ForceApplying.SafeRise(this, point);
                Debug.WriteLine("Force Point: x={0} y={1}", point.X, point.Y);
                ResetTrackingCounters();
            }

            if (CanFireForesDispel())
            {
                ForceDispel.SafeRise(this);
                ResetTrackingCounters();
            }
        }

        private Boolean CanFireForesDispel()
        {
            return _handTrackedCount != 0 && _handTrackedCount != _frameCount;
        }

        private Boolean CanFireForesMove()
        {
            return _frameCount % Frequency == 0 && _handTrackedCount == Frequency;
        }

        private void ResetTrackingCounters(int reset = 0)
        {
            _handTrackedCount = reset;
            _headTrackedCount = reset;
            _frameCount = 0;
        }

        private Boolean IsHandInInitialGesture(HandState handState)
        {
            return handState == InitialHandState;
        }

        private Int32 _frameCount;
        private CameraSpacePoint _lastHandPosition;
        private CameraSpacePoint _lastHeadPosition;
        private Int32 _handTrackedCount = 0;
        private Int32 _headTrackedCount = 0;
        private static readonly HandState InitialHandState = HandState.Open;
        private MultiSourceFrameReader _reader;
        private KinectSensor _sensor;
        private readonly int Frequency = 4;
    }
}
