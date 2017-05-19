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
using System;
using Android.Test.Suitebuilder;
using Java.IO;
using Javax.Security.Auth;
using LowPolyLibrary.BitmapPool;

namespace LowPolyLibrary
{
	public class Triangulation
	{
        internal int BoundsWidth;
		internal int BoundsHeight;
		internal double CellSize = 150;
		public double Variance = .75;
		private double calcVariance, cells_x, cells_y;
		internal double bleed_x, bleed_y;
        internal IManagedBitmap Gradient;
		internal List<DelaunayTriangulator.Vertex> InternalPoints;

	    internal BitmapPool.BitmapPool ReuseableBitmapPool;

	    private Paint _paint;

        public List<Triad> TriangulatedPoints;

        public Triangulation(int boundsWidth, int boundsHeight, double variance, double cellSize, BitmapPool.BitmapPool imagePool)
        {
            BoundsWidth = boundsWidth;
            BoundsHeight = boundsHeight;
            Variance = variance;
            CellSize = cellSize;

            _paint = new Paint();
            _paint.SetStyle(Paint.Style.FillAndStroke);
            _paint.AntiAlias = true;

            ReuseableBitmapPool = imagePool;

            UpdateVars();
			InternalPoints = GeneratePoints();
		    var angulator = new Triangulator();
		    TriangulatedPoints = angulator.Triangulation(InternalPoints);
		}

        //~Triangulation()
        //{
        //    Gradient.Recycle();
        //    Gradient.Dispose();
        //}

	    public IManagedBitmap GeneratedBitmap
	    {
            get { return DrawFrame(); }
	    }

		private IManagedBitmap DrawFrame()
		{
		    var drawingCanvas = ReuseableBitmapPool.getBitmap();
            Canvas canvas = new Canvas(drawingCanvas.GetBitmap());
            
            for (int i = 0; i < TriangulatedPoints.Count; i++)
            {
                var a = new PointF(InternalPoints[TriangulatedPoints[i].a].x, InternalPoints[TriangulatedPoints[i].a].y);
                var b = new PointF(InternalPoints[TriangulatedPoints[i].b].x, InternalPoints[TriangulatedPoints[i].b].y);
                var c = new PointF(InternalPoints[TriangulatedPoints[i].c].x, InternalPoints[TriangulatedPoints[i].c].y);

                var center = Geometry.centroid(TriangulatedPoints[i], InternalPoints);

				var triAngleColorCenter = Geometry.KeepInPicBounds(center, bleed_x, bleed_y, BoundsWidth, BoundsHeight);
                _paint.Color =Geometry.GetTriangleColor(Gradient, triAngleColorCenter);

                using (var trianglePath = Geometry.DrawTrianglePath(a, b, c))
                {
                    canvas.DrawPath(trianglePath, _paint);
                }
            }
            return drawingCanvas;
        }

        private void UpdateVars()
        {
            calcVariance = CellSize * Variance / 2;
            cells_x = Math.Floor((BoundsWidth + 4 * CellSize) / CellSize);
            cells_y = Math.Floor((BoundsHeight + 4 * CellSize) / CellSize);
            bleed_x = ((cells_x * CellSize) - BoundsWidth) / 2;
            bleed_y = ((cells_y * CellSize) - BoundsHeight) / 2;
            Gradient = GetGradient();
		}

		private int[] getGradientColors()
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

		private IManagedBitmap GetGradient()
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
					                          BoundsWidth,
					                          BoundsHeight,
					                          colorArray,
					                          null,
					                          Shader.TileMode.Repeat
				                          );
				    break;
			    case 1:
				    gradientShader = new RadialGradient (
					                            ((float)BoundsWidth / 2),
					                            ((float)BoundsHeight / 2),
					                            ((float)BoundsWidth / 2),
					                            colorArray,
					                            null,
					                            Shader.TileMode.Clamp
				                            );
				    break;
               case 2:
                        gradientShader = new SweepGradient(
                            ((float)BoundsWidth / 2),
                            ((float)BoundsHeight / 2),
                            colorArray,
                            null
                        );
                        break;
              default:
				    gradientShader = new LinearGradient (
					    0,
					    0,
					    BoundsWidth,
					    BoundsHeight,
					    colorArray,
					    null,
					    Shader.TileMode.Repeat
				    );
				    break;
			}

		    var bmp = ReuseableBitmapPool.getBitmap();

            
			_paint.SetStyle (Paint.Style.Fill);
		    var oldShader = _paint.Shader;
			_paint.SetShader (gradientShader);
		    using (Canvas canvas = new Canvas(bmp.GetBitmap()))
		    {
		        canvas.DrawRect(0, 0, BoundsWidth, BoundsHeight, _paint);
            }
			
		    _paint.SetShader(oldShader);
			return bmp;
		}

		private List<DelaunayTriangulator.Vertex> GeneratePoints()
		{
            var rand = new System.Random();
            var points = new List<DelaunayTriangulator.Vertex>();
			for (var i = - bleed_x; i < BoundsWidth + bleed_x; i += CellSize) 
			{
				for (var j = - bleed_y; j < BoundsHeight + bleed_y; j += CellSize) 
				{
					var x = i + CellSize/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					var y = j + CellSize/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					points.Add(new DelaunayTriangulator.Vertex((float)Math.Floor(x),(float)Math.Floor(y)));
				}
			}
			return points;
		}

		private double _map(double num, int[] in_range, double[] out_range)
		{
			return (num - in_range[0]) * (out_range[1] - out_range[0]) / (in_range[1] - in_range[0]) + out_range[0];
		}
        
	}
}

