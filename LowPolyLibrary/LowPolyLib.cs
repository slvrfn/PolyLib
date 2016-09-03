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
	public class LowPolyLib
	{
        //private List<DelaunayTriangulator.Vertex> _points;
        AnimationLib animator = new AnimationLib();
	    
        public int boundsWidth;
		public int boundsHeight;
		public double cell_size = 75;
		public double setVariance = .75;
		private double calcVariance, cells_x, cells_y;
		private double bleed_x, bleed_y;
		private static int numFrames = 12; //static necessary for creation of framedPoints list
        Bitmap gradient;

		public Bitmap GenerateNew()
		{
			UpdateVars();
			var _points = GeneratePoints();
            var direction = animator.get360Direction();
            animator.seperatePointsIntoRectangleFrames(_points, boundsWidth, boundsHeight, direction);
            return createSweepAnimBitmap(0, direction);
		}

        private void UpdateVars()
        {
            calcVariance = cell_size * setVariance / 2;
            cells_x = Math.Floor((boundsWidth + 4 * cell_size) / cell_size);
            cells_y = Math.Floor((boundsHeight + 4 * cell_size) / cell_size);
            bleed_x = ((cells_x * cell_size) - boundsWidth) / 2;
            bleed_y = ((cells_y * cell_size) - boundsHeight) / 2;
            gradient = getGradient();
        }

        public Bitmap createSweepAnimBitmap(int frame, int direction)
        {
            var frameList = animator.makeSweepPointsFrame(frame, direction);
            var frameBitmap = drawPointFrame(frameList);
            return frameBitmap;
        }

		public AnimationDrawable makeAnimation(int numFrames2)
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42*2;//roughly how many milliseconds each frame will be for 24fps
		    var direction = animator.get360Direction();
            for (int i = 0; i < numFrames2; i++)
            {
                var frameBitmap =createSweepAnimBitmap(i, direction);
                BitmapDrawable frame = new BitmapDrawable(frameBitmap);
                animation.AddFrame(frame,duration);
            }
            return animation;
        }

        private Bitmap drawTriFrame(Dictionary<PointF, List<Triad>> frameDic, List<DelaunayTriangulator.Vertex> points)
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.StrokeWidth = .5f;
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

            foreach (KeyValuePair<PointF, List<Triad>> entry in frameDic)
            {
                // do something with entry.Value or entry.Key
                var frameTriList = entry.Value;
                foreach (var tri in frameTriList)
                {
                    var a = new PointF(points[tri.a].x, points[tri.a].y);
                    var b = new PointF(points[tri.b].x, points[tri.b].y);
                    var c = new PointF(points[tri.c].x, points[tri.c].y);

                    Path trianglePath = drawTrianglePath(a, b, c);

                    var center = centroid(tri, points);

                    paint.Color = getTriangleColor(gradient, center);

                    canvas.DrawPath(trianglePath, paint);
                }
            }
            return drawingCanvas;
        }

        private Bitmap drawPointFrame(List<PointF>[] frameList)
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

            //generating a new base triangulation. if an old one exists get rid of it
            //if (poTriDic != null)
            //    poTriDic = new Dictionary<System.Drawing.PointF, List<Triad>>();

            var convertedPoints = new List<DelaunayTriangulator.Vertex>();
            //can we just stay in PointF's?
            foreach (var frame in frameList)
            {
                foreach (var point in frame)
                {
                    var currentlyExists = convertedPoints.Exists(x => 
                        x.x.CompareTo(point.X) == 0 &&
                        x.y.CompareTo(point.Y) == 0
                    );
                    if(!currentlyExists)
                        convertedPoints.Add(new DelaunayTriangulator.Vertex(point.X, point.Y));
                }
            }
            var angulator = new Triangulator();
            var newTriangulatedPoints = angulator.Triangulation(convertedPoints);
            for (int i = 0; i < newTriangulatedPoints.Count; i++)
            {
                var a = new PointF(convertedPoints[newTriangulatedPoints[i].a].x, convertedPoints[newTriangulatedPoints[i].a].y);
                var b = new PointF(convertedPoints[newTriangulatedPoints[i].b].x, convertedPoints[newTriangulatedPoints[i].b].y);
                var c = new PointF(convertedPoints[newTriangulatedPoints[i].c].x, convertedPoints[newTriangulatedPoints[i].c].y);

                Path trianglePath = drawTrianglePath(a, b, c);
                
                var center = centroid(newTriangulatedPoints[i], convertedPoints);

                //animation logic
                //divyTris(a, overlays, i);
                //divyTris(b, overlays, i);
                //divyTris(c,overlays, i);

                paint.Color = getTriangleColor(gradient, center);

                canvas.DrawPath(trianglePath, paint);
            }
            return drawingCanvas;
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
            var rand = new System.Random();
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
            var rand = new System.Random();
            var colorArray = getGradientColors ();

			Shader gradientShader;
            //set to 2, bc want to temporarily not make sweep gradient
			switch (rand.Next(2)) {
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
				    gradientShader = new RadialGradient (
					                            ((float)boundsWidth / 2),
					                            ((float)boundsHeight / 2),
					                            ((float)boundsWidth / 2),
					                            colorArray,
					                            null,
					                            Shader.TileMode.Clamp
				                            );
				    break;
               case 2:
                        gradientShader = new SweepGradient(
                            ((float)boundsWidth / 2),
                            ((float)boundsHeight / 2),
                            colorArray,
                            null
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

            Bitmap bmp = Bitmap.CreateBitmap (boundsWidth, boundsHeight, Bitmap.Config.Rgb565);

            Canvas canvas = new Canvas (bmp);
			Paint pnt = new Paint();
			pnt.SetStyle (Paint.Style.Fill);
			pnt.SetShader (gradientShader);
			canvas.DrawRect(0,0,boundsWidth,boundsHeight,pnt);
			return bmp;
		}

		public List<DelaunayTriangulator.Vertex> GeneratePoints()
		{
            var rand = new System.Random();
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

		internal System.Drawing.Point centroid(Triad triangle, List<DelaunayTriangulator.Vertex> points)
		{
			var x = (int)((points[triangle.a].x + points[triangle.b].x + points[triangle.c].x) / 3);
			var y = (int)((points[triangle.a].y + points[triangle.b].y + points[triangle.c].y) / 3);

			return new System.Drawing.Point(x,y);
		}
	}
}

