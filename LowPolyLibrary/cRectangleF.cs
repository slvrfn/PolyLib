using System;
using System.Drawing;

namespace LowPolyLibrary
{
	internal class cRectangleF
	{
		public PointF A;
		public PointF B;
		public PointF C;
		public PointF D;

	    internal cRectangleF(){}

	    internal cRectangleF(PointF a, PointF b, PointF c, PointF d)
	    {
	        A = new PointF(a.X, a.Y);
	        B = new PointF(b.X, b.Y);
	        C = new PointF(c.X, c.Y);
	        D = new PointF(d.X, d.Y);
	    }

	    private PointF vector(PointF p1, PointF p2)
	    {
	        var point = new PointF();
	        point.X = p2.X - p1.X;
	        point.Y = p2.Y - p1.Y;
	        return point;
	    }

	    private float dot(PointF u, PointF v)
	    {
	        return u.X*v.X + u.Y*v.Y;
	    }

        public bool Contains(PointF m)
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

		internal bool circleContainsPoints(PointF circle, int radius, PointF point1, PointF point2)
		{
			//http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html

			var abs = (point2.X - point1.X)*(point1.Y - circle.Y) - (point1.X-circle.X)*(point2.Y-point1.Y);
			var top = Math.Abs(abs);
			var sqrt = (point2.X - point1.X) * (point2.X - point1.X) + (point2.Y - point1.Y) * (point2.Y - point1.Y);
			var bottom = Math.Sqrt(sqrt);
			var distance = top / bottom;
			return distance <= radius;
		}

		public bool isInsideCircle(PointF center, int radius)
		{
			return  circleContainsPoints(center, radius, A, B) ||
				    circleContainsPoints(center, radius, B, C) ||
					circleContainsPoints(center, radius, C, D) ||
					circleContainsPoints(center, radius, D, A);
		}
	}
}

