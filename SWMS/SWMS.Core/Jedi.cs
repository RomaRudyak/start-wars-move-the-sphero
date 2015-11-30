using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


            _body = bodys.FirstOrDefault();
            if (_body == null)
            {
                return;
            }

            ProcessBody();
        }

        private void ProcessBody()
        {
            
        }

        private Body _body;
    }
}
