using System;
using System.Drawing;
namespace LowPolyLibrary
{
	public class AnimatedPoint
	{
		public PointF Point;
		public float XDisplacement;
		public float YDisplacement;

		public AnimatedPoint()
		{
			Point = new PointF();
			XDisplacement = 0.0f;
			YDisplacement = 0.0f;
		}

		public AnimatedPoint(PointF point, float xDisplacement, float yDisplacement)
		{
			Point = point;
			XDisplacement = xDisplacement;
			YDisplacement = yDisplacement;
		}

		public AnimatedPoint(PointF point)
		{
			Point = point;
			XDisplacement = 0.0f;
			YDisplacement = 0.0f;
		}

		public AnimatedPoint(DelaunayTriangulator.Vertex vertex)
		{
			Point = new PointF(vertex.x, vertex.y);
			XDisplacement = 0.0f;
			YDisplacement = 0.0f;
		}
	}
}
