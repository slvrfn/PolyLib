using System;
using System.Collections.Generic;
using System.Text;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary
{
	class Geometry
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

		internal static double degreesToRadians(int angle)
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
		internal static List<cRectangleF[]> createRectangleOverlays(int angle, int numFrames, int boundsWidth, int boundsHeight)
		{
			//array size numFrames of rectangles. each array entry serves as a rotated cRectangleF
			cRectangleF[] frames = new cRectangleF[numFrames];

			//slope of the given angle
			var slope = (float)Math.Tan(Geometry.degreesToRadians(angle));
			var recipSlope = -1 / slope;

			SKPoint ADIntersection;
			SKPoint DCIntersection;
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

			ADIntersection = Geometry.getIntersection(slope, cornerA, cornerD);
			DCIntersection = Geometry.getIntersection(recipSlope, cornerD, cornerC);
			//ABIntersection used to calculate framewidth
			var ABIntersection = Geometry.getIntersection(slope, cornerA, cornerB);
			var frameWidth = (float)Geometry.dist(ADIntersection, ABIntersection) / numFrames;
			var wideOverlays = createWideRectangleOverlays(frameWidth, ADIntersection, DCIntersection, angle, numFrames, boundsWidth, boundsHeight);

			var walkedB = Geometry.walkAngle(angle, frameWidth, ADIntersection);
			var walkedC = Geometry.walkAngle(angle, frameWidth, DCIntersection);
			frames[0] = new cRectangleF
			{
				A = new SKPoint(ADIntersection.X, ADIntersection.Y),
				B = new SKPoint(walkedB.X, walkedB.Y),
				C = new SKPoint(walkedC.X, walkedC.Y),
				D = new SKPoint(DCIntersection.X, DCIntersection.Y)
			};

			//starts from second array entry because first entry is assigned above
			for (int i = 1; i < numFrames; i++)
			{
				var overlay = new cRectangleF();
				overlay.A = frames[i - 1].B;
				overlay.D = frames[i - 1].C;
				overlay.B = Geometry.walkAngle(angle, frameWidth, overlay.A);
				overlay.C = Geometry.walkAngle(angle, frameWidth, overlay.D);
				frames[i] = overlay;
			}
			var returnList = new List<cRectangleF[]>();
			returnList.Add(frames);
			returnList.Add(wideOverlays);
			return returnList;
		}

		internal static cRectangleF[] createWideRectangleOverlays(float frameWidth, SKPoint A, SKPoint D, int angle, int numFrames, int boundsWidth, int boundsHeight)
		{
			//first and last rectangles need to be wider to cover points that are outside to the left and right of the pic bounds
			//all rectangles need to be higher and lower than the pic bounds to cover points above and below the pic bounds

			//array size numFrames of rectangles. each array entry serves as a rectangle(i) starting from the left
			cRectangleF[] frames = new cRectangleF[numFrames];


			//represents the corner A of the regular overlays
			var overlayA = new SKPoint(A.X, A.Y);
			var overlayD = new SKPoint(D.X, D.Y);

			var tempWidth = boundsWidth / 2;
			var tempHeight = boundsHeight / 2;

			frames[0] = new cRectangleF();
			frames[0].A = Geometry.walkAngle(angle + 90, tempHeight, overlayA);
			frames[0].B = Geometry.walkAngle(angle, frameWidth, frames[0].A);
			frames[0].A = Geometry.walkAngle(angle + 180, tempWidth, frames[0].A);
			frames[0].D = Geometry.walkAngle(angle + 270, tempHeight, overlayD);
			frames[0].C = Geometry.walkAngle(angle, frameWidth, frames[0].D);
			frames[0].D = Geometry.walkAngle(angle + 180, tempWidth, frames[0].D);


			//this logic is for grabbing all points (even those outside the visible drawing area)
			//starts at 1 cause first array spot handled above
			for (int i = 1; i < numFrames; i++)
			{
				cRectangleF overlay = new cRectangleF();
				if (i == numFrames - 1)
				{
					overlay.A = new SKPoint(frames[i - 1].B.X, frames[i - 1].B.Y);
					overlay.D = new SKPoint(frames[i - 1].C.X, frames[i - 1].C.Y);
					overlay.B = Geometry.walkAngle(angle, frameWidth + tempWidth, overlay.A);
					overlay.C = Geometry.walkAngle(angle, frameWidth + tempWidth, overlay.D);
				}
				else
				{
					overlay.A = new SKPoint(frames[i - 1].B.X, frames[i - 1].B.Y);
					overlay.D = new SKPoint(frames[i - 1].C.X, frames[i - 1].C.Y);
					overlay.B = Geometry.walkAngle(angle, frameWidth, overlay.A);
					overlay.C = Geometry.walkAngle(angle, frameWidth, overlay.D);
				}
				frames[i] = overlay;
			}

			return frames;
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
		    //dstinf.ColorType = SKColorType.Argb4444;
		    dstinf.Width = 1;
		    dstinf.Height = 1;

		    // create the 1x1 bitmap (auto allocates the pixel buffer)
		    SKBitmap bitmap = new SKBitmap(dstinf);

		    // get the pixel buffer for the bitmap
		    IntPtr dstpixels = bitmap.GetPixels();

		    // read the surface into the bitmap
		    gradient.ReadPixels(dstinf, dstpixels, dstinf.RowBytes, (int)center.X, (int)center.Y);

		    // access the color
		    return bitmap.GetPixel(0, 0);
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
