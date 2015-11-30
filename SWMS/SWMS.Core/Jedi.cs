using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWMS.Core.Extentions;
using SWMS.Core.Helpers;

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
            using (var bodyframe = e.FrameReference.AcquireFrame().BodyFrameReference.AcquireFrame())
            {
                if (bodyframe == null)
                {
                    return;
                }

                bodys = new Body[bodyframe.BodyCount];
                bodyframe.GetAndRefreshBodyData(bodys);
            }

            if (bodys == null)
            {
                return;
            }


            var body = bodys.FirstOrDefault();
            ProcessBody(body);
        }

        private void ProcessBody(Body body)
        {
            if (body == null || !body.IsTracked)
            {
                return;
            }

            var head = body.Joints[JointType.Head];
            var handLeft = body.Joints[JointType.HandLeft];
            var handRight = body.Joints[JointType.HandRight];

            if (head.TrackingState != TrackingState.Tracked ||
                handLeft.TrackingState != TrackingState.Tracked ||
                handRight.TrackingState != TrackingState.Tracked)
            {
                return;
            }


            if (_initializedContext != null)
            {
                if (body.HandRightState == HandState.Lasso)
                {
                    _initializedContext = new Tuple<JointType, DateTime>(JointType.HandRight, DateTime.UtcNow);
                }

                if (body.HandLeftState == HandState.Lasso)
                {
                    _initializedContext = new Tuple<JointType, DateTime>(JointType.HandLeft, DateTime.UtcNow);
                }
            }
            else if (body.HandRightState != HandState.Lasso || body.HandLeftState != HandState.Lasso)
            {
                _initializedContext = null;
            }

            if (_initializedContext.Item2 - DateTime.UtcNow >= InitializationFazeSpan)
            {
                var handPosition = body.Joints[_initializedContext.Item1].Position;

                var x = CoordinateHelper.FindPointProection(head.Position.GetProectionForXZ(), handPosition.GetProectionForXZ());
                var y = CoordinateHelper.FindPointProection(head.Position.GetProectionForZY(), handPosition.GetProectionForZY());

                ForceActivated.SafeRise(this, new PointF { X = x, Y = y});
                _initializedContext = null;
            }
            
        }

        private Tuple<JointType, DateTime> _initializedContext;
        private static readonly TimeSpan InitializationFazeSpan = TimeSpan.FromSeconds(2d);
    }
}
