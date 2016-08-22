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

	    internal cRectangleF()
	    {
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

        private float recArea()
		{
			AnimationLib anim = new AnimationLib();
			var recHeight = anim.dist(A, B);
			var recBase = anim.dist(B, C);
			var area = recHeight * recBase;
			return (float)area;
		}

		public bool Contains(PointF m)
		{
            //all containst logic from
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
	}
}

