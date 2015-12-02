using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWMS.Core.Extentions;
using SWMS.Core.Helpers;
using System.Diagnostics;

namespace SWMS.Core
{
    public class Jedi
    {
        /// <summary>
        /// Fires when Jedi activates force
        /// </summary>
        public event Action<Object, PointF> ForceActivated;

        /// <summary>
        /// Fires when Jedi moving something with force
        /// </summary>
        public event Action<Object, PointF> ForceApplying;

        /// <summary>
        /// Fires when Jedi stop using the force
        /// </summary>
        public event Action ForceDispel;

        public void ProcessMove(object sender, MultiSourceFrameArrivedEventArgs e)
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

            CameraSpacePoint newPoint;

            if (IsHandInInitialGesture(body.HandRightState, body.HandRightConfidence))
            {
                newPoint = handRight.Position;
                Debug.WriteLine("Right hand");
            } 
            else if (IsHandInInitialGesture(body.HandLeftState, body.HandLeftConfidence))
            {
                newPoint = handLeft.Position;
                Debug.WriteLine("Left hand");
            }
            else
            {
                //ResetTrackingCounters();
                return;
            }

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

            Debug.WriteLine("Hand tracked: {0} Frame: {1}", _handTrackedCount, _frameCount);

            
            
            if (_frameCount % 7 == 0 && _handTrackedCount >= 7)
            {
                var handPosition = _lastHandPosition;
                var headPosition = _lastHeadPosition;

                var x = CoordinateHelper.FindPointProection(headPosition.GetProectionForXZ(), handPosition.GetProectionForXZ());
                var headZY = headPosition.GetProectionForZY();
                var y = CoordinateHelper.FindPointProection(headZY, handPosition.GetProectionForZY());

                var point = new PointF { X = x, Y = y };
                ForceApplying.SafeRise(this, point);
                Debug.WriteLine("Point: x={0} y={1}", point.X, point.Y);
                ResetTrackingCounters();
            }

            
        }

        private void ResetTrackingCounters(int reset = 0)
        {
            _handTrackedCount = reset;
            _headTrackedCount = reset;
            _frameCount = 0;
        }

        private Boolean IsHandInInitialGesture(HandState handState, TrackingConfidence trackingCondition)
        {
            return handState == InitialHandState;// && trackingCondition == TrackingConfidence.High;
        }

        private Int32 _frameCount;
        private CameraSpacePoint _lastHandPosition;
        private CameraSpacePoint _lastHeadPosition;
        private Int32 _handTrackedCount = 0;
        private Int32 _headTrackedCount = 0;
        private Tuple<JointType, DateTime, CameraSpacePoint> _initializationContext;
        private static readonly TimeSpan InitializationFazeSpan = TimeSpan.FromSeconds(2d);
        private static readonly HandState InitialHandState = HandState.Open;
    }
}
