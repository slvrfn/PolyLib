using System;
using System.Drawing;
using System.Collections.Generic;
using ceometric.DelaunayTriangulator;
using Android.Graphics;

namespace LowPolyLibrary
{
	public class LowPolyLib
	{
		private DelaunayTriangulation2d _delaunay = new DelaunayTriangulation2d();
		private List<ceometric.DelaunayTriangulator.Point> _points;
	    private List<Triangle> triangulatedPoints;
        public int boundsWidth;
		public int boundsHeight;
		public double cell_size = 75;
		public double setVariance = .75;
		private double calcVariance, cells_x, cells_y;
		private double bleed_x, bleed_y;
		private static int numFrames = 12; //static necessary for creation of framedTriangles list
		List<Triangle>[] framedTriangles = new List<Triangle>[numFrames];

		Random rand = new Random();

		public Bitmap GenerateNew()
		{
			UpdateVars();
			_points = GeneratePoints();
			return createBitmap();
		}

		private void UpdateVars()
		{
			calcVariance = cell_size * setVariance / 2;
			cells_x = Math.Floor((boundsWidth + 4 * cell_size) / cell_size);
			cells_y = Math.Floor((boundsHeight + 4 * cell_size) / cell_size);
			bleed_x = ((cells_x * cell_size) - boundsWidth) / 2;
			bleed_y = ((cells_y * cell_size) - boundsHeight) / 2;
		}

		private Bitmap createBitmap()
		{
			Bitmap drawingCanvas = Bitmap.CreateBitmap (boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
			Canvas canvas = new Canvas (drawingCanvas);

			Paint paint = new Paint();

			paint.StrokeWidth = .5f;
			paint.SetStyle (Paint.Style.FillAndStroke);
			paint.AntiAlias = true;

		    var overlays = createOverlays();
		    for (int i = 0; i < framedTriangles.Length; i++)
		    {
		        framedTriangles[i] = new List<Triangle>();
		    }

			var gradient = getGradient();
			triangulatedPoints = _delaunay.Triangulate(_points);
			for (int i = 0; i < triangulatedPoints.Count; i++)
			{
				System.Drawing.PointF a = new System.Drawing.PointF ((float)triangulatedPoints [i].Vertex1.X, (float)triangulatedPoints [i].Vertex1.Y);
				System.Drawing.PointF b = new System.Drawing.PointF ((float)triangulatedPoints [i].Vertex2.X, (float)triangulatedPoints [i].Vertex2.Y);
				System.Drawing.PointF c = new System.Drawing.PointF ((float)triangulatedPoints [i].Vertex3.X, (float)triangulatedPoints [i].Vertex3.Y);
                
			    Path trianglePath = drawTrianglePath(a, b, c);

				var center = centroid(triangulatedPoints[i]);

                //annimation logic
                for (int j = 0; j<overlays.Length; j++)
                {
                    var adjustedCenter = keepInPicBounds(center);
                    //if the rectangle contains the center of a given triangle
                    if (overlays[j].Contains(adjustedCenter))
                    {
                        //add a reference of the triangle to the list of the rectangle it is inside
                        framedTriangles[j].Add(triangulatedPoints[i]);
                    }
                }

			    paint.Color = getTriangleColor (gradient, center);

				canvas.DrawPath (trianglePath, paint);
			}
			return drawingCanvas;
		}

	    private RectangleF[] createOverlays()
	    {
            //first and last rectangles need to be wider to cover points that are outside to the left and right of the pic bounds
			//all rectangles need to be higher and lower than the pic bounds to cover points above and below the pic bounds

			//get width of frame when there are 12 rectangles on screen
            var frameWidth = boundsWidth / numFrames;
            //represents the left edge of the rectangles
            var currentX = 0;
			//array size numFrames of rectangles. each array entry serves as a rectangle(i) starting from the left
            RectangleF[] frames = new RectangleF[numFrames];
            for (int i = 0; i < numFrames; i++)
            {
				System.Drawing.RectangleF overlay;
				//if the first rectangle
				if (i == 0)
					overlay = new RectangleF(currentX - (boundsWidth / 2), 0 - (boundsHeight / 2), frameWidth + (boundsWidth / 2), boundsHeight + (boundsHeight / 2));
				//if the last rectangle
				else if (i == numFrames - 1)
					overlay = new RectangleF(currentX, 0 - (boundsHeight / 2), frameWidth + (boundsWidth / 2), boundsHeight + (boundsHeight / 2));
				else
					overlay = new RectangleF(currentX, 0 - (boundsHeight / 2), frameWidth, boundsHeight + (boundsHeight / 2));
				
                frames[i] = overlay;
                currentX += frameWidth;
            }
	        return frames;
	    }

	    private Path drawTrianglePath(System.Drawing.PointF a, System.Drawing.PointF b, System.Drawing.PointF c)
	    {
            Path path = new Path();
            path.SetFillType(Path.FillType.EvenOdd);
            path.MoveTo(b.X, b.Y);
            path.LineTo(c.X, c.Y);
            path.LineTo(a.X, a.Y);
            path.Close();
            return path;
        }

		private Android.Graphics.Color getTriangleColor(Bitmap gradient, System.Drawing.Point center)
		{
		    center = keepInPicBounds(center);

			var colorFromRGB = System.Drawing.Color.FromArgb( gradient.GetPixel(center.X, center.Y));

			Android.Graphics.Color triColor = Android.Graphics.Color.Rgb (colorFromRGB.R, colorFromRGB.G, colorFromRGB.B);
			return triColor;
		}

	    private System.Drawing.Point keepInPicBounds(System.Drawing.Point center)
	    {
            if (center.X < 0)
                center.X += (int)bleed_x;
            else if (center.X > boundsWidth)
                center.X -= (int)bleed_x;
            else if (center.X == boundsWidth)
                center.X -= (int)bleed_x - 1;
            if (center.Y < 0)
                center.Y += (int)bleed_y;
            else if (center.Y > boundsHeight)
                center.Y -= (int)bleed_y + 1;
            else if (center.Y == boundsHeight)
                center.Y -= (int)bleed_y - 1;
	        return center;
	    }

		private	int[] getGradientColors()
		{
			//get all gradient codes
			var values = Enum.GetValues(typeof(ColorBru.Code));
			ColorBru.Code randomCode = (ColorBru.Code)values.GetValue(rand.Next(values.Length));
			//gets specified colors in gradient length: #
			var brewColors = ColorBru.GetHtmlCodes (randomCode, 6);
			//array of ints converted from brewColors
			var colorArray = new int[brewColors.Length];
			for (int i = 0; i < brewColors.Length; i++) {
				colorArray [i] = Android.Graphics.Color.ParseColor (brewColors [i]);
			}
			return colorArray;
		}

		private Bitmap getGradient()
		{
			var colorArray = getGradientColors ();

			Shader gradientShader;

			switch (rand.Next(3)) {
			case 0:
				gradientShader = new LinearGradient (
					                      0,
					                      0,
					                      boundsWidth,
					                      boundsHeight,
					                      colorArray,
					                      null,
					                      Shader.TileMode.Repeat
				                      );
				break;
			case 1:
				gradientShader = new SweepGradient (
					((float)boundsWidth / 2),
					((float)boundsHeight / 2),
					colorArray,
					//new float[]{ }
					null
				);
				break;
			case 2:
				gradientShader = new RadialGradient (
					                        ((float)boundsWidth / 2),
					                        ((float)boundsHeight / 2),
					                        ((float)boundsWidth / 2),
					                        colorArray,
					                        null,
					                        Shader.TileMode.Clamp
				                        );
				break;
			default:
				gradientShader = new LinearGradient (
					0,
					0,
					boundsWidth,
					boundsHeight,
					colorArray,
					null,
					Shader.TileMode.Repeat
				);
				break;
			}

//			LinearGradientBrush brush = new LinearGradientBrush(
//				new System.Drawing.Point(0,0),
//				new System.Drawing.Point(width, height), 
//				Color.FromArgb(255,0,0,255),
//				Color.FromArgb(255,0,255,0));

//			Bitmap temp = new Bitmap(width, height);
			Bitmap bmp = Bitmap.CreateBitmap (boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
//			Graphics graphics = Graphics.FromImage(temp);
			Canvas canvas = new Canvas (bmp);
			Paint pnt = new Paint();
			pnt.SetStyle (Paint.Style.Fill);
			pnt.SetShader (gradientShader);
			canvas.DrawRect(0,0,boundsWidth,boundsHeight,pnt);
			return bmp;
		}

		public List<ceometric.DelaunayTriangulator.Point> GeneratePoints()
		{
			var points = new List<ceometric.DelaunayTriangulator.Point>();
			for (var i = - bleed_x; i < boundsWidth + bleed_x; i += cell_size) 
			{
				for (var j = - bleed_y; j < boundsHeight + bleed_y; j += cell_size) 
				{
					var x = i + cell_size/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					var y = j + cell_size/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					points.Add(new ceometric.DelaunayTriangulator.Point(Math.Floor(x),Math.Floor(y),0));
				}
			}
			return points;
		}

		private double _map(double num, int[] in_range, double[] out_range)
		{
			return (num - in_range[0]) * (out_range[1] - out_range[0]) / (in_range[1] - in_range[0]) + out_range[0];
		}

		private System.Drawing.Point centroid(Triangle triangle)
		{
			int x = (int)((triangle.Vertex1.X + triangle.Vertex2.X + triangle.Vertex3.X)/3);
			int y = (int)((triangle.Vertex1.Y + triangle.Vertex2.Y + triangle.Vertex3.Y) / 3);

			return new System.Drawing.Point(x,y);
		}
	}
}

