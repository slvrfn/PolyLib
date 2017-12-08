using System;
using System.Collections.Generic;
using System.Text;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary
{
	public class Geometry
	{
		internal static SKPoint KeepInPicBounds(SKPoint center, double bleed_x, double bleed_y, int BoundsWidth, int BoundsHeight)
		{
			if (center.X < 0)
				center.X += (int)bleed_x;
			else if (center.X > BoundsWidth)
				center.X -= (int)bleed_x;
			else if (center.X.Equals(BoundsWidth))
				center.X -= (int)bleed_x - 1;
			if (center.Y < 0)
				center.Y += (int)bleed_y;
			else if (center.Y > BoundsHeight)
				center.Y -= (int)bleed_y + 1;
			else if (center.Y.Equals(BoundsHeight))
				center.Y -= (int)bleed_y - 1;
			return center;
		}

		#region Circles
		internal static bool pointInsideCircle(SKPoint point, SKPoint center, int radius)
		{
			//http://stackoverflow.com/questions/481144/equation-for-testing-if-a-point-is-inside-a-circle
			return (point.X - center.X) * (point.X - center.X) + (point.Y - center.Y) * (point.Y - center.Y) < radius * radius;
		}

		internal static double GetPolarCoordinates(SKPoint center, SKPoint point)
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

		internal static double degreesToRadians(float angle)
		{
			var toRad = Math.PI / 180;
			return angle * toRad;
		}

		internal static double radiansToDegrees(double angle)
		{
			var toDeg = 180 / Math.PI;
			return angle * toDeg;
		}
		#endregion

		#region Rectangles
        // create rotated rectangle that perfectly fits around the rectangle specified by h&w
        public static Rectangle createContainingRec(int angle, int boundsWidth, int boundsHeight)
        {
            var radians = Geometry.degreesToRadians(angle);

            //used to avoid undefined tan evaluation
            //this makes the angle off by +~.057 degrees at these angles
            if (angle == 90 || angle == 270)
            {
                radians += .001f;
            }

            //slope of the given angle
            var slope = (float)Math.Tan(radians);
            var recipSlope = -1 / slope;
            
            var drawingAreaA = new SKPoint(0, boundsHeight);
            var drawingAreaB = new SKPoint(boundsWidth, boundsHeight);
            var drawingAreaC = new SKPoint(boundsWidth, 0);
            var drawingAreaD = new SKPoint(0, 0);

            SKPoint cornerA;
            SKPoint cornerB;
            SKPoint cornerC;
            SKPoint cornerD;

            if (angle < 90)
            {
                //quad1
                cornerA = drawingAreaA;
                cornerB = drawingAreaB;
                cornerC = drawingAreaC;
                cornerD = drawingAreaD;
            }
            else if (angle < 180)
            {
                //quad2
                cornerA = drawingAreaD;
                cornerB = drawingAreaA;
                cornerC = drawingAreaB;
                cornerD = drawingAreaC;
            }
            else if (angle < 270)
            {
                //quad3
                cornerA = drawingAreaC;
                cornerB = drawingAreaD;
                cornerC = drawingAreaA;
                cornerD = drawingAreaB;
            }
            else
            {
                //quad4
                cornerA = drawingAreaB;
                cornerB = drawingAreaC;
                cornerC = drawingAreaD;
                cornerD = drawingAreaA;
            }

            SKPoint ABIntersection, DCIntersection, BCIntersection,  ADIntersection;
            
            //A
            ADIntersection = Geometry.getIntersection(slope, cornerA, cornerD);
            //B
            ABIntersection = Geometry.getIntersection(slope, cornerA, cornerB);
            //C
            BCIntersection = Geometry.getIntersection(recipSlope, cornerB, cornerC);
            //D
            DCIntersection = Geometry.getIntersection(recipSlope, cornerD, cornerC);

            return new Rectangle(ADIntersection, ABIntersection, BCIntersection, DCIntersection); 
        }

        // create the RotatedGrid numFrames*numFrames transformation for the given angle and bounds w&h
        public static RotatedGrid createGridTransformation(int angle, int boundsWidth, int boundsHeight, int numFrames)
        {
            var containingRec = createContainingRec(angle, boundsWidth, boundsHeight);

            //creating X basis, so needs to be 1 relative unit away on X plane
			var newB = walkAngle(angle, 1f, containingRec.A);

			//reciprocal used bc perpendicular from current angle
			var recipAngle = (angle + 270) % 360;
			//creating Y basis, so needs to be 1 relative unit away on Y plane
			var newD = walkAngle(recipAngle, 1f, containingRec.A);

			//find origin shift (negative to shift points to standard origin
			var trans = SKMatrix.MakeTranslation(-containingRec.A.X, -containingRec.A.Y);
			//need to translate B&D by origin to make the points relative from their origin (A)
			newB = trans.MapPoint(newB);
			newD = trans.MapPoint(newD);

			//grid in space
			var basisMatrix = new SKMatrix()
			{
				ScaleX = newB.X,
				SkewX = newB.Y,
				SkewY = newD.X,
				ScaleY = newD.Y,
				TransX = containingRec.A.X,
				TransY = containingRec.A.Y,
				Persp2 = 1
			};

            return new RotatedGrid(basisMatrix, containingRec, numFrames);
        }

	    public static Rectangle[][] createContaingGrid(int angle, int numFrames, int boundsWidth, int boundsHeight)
	    {
            var rotGrid = createGridTransformation(angle, boundsWidth, boundsHeight, numFrames);

            Rectangle[][] grid = new Rectangle[numFrames][];
	        for (int i = 0; i < numFrames; i++)
	        {
	            grid[i] = new Rectangle[numFrames];
	        }

            for (int i = 0; i < grid.Length; i++)
	        {
	            for (int j = 0; j < grid[i].Length; j++)
	            {
	                var A = new SKPoint(j * rotGrid.CellWidth, i * rotGrid.CellHeight);

	                var B = new SKPoint(A.X, A.Y);
	                B.Offset(rotGrid.CellWidth, 0);

	                var C = new SKPoint(A.X, A.Y);
	                C.Offset(rotGrid.CellWidth, rotGrid.CellHeight);

                    var D = new SKPoint(A.X, A.Y);
	                D.Offset(0, rotGrid.CellHeight);

                    grid[i][j] = new Rectangle(rotGrid.ToOriginCoords(A), rotGrid.ToOriginCoords(B), 
                                                 rotGrid.ToOriginCoords(C), rotGrid.ToOriginCoords(D));
	            }
	        }


	        return grid;
	    }

		public static Rectangle gridRecAroundTouch(SKPoint touch, int angle, int numFrames, int boundsWidth, int boundsHeight)
		{
			var rotGrid = createGridTransformation(angle, boundsWidth, boundsHeight, numFrames);

            var gridCoords = rotGrid.CellCoordsFromOriginPoint(touch);

            var A = new SKPoint(gridCoords.X * rotGrid.CellWidth, gridCoords.Y * rotGrid.CellHeight);

			var B = new SKPoint(A.X, A.Y);
			B.Offset(rotGrid.CellWidth, 0);

			var C = new SKPoint(A.X, A.Y);
			C.Offset(rotGrid.CellWidth, rotGrid.CellHeight);

			var D = new SKPoint(A.X, A.Y);
			D.Offset(0, rotGrid.CellHeight);

            return new Rectangle(rotGrid.ToOriginCoords(A), rotGrid.ToOriginCoords(B),
                                   rotGrid.ToOriginCoords(C), rotGrid.ToOriginCoords(D));
		}

        public class Rectangle
        {
            public SKPoint A;
            public SKPoint B;
            public SKPoint C;
            public SKPoint D;

            public Rectangle() { }

            public Rectangle(SKPoint a, SKPoint b, SKPoint c, SKPoint d)
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
                return u.X * v.X + u.Y * v.Y;
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

            public override string ToString()
            {
                return $"{nameof(A)}: {A}, {nameof(B)}: {B}, {nameof(C)}: {C}, {nameof(D)}: {D}";
            }

            //internal bool circleContainsPoints(SKPoint circle, int radius, SKPoint point1, SKPoint point2)
            //{
            //  //http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html

            //  var abs = (point2.X - point1.X)*(point1.Y - circle.Y) - (point1.X-circle.X)*(point2.Y-point1.Y);
            //  var top = Math.Abs(abs);
            //  var sqrt = (point2.X - point1.X) * (point2.X - point1.X) + (point2.Y - point1.Y) * (point2.Y - point1.Y);
            //  var bottom = Math.Sqrt(sqrt);
            //  var distance = top / bottom;
            //  return distance <= radius;
            //}

            //public bool isInsideCircle(SKPoint center, int radius)
            //{
            //  return  circleContainsPoints(center, radius, A, B) ||
            //          circleContainsPoints(center, radius, B, C) ||
            //          circleContainsPoints(center, radius, C, D) ||
            //          circleContainsPoints(center, radius, D, A);
            //}
        }
		
        public class RotatedGrid
        {
            public SKMatrix FromGrid { get; private set; }
            //made since properties cane be passed with out identifier
            private SKMatrix toGrid;
            public SKMatrix ToGrid { get { return toGrid; } }

            public Rectangle GridContainer { get; private set; }

            public float CellWidth { get; private set; }
            public float CellHeight { get; private set; }

            public RotatedGrid(SKMatrix matrix, Rectangle containingRec, int numFrames)
            {
                GridContainer = containingRec;
                CellWidth = (float)dist(containingRec.A, containingRec.B) / numFrames;
                CellHeight = (float)dist(containingRec.A, containingRec.D) / numFrames;
                FromGrid = matrix;
                //try to invert, if unsucessful ToGrid will be all 0's
                if (!FromGrid.TryInvert(out toGrid))
                {
                    toGrid = new SKMatrix();
                }
            }

            public SKPoint ToGridCoords(SKPoint originPoint)
            {
                return ToGrid.MapPoint(originPoint);
            }

            public SKPoint ToOriginCoords(SKPoint gridPoint)
            {
                return FromGrid.MapPoint(gridPoint);
            }

            public SKPointI CellCoordsFromOriginPoint(SKPoint originPoint)
            {
                var pInGridCoords = ToGridCoords(originPoint);
                return CellCoordsFromGridPoint(pInGridCoords);
            }

            public SKPointI CellCoordsFromGridPoint(SKPoint gridPoint)
            {
                var y = (int)(gridPoint.Y / CellHeight);
                var x = (int)(gridPoint.X / CellWidth);
                return new SKPointI(x, y);
            }
        }
        #endregion

		#region Lines
		internal static SKPoint getIntersection(float slope, SKPoint linePoint, SKPoint perpendicularLinePoint)
		{
			var linePoint2 = new SKPoint();
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

			return new SKPoint(x4, y4);
		}

		internal static SKPoint walkAngle(int angle, float distance, SKPoint startingPoint)
		{
			var endPoint = new SKPoint(startingPoint.X, startingPoint.Y);
			var y = distance * ((float)Math.Sin(degreesToRadians(angle)));
			var x = distance * ((float)Math.Cos(degreesToRadians(angle)));
			endPoint.X += x;
			endPoint.Y += y;
			return endPoint;
		}

		internal static double dist(SKPoint workingPoint, DelaunayTriangulator.Vertex vertex)
		{
			var xSquare = (workingPoint.X - vertex.x) * (workingPoint.X - vertex.x);
			var ySquare = (workingPoint.Y - vertex.y) * (workingPoint.Y - vertex.y);
			return Math.Sqrt(xSquare + ySquare);
		}

		internal static double dist(SKPoint workingPoint, SKPoint vertex)
		{
			var xSquare = (workingPoint.X - vertex.X) * (workingPoint.X - vertex.X);
			var ySquare = (workingPoint.Y - vertex.Y) * (workingPoint.Y - vertex.Y);
			return Math.Sqrt(xSquare + ySquare);
		}

		internal static double getXComponent(int angle, double length)
		{
			return length * Math.Cos(degreesToRadians(angle));
		}

		internal static double getYComponent(int angle, double length)
		{
			return length * Math.Sin(degreesToRadians(angle));
		}

		internal static int getAngleInRange(int angle, int range)
		{
			var rand = new System.Random();
			var range_lower = angle - range;
			var range_upper = angle + range;
			//return a int from range_lower to range_upper that represents the direction a point will move
			//this is done to add an amount of 'variability'. Each point will travel in the same general direction, but with a little bit of 'wiggle room'
			return rand.Next(range_lower, range_upper);
		}

		internal static int get360Direction()
		{
			var rand = new System.Random();
			//return a int from 0 to 359 that represents the direction a point will move
			return rand.Next(360);
		}
		#endregion

		#region Triangles
		internal static SKPoint centroid(Triad triangle, List<DelaunayTriangulator.Vertex> points)
		{
			var x = (int)((points[triangle.a].x + points[triangle.b].x + points[triangle.c].x) / 3);
			var y = (int)((points[triangle.a].y + points[triangle.b].y + points[triangle.c].y) / 3);

			return new SKPoint(x, y);
		}

		internal static SKColor GetTriangleColor(SKSurface gradient, SKPoint center)
		{
            //center = KeepInPicBounds(center, bleed_x, bleed_y, BoundsWidth, BoundsHeight);
            
		    //https://forums.xamarin.com/discussion/92899/read-a-pixel-info-from-a-canvas
            SKImageInfo dstinf = new SKImageInfo();
		    dstinf.ColorType = SKColorType.Argb4444;
		    dstinf.AlphaType = SKAlphaType.Premul;

            dstinf.Width = 1;
		    dstinf.Height = 1;

		    // create the 1x1 bitmap (auto allocates the pixel buffer)
            using (SKBitmap bitmap = new SKBitmap(dstinf))
		    {
		        // get the pixel buffer for the bitmap
		        IntPtr dstpixels = bitmap.GetPixels();

		        // read the surface into the bitmap
		        gradient.ReadPixels(dstinf, dstpixels, dstinf.RowBytes, (int)center.X, (int)center.Y);

		        // access the color
		        return bitmap.GetPixel(0, 0);
            }
        }

		internal static SKPath DrawTrianglePath(SKPoint a, SKPoint b, SKPoint c)
		{
			var path = new SKPath();
            path.FillType = SKPathFillType.EvenOdd;
			path.MoveTo(b.X, b.Y);
			path.LineTo(c.X, c.Y);
			path.LineTo(a.X, a.Y);
			path.Close();
			return path;
		}
		#endregion

	}
}
