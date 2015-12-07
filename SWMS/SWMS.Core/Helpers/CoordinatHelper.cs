using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SWMS.Core.Helpers
{
    public static class CoordinateHelper
    {
        public static Double GetDistance(Point p1, Point p2)
        {
            return Math.Abs(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)));
        }

        public static Double GetDistance(CameraSpacePoint p1, CameraSpacePoint p2)
        {
            return Math.Abs(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2)));
        }


        public static Double FindPointProjection(Point b, Point m)
        {
            var a = new Point()
            {
                X = b.X,
                Y = 0F
            };

            var k = new Point
            {
                X = b.X,
                Y = m.Y
            };

            var kmDistance = CoordinateHelper.GetDistance(k, m);
            var kbDistance = CoordinateHelper.GetDistance(k, b);
            var tgAlpha = kmDistance / kbDistance;
            var abDistnce = CoordinateHelper.GetDistance(a, b);
            var acDistance = abDistnce * tgAlpha;
            return a.X < m.X
                ? a.X + acDistance
                : a.X - acDistance;
        }


    }
}
