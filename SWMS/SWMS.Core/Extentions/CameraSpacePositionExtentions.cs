using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Core.Extentions
{
    internal static class CameraSpacePositionExtentions
    {
        public static PointF GetProectionForZY(this CameraSpacePoint point)
        {
            return new PointF { X = point.Z, Y = point.Y };
        }

        public static PointF GetProectionForXZ(this CameraSpacePoint point)
        {
            return new PointF { X = point.X, Y = point.Z };
        }
    }
}
