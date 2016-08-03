using System.Drawing;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Java.Lang;
using Java.Util;
using Triangulator = DelaunayTriangulator.Triangulator;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Enum = System.Enum;
using Math = System.Math;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
	public partial class LowPolyLib
	{
		private List<DelaunayTriangulator.Vertex> _points;
	    private List<Triad> triangulatedPoints;
        public int boundsWidth;
		public int boundsHeight;
		public double cell_size = 75;
		public double setVariance = .75;
		private double calcVariance, cells_x, cells_y;
		private double bleed_x, bleed_y;
		private static int numFrames = 12; //static necessary for creation of framedPoints list
		List<System.Drawing.PointF>[] framedPoints = new List<System.Drawing.PointF>[numFrames];
        List<System.Drawing.PointF>[] wideFramedPoints = new List<System.Drawing.PointF>[numFrames];

        Bitmap gradient;

        Dictionary<System.Drawing.PointF, List<Triad>> poTriDic = new Dictionary<System.Drawing.PointF, List<Triad>>();

		System.Random rand = new System.Random();

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

			gradient = getGradient();
            var angulator = new Triangulator();
            triangulatedPoints = angulator.Triangulation(_points);
			seperatePointsIntoFrames(_points);
			//generating a new base triangulation. if an old one exists get rid of it
			if (poTriDic != null)
				poTriDic = new Dictionary<System.Drawing.PointF, List<Triad>>();

			for (int i = 0; i < triangulatedPoints.Count; i++)
			{
				var a = new PointF((float)_points[triangulatedPoints[i].a].x, (float)_points[triangulatedPoints[i].a].y);
				var b = new PointF((float)_points[triangulatedPoints[i].b].x, (float)_points[triangulatedPoints[i].b].y);
				var c = new PointF((float)_points[triangulatedPoints[i].c].x, (float)_points[triangulatedPoints[i].c].y);
                
			    Path trianglePath = drawTrianglePath(a, b, c);

				var center = centroid(triangulatedPoints[i], _points);

                //animation logic
                //divyTris(a, overlays, i);
                //divyTris(b, overlays, i);
                //divyTris(c,overlays, i);

                paint.Color = getTriangleColor (gradient, center);

				canvas.DrawPath (trianglePath, paint);
			}
			return drawingCanvas;
		}

		

        public AnimationDrawable makeAnimation(int numFrames2)
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42*2;//roughly how many milliseconds each frame will be for 24fps
            
            for (int i = 0; i < numFrames2; i++)
            {
                var frameBitmap = createAnimBitmap(i);
                BitmapDrawable frame = new BitmapDrawable(frameBitmap);
                animation.AddFrame(frame,duration);
            }
            return animation;
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

			System.Drawing.Color colorFromRGB;
			try
			{
				colorFromRGB = System.Drawing.Color.FromArgb(gradient.GetPixel(center.X, center.Y));
			}
			catch
			{
				colorFromRGB = System.Drawing.Color.Cyan;
			}

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

		public List<DelaunayTriangulator.Vertex> GeneratePoints()
		{
			var points = new List<DelaunayTriangulator.Vertex>();
			for (var i = - bleed_x; i < boundsWidth + bleed_x; i += cell_size) 
			{
				for (var j = - bleed_y; j < boundsHeight + bleed_y; j += cell_size) 
				{
					var x = i + cell_size/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					var y = j + cell_size/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					points.Add(new DelaunayTriangulator.Vertex((float)Math.Floor(x),(float)Math.Floor(y)));
				}
			}
			return points;
		}

		private double _map(double num, int[] in_range, double[] out_range)
		{
			return (num - in_range[0]) * (out_range[1] - out_range[0]) / (in_range[1] - in_range[0]) + out_range[0];
		}

		private System.Drawing.Point centroid(Triad triangle, List<DelaunayTriangulator.Vertex> points)
		{
			var x = (int)((points[triangle.a].x + points[triangle.b].x + points[triangle.c].x) / 3);
			var y = (int)((points[triangle.a].y + points[triangle.b].y + points[triangle.c].y) / 3);

			return new System.Drawing.Point(x,y);
		}
	}
}

