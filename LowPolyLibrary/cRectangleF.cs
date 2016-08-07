using System;
using System.Drawing;

namespace LowPolyLibrary
{
	struct cRectangleF
	{
		public PointF A;
		public PointF B;
		public PointF C;
		public PointF D;

		private float triArea(PointF a, PointF b, PointF c)
		{
			var tri1 = a.X * (b.Y - c.Y);
			var tri2 = b.X * (c.Y - a.Y);
			var tri3 = c.X * (a.Y - b.Y);
			var sum = tri1 + tri2 + tri3;
			var area = sum / 2;
			return Math.Abs(area);
		}

		private float recArea()
		{
			AnimationLib anim = new AnimationLib();
			var recHeight = anim.dist(A, B);
			var recBase = anim.dist(B, C);
			var area = recHeight * recBase;
			return (float)area;
		}

		public bool Contains(PointF point)
		{
			var tAPD = triArea(A, point, D);
			var tDPC = triArea(D, point, C);
			var tCPB = triArea(C, point, B);
			var tPBA = triArea(point, B, A);
			var totalTriArea = tAPD + tDPC + tCPB + tPBA;
			var rectangleArea = recArea();
			if (totalTriArea > rectangleArea)
				return false;
			return true;
		}
	}
}

