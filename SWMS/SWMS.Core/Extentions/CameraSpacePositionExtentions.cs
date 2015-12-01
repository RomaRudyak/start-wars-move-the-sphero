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

        public static PointF GetProectionForXY(this CameraSpacePoint point)
        {
            return new PointF { X = point.X, Y = point.Y };
        }

        public static CameraSpacePoint GetAccumulatedAvarage(this CameraSpacePoint lastAnchor, CameraSpacePoint newPosition, Int32 count)
        {
            var result = new CameraSpacePoint();

            result.X = lastAnchor.X * count / (count + 1) + newPosition.X * (1 / (count + 1));
            result.Y = lastAnchor.Y * count / (count + 1) + newPosition.Y * (1 / (count + 1));
            result.Z = lastAnchor.Z * count / (count + 1) + newPosition.Z * (1 / (count + 1));

            return result;
        }
    }
}
