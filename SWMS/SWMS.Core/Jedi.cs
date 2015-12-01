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
            Body[] bodys = null;
            var mFrame = e.FrameReference.AcquireFrame();

            using (var bodyframe = mFrame.BodyFrameReference.AcquireFrame())
            {
                if (bodyframe == null)
                {
                    return;
                }

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
            } 
            else if (IsHandInInitialGesture(body.HandLeftState, body.HandLeftConfidence))
            {
                newPoint = handLeft.Position;
            }
            else
            {
                _handTrackedCount = 0;
                _headTrackedCount = 0;
                return;
            }

            _lastHeadPosition = _lastHeadPosition.GetAccumulatedAvarage(head.Position, _headTrackedCount);
            _lastHandPosition = _lastHandPosition.GetAccumulatedAvarage(newPoint, _handTrackedCount);

            _handTrackedCount++;
            _headTrackedCount++;

            Debug.WriteLine("Hand tracked: {0}", _handTrackedCount);

            if (_handTrackedCount == 15)
            {
                var handPosition = _lastHandPosition;
                var headPosition = _lastHeadPosition;

                var x = CoordinateHelper.FindPointProection(headPosition.GetProectionForXZ(), handPosition.GetProectionForXZ());
                var y = CoordinateHelper.FindPointProection(headPosition.GetProectionForZY(), handPosition.GetProectionForZY());

                var point = new PointF { X = x, Y = y };
                ForceActivated.SafeRise(this, point);
                Debug.WriteLine("Point: x={0} y={1}", point.X, point.Y);
                _handTrackedCount=0;
                _headTrackedCount=0;
            }

            
        }

        private Boolean IsHandInInitialGesture(HandState handState, TrackingConfidence trackingCondition)
        {
            return handState == InitialHandState && trackingCondition == TrackingConfidence.High;
        }


        private CameraSpacePoint _lastHandPosition;
        private CameraSpacePoint _lastHeadPosition;
        private Int32 _handTrackedCount = 0;
        private Int32 _headTrackedCount = 0;
        private Tuple<JointType, DateTime, CameraSpacePoint> _initializationContext;
        private static readonly TimeSpan InitializationFazeSpan = TimeSpan.FromSeconds(2d);
        private static readonly HandState InitialHandState = HandState.Lasso;
    }
}
