using System.Collections.Generic;
using Triangulator = DelaunayTriangulator.Triangulator;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Enum = System.Enum;
using Math = System.Math;
using System;
using SkiaSharp;

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
        internal SKSurface Gradient;
		internal List<DelaunayTriangulator.Vertex> InternalPoints;

	    public List<Triad> TriangulatedPoints;

        public Triangulation(int boundsWidth, int boundsHeight, double variance, double cellSize)
        {
            BoundsWidth = boundsWidth;
            BoundsHeight = boundsHeight;
            Variance = variance;
            CellSize = cellSize;
            var info = new SKImageInfo(boundsWidth, boundsHeight);
            UpdateVars(info);
			InternalPoints = GeneratePoints();
		    var angulator = new Triangulator();
		    TriangulatedPoints = angulator.Triangulation(InternalPoints);
		}

        //~Triangulation()
        //{
        //    Gradient.Recycle();
        //    Gradient.Dispose();
        //}

	    public void GeneratedBitmap(SKSurface surface)
	    {
	        DrawFrame(surface);
        }

		private void DrawFrame(SKSurface surface)
		{
		    using (var canvas = surface.Canvas)
		    {
		        using (var paint = new SKPaint())
		        {
		            paint.Style = SKPaintStyle.StrokeAndFill;
		            paint.IsAntialias = true;

		            for (int i = 0; i < TriangulatedPoints.Count; i++)
		            {
		                var a = new SKPoint(InternalPoints[TriangulatedPoints[i].a].x, InternalPoints[TriangulatedPoints[i].a].y);
		                var b = new SKPoint(InternalPoints[TriangulatedPoints[i].b].x, InternalPoints[TriangulatedPoints[i].b].y);
		                var c = new SKPoint(InternalPoints[TriangulatedPoints[i].c].x, InternalPoints[TriangulatedPoints[i].c].y);

		                var center = Geometry.centroid(TriangulatedPoints[i], InternalPoints);

		                var triAngleColorCenter = Geometry.KeepInPicBounds(center, bleed_x, bleed_y, BoundsWidth, BoundsHeight);
		                paint.Color = Geometry.GetTriangleColor(Gradient, triAngleColorCenter);

		                using (var trianglePath = Geometry.DrawTrianglePath(a, b, c))
		                {
		                    canvas.DrawPath(trianglePath, paint);
		                }
		            }
		        }
            }
        }

        private void UpdateVars(SKImageInfo info)
        {
            calcVariance = CellSize * Variance / 2;
            cells_x = Math.Floor((BoundsWidth + 4 * CellSize) / CellSize);
            cells_y = Math.Floor((BoundsHeight + 4 * CellSize) / CellSize);
            bleed_x = ((cells_x * CellSize) - BoundsWidth) / 2;
            bleed_y = ((cells_y * CellSize) - BoundsHeight) / 2;
            Gradient = GetGradient(info);
		}

		private SKColor[] getGradientColors()
		{
            var rand = new System.Random();
            //get all gradient codes
            var values = Enum.GetValues(typeof(ColorBru.Code));
			ColorBru.Code randomCode = (ColorBru.Code)values.GetValue(rand.Next(values.Length));
			//gets specified colors in gradient length: #
			var brewColors = ColorBru.GetHtmlCodes (randomCode, 6);
			//array of ints converted from brewColors
			var colorArray = new SKColor[brewColors.Length];
			for (int i = 0; i < brewColors.Length; i++) {
				colorArray[i] = SKColor.Parse(brewColors[i]);
			}
			return colorArray;
		}

		private SKSurface GetGradient(SKImageInfo info)
		{
            var rand = new System.Random();
            var colorArray = getGradientColors ();

			SKShader gradientShader;
            //set to 2, bc want to temporarily not make sweep gradient
			switch (rand.Next(2)) {
			    case 0:
				    gradientShader = SKShader.CreateLinearGradient (
					                          new SKPoint(0,0),
					                          new SKPoint(BoundsWidth, BoundsHeight),
					                          colorArray,
					                          null,
					                          SKShaderTileMode.Repeat
				                          );
				    break;
			    case 1:
				    gradientShader = SKShader.CreateRadialGradient (
					                            new SKPoint(BoundsWidth/2, BoundsHeight/2),
					                            ((float)BoundsWidth / 2),
					                            colorArray,
					                            null,
					                            SKShaderTileMode.Clamp
				                            );
				    break;
               case 2:
                    gradientShader = SKShader.CreateSweepGradient(
                    new SKPoint(BoundsWidth / 2, BoundsHeight / 2),
                            colorArray,
                            null
                        );
                        break;
              default:
					gradientShader = SKShader.CreateLinearGradient(
											  new SKPoint(0, 0),
											  new SKPoint(BoundsWidth, BoundsHeight),
											  colorArray,
											  null,
											  SKShaderTileMode.Repeat
										  );
				    break;
			}
		    var bmp = SKSurface.Create(info);
		    using (var paint = new SKPaint())
		    {
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = true;

		        var oldShader = paint.Shader;
                paint.Shader = gradientShader;

                using (var canvas = bmp.Canvas)
		        {
                    var r = new SKRect();
                    r.Top = 0;
                    r.Left = 0;
                    r.Right = BoundsWidth;
                    r.Bottom = BoundsHeight;
		            canvas.DrawRect(r, paint);
		        }

                paint.Shader = oldShader;
            }

		    
		    
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

