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
        /// <summary>
        /// For finding Z(Y) on the fllor
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static PointF GetProectionForZY(this CameraSpacePoint point)
        {
            return new PointF { X = point.Z, Y = point.Y };
        }

        /// <summary>
        /// For finding X on the floar
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static PointF GetProectionForXZ(this CameraSpacePoint point)
        {
            return new PointF { X = point.X, Y = point.Z };
        }

        public static CameraSpacePoint GetAccumulatedAvarage(this CameraSpacePoint lastAnchor, CameraSpacePoint newPosition, ref Int32 count)
        {
            var result = new CameraSpacePoint();
            
            // TODO ! RORU find out poblem of this filtering
            //result.X = (lastAnchor.X * count) / (count + 1) + newPosition.X * (1 / (count + 1));
            //result.Y = (lastAnchor.Y * count) / (count + 1) + newPosition.Y * (1 / (count + 1));
            //result.Z = (lastAnchor.Z * count) / (count + 1) + newPosition.Z * (1 / (count + 1));

            result.X = (lastAnchor.X + newPosition.X) / 2;
            result.Y = (lastAnchor.Y + newPosition.Y) / 2;
            result.Z = (lastAnchor.Z + newPosition.Z) / 2;

            ++count;

            return result;
        }
    }
}
