using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DelaunayTriangulator;

namespace LowPolyLibrary
{
    class Geometry
    {
        internal bool pointInsideCircle(PointF point, PointF center, int radius)
        {
            //http://stackoverflow.com/questions/481144/equation-for-testing-if-a-point-is-inside-a-circle
            return (point.X - center.X) * (point.X - center.X) + (point.Y - center.Y) * (point.Y - center.Y) < radius * radius;
        }

        /*private double getPolarCoordinates(PointF center, PointF point)
		{
			var y = Math.Abs(center.Y - point.Y);
			var x = Math.Abs(center.X - point.X);
			var radians = Math.Atan(y / x);
			return radiansToDegrees(radians);
		}*/

        //this may be more correct since graphics origin is in the TL corner instead of BL
        private double GetPolarCoordinates(Point center, Point point)
        {
            //http://stackoverflow.com/questions/2676719/calculating-the-angle-between-the-line-defined-by-two-points
            var x = point.X - center.X;
            var y = center.Y - point.Y;
            var radians = Math.Atan(y / x);

            var degrees = radiansToDegrees(radians);

            if (point.X < center.X)
                degrees += 180;

            if (degrees < 0)
            {
                degrees = 360 + degrees;
            }

            return degrees;
        }

        private double degreesToRadians(int angle)
        {
            var toRad = Math.PI / 180;
            return angle * toRad;
        }

        private double radiansToDegrees(double angle)
        {
            var toDeg = 180 / Math.PI;
            return angle * toDeg;
        }

        private PointF getIntersection(float slope, PointF linePoint, PointF perpendicularLinePoint)
        {
            var linePoint2 = new PointF();
            var point2Offset = (float)(2 * dist(linePoint, perpendicularLinePoint));
            //if (slope > 0)
            //    point2Offset = -point2Offset;
            linePoint2.X = linePoint.X + point2Offset;
            //linePoint2.Y = (slope * linePoint2.X);
            linePoint2.Y = (slope * linePoint2.X) - (slope * linePoint.X) + linePoint.Y;
            //http://stackoverflow.com/questions/10301001/perpendicular-on-a-line-segment-from-a-given-point
            //var k = ((linePoint2.Y - linePoint.Y) * (perpendicularLinePoint.X - linePoint.X) - (linePoint2.X - linePoint.X) * (perpendicularLinePoint.Y - linePoint.Y)) / (((linePoint2.Y - linePoint.Y) * (linePoint2.Y - linePoint.Y)) + ((linePoint2.X - linePoint.X) * (linePoint2.X - linePoint.X)));
            var top = (perpendicularLinePoint.X - linePoint.X) * (linePoint2.X - linePoint.X) +
                    (perpendicularLinePoint.Y - linePoint.Y) * (linePoint2.Y - linePoint.Y);

            var bottom = (linePoint2.X - linePoint.X) * (linePoint2.X - linePoint.X) +
                         (linePoint2.Y - linePoint.Y) * (linePoint2.Y - linePoint.Y);
            var t = top / bottom;

            var x4 = linePoint.X + t * (linePoint2.X - linePoint.X);
            var y4 = linePoint.Y + t * (linePoint2.Y - linePoint.Y);

            return new PointF(x4, y4);
        }

        private PointF walkAngle(int angle, float distance, PointF startingPoint)
        {
            var endPoint = new PointF(startingPoint.X, startingPoint.Y);
            var y = distance * ((float)Math.Sin(degreesToRadians(angle)));
            var x = distance * ((float)Math.Cos(degreesToRadians(angle)));
            endPoint.X += x;
            endPoint.Y += y;
            return endPoint;
        }

        private double dist(PointF workingPoint, DelaunayTriangulator.Vertex vertex)
        {
            var xSquare = (workingPoint.X - vertex.x) * (workingPoint.X - vertex.x);
            var ySquare = (workingPoint.Y - vertex.y) * (workingPoint.Y - vertex.y);
            return Math.Sqrt(xSquare + ySquare);
        }

        internal double dist(PointF workingPoint, PointF vertex)
        {
            var xSquare = (workingPoint.X - vertex.X) * (workingPoint.X - vertex.X);
            var ySquare = (workingPoint.Y - vertex.Y) * (workingPoint.Y - vertex.Y);
            return Math.Sqrt(xSquare + ySquare);
        }

        internal int getAngleInRange(int angle, int range)
        {
            var rand = new System.Random();
            var range_lower = angle - range;
            var range_upper = angle + range;
            //return a int from range_lower to range_upper that represents the direction a point will move
            //this is done to add an amount of 'variability'. Each point will travel in the same general direction, but with a little bit of 'wiggle room'
            return rand.Next(range_lower, range_upper);
        }

        internal int get360Direction()
        {
            var rand = new System.Random();
            //return a int from 0 to 359 that represents the direction a point will move
            return rand.Next(360);
        }

        private double getXComponent(int angle, double length)
        {
            return length * Math.Cos(degreesToRadians(angle));
        }

        private double getYComponent(int angle, double length)
        {
            return length * Math.Sin(degreesToRadians(angle));
        }

        internal System.Drawing.Point centroid(Triad triangle, List<DelaunayTriangulator.Vertex> points)
        {
            var x = (int)((points[triangle.a].x + points[triangle.b].x + points[triangle.c].x) / 3);
            var y = (int)((points[triangle.a].y + points[triangle.b].y + points[triangle.c].y) / 3);

            return new System.Drawing.Point(x, y);
        }
    }
}
