using System;
using SkiaSharp;

namespace LowPolyLibrary
{
	internal class cRectangleF
	{
		public SKPoint A;
		public SKPoint B;
		public SKPoint C;
		public SKPoint D;

	    internal cRectangleF(){}

	    internal cRectangleF(SKPoint a, SKPoint b, SKPoint c, SKPoint d)
	    {
	        A = new SKPoint(a.X, a.Y);
	        B = new SKPoint(b.X, b.Y);
	        C = new SKPoint(c.X, c.Y);
	        D = new SKPoint(d.X, d.Y);
	    }

	    private SKPoint vector(SKPoint p1, SKPoint p2)
	    {
	        var point = new SKPoint();
	        point.X = p2.X - p1.X;
	        point.Y = p2.Y - p1.Y;
	        return point;
	    }

	    private float dot(SKPoint u, SKPoint v)
	    {
	        return u.X*v.X + u.Y*v.Y;
	    }

        public bool Contains(SKPoint m)
		{
            //all contains logic from
            //http://math.stackexchange.com/a/190373
            //http://stackoverflow.com/a/37865332/3344317
            var AB = vector(A, B);
            var AM = vector(A, m);
            var BC = vector(B, C);
            var BM = vector(B, m);
            var dotABAM = dot(AB, AM);
            var dotABAB = dot(AB, AB);
            var dotBCBM = dot(BC, BM);
            var dotBCBC = dot(BC, BC);
            return 0 <= dotABAM && dotABAM <= dotABAB && 0 <= dotBCBM && dotBCBM <= dotBCBC;
        }

		internal bool circleContainsPoints(SKPoint circle, int radius, SKPoint point1, SKPoint point2)
		{
			//http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html

			var abs = (point2.X - point1.X)*(point1.Y - circle.Y) - (point1.X-circle.X)*(point2.Y-point1.Y);
			var top = Math.Abs(abs);
			var sqrt = (point2.X - point1.X) * (point2.X - point1.X) + (point2.Y - point1.Y) * (point2.Y - point1.Y);
			var bottom = Math.Sqrt(sqrt);
			var distance = top / bottom;
			return distance <= radius;
		}

		public bool isInsideCircle(SKPoint center, int radius)
		{
			return  circleContainsPoints(center, radius, A, B) ||
				    circleContainsPoints(center, radius, B, C) ||
					circleContainsPoints(center, radius, C, D) ||
					circleContainsPoints(center, radius, D, A);
		}
	}
}

