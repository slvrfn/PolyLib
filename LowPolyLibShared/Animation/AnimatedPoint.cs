using System;
using SkiaSharp;
namespace LowPolyLibrary.Animation
{
	public class AnimatedPoint
	{
		public SKPoint Point;
		public float XDisplacement;
		public float YDisplacement;

        //used to set an optional max displacement available for this point
	    public bool LimitDisplacement
	    {
	        get;
	        private set;
	    }

        public float MaxXDisplacement
	    {
	        get;
	        private set;
	    }
        public float MaxYDisplacement
	    {
            get;
            private set;
        }

	    public AnimatedPoint()
		{
			Point = new SKPoint();
			XDisplacement = 0.0f;
			YDisplacement = 0.0f;
		}

		public AnimatedPoint(SKPoint point, float xDisplacement, float yDisplacement)
		{
			Point = point;
			XDisplacement = xDisplacement;
			YDisplacement = yDisplacement;
		}

		public AnimatedPoint(SKPoint point)
		{
			Point = point;
			XDisplacement = 0.0f;
			YDisplacement = 0.0f;
		}

		public AnimatedPoint(DelaunayTriangulator.Vertex vertex)
		{
			Point = new SKPoint(vertex.x, vertex.y);
			XDisplacement = 0.0f;
			YDisplacement = 0.0f;
		}

	    public void SetMaxDisplacement(float x, float y)
	    {
	        LimitDisplacement = true;
	        MaxXDisplacement = x;
	        MaxYDisplacement = y;
	    }

	    protected bool Equals(AnimatedPoint other)
	    {
	        return Point.Equals(other.Point) && XDisplacement.Equals(other.XDisplacement) && YDisplacement.Equals(other.YDisplacement);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((AnimatedPoint) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            var hashCode = Point.GetHashCode();
	            hashCode = (hashCode * 397) ^ XDisplacement.GetHashCode();
	            hashCode = (hashCode * 397) ^ YDisplacement.GetHashCode();
	            return hashCode;
	        }
	    }
	}
}
