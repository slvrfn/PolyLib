using System.Collections.Generic;
using Triangulator = DelaunayTriangulator.Triangulator;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Enum = System.Enum;
using Math = System.Math;
using System;
using DelaunayTriangulator;
using Java.Util;
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
		internal readonly List<DelaunayTriangulator.Vertex> InternalPoints;
        
	    internal Dictionary<Vertex, HashSet<Triad>> pointToTriangleDic = null;

        public List<Triad> TriangulatedPoints;

        //used to speed up color access time from gradient
	    SKImageInfo readColorImageInfo;
	    SKBitmap readColorBitmap;
	    IntPtr pixelBuffer;

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

            //https://forums.xamarin.com/discussion/92899/read-a-pixel-info-from-a-canvas
            
            readColorImageInfo = new SKImageInfo();
            readColorImageInfo.ColorType = SKColorType.Argb4444;
            readColorImageInfo.AlphaType = SKAlphaType.Premul;

            readColorImageInfo.Width = 1;
            readColorImageInfo.Height = 1;

            // create the 1x1 bitmap (auto allocates the pixel buffer)
            readColorBitmap = new SKBitmap(readColorImageInfo);
            readColorBitmap.LockPixels();
            // get the pixel buffer for the bitmap
            
            pixelBuffer = readColorBitmap.GetPixels();
        }

        ~Triangulation()
        {
            //need to release bitmap
            readColorBitmap.Dispose();
            Gradient.Dispose();
        }

        public void GeneratedBitmap(SKSurface surface)
	    {
	        DrawFrame(surface);
        }

		private void DrawFrame(SKSurface surface)
		{
		    using (var canvas = surface.Canvas)
		    {
		        canvas.Clear();
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
		                paint.Color = GetTriangleColor(triAngleColorCenter);

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

	    internal bool HasPointsToTrianglesSetup()
	    {
	        return pointToTriangleDic != null;
	    }

	    internal void SetupPointsToTriangles()
	    {
	        pointToTriangleDic = new Dictionary<Vertex, HashSet<Triad>>();
            divyTris(InternalPoints);
        }

	    private void divyTris(Vertex point, int arrayLoc)
	    {
	        //if the point/triList distionary has a point already, add that triangle to the list at that key(point)
	        if (pointToTriangleDic.ContainsKey(point))
	            pointToTriangleDic[point].Add(TriangulatedPoints[arrayLoc]);
	        //if the point/triList distionary doesnt not have a point, initialize it, and add that triangle to the list at that key(point)
	        else
	        {
	            pointToTriangleDic[point] = new HashSet<Triad> {TriangulatedPoints[arrayLoc]};
	        }
	    }

	    internal void divyTris(List<Vertex> points)
	    {
	        for (int i = 0; i < TriangulatedPoints.Count; i++)
	        {
	            //animation logic
	            divyTris(points[TriangulatedPoints[i].a], i);
	            divyTris(points[TriangulatedPoints[i].b], i);
	            divyTris(points[TriangulatedPoints[i].c], i);
	        }
	    }

	    internal SKColor GetTriangleColor(SKPoint center)
	    {
            //center = KeepInPicBounds(center, bleed_x, bleed_y, BoundsWidth, BoundsHeight);

            // read the surface into the bitmap
	        Gradient.ReadPixels(readColorImageInfo, pixelBuffer, readColorImageInfo.RowBytes, (int)center.X, (int)center.Y);

	        // access the color
	        return readColorBitmap.GetPixel(0, 0);
        }

        private SKColor[] getGradientColors()
		{
            //get all gradient codes
            var values = Enum.GetValues(typeof(ColorBru.Code));
			ColorBru.Code randomCode = (ColorBru.Code)values.GetValue(Random.Rand.Next(values.Length));
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
            var colorArray = getGradientColors ();

			SKShader gradientShader;
            //set to 2, bc want to temporarily not make sweep gradient
			switch (Random.Rand.Next(2)) {
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
            var points = new List<DelaunayTriangulator.Vertex>();
			for (var i = - bleed_x; i < BoundsWidth + bleed_x; i += CellSize) 
			{
				for (var j = - bleed_y; j < BoundsHeight + bleed_y; j += CellSize) 
				{
					var x = i + CellSize/2 + _map(Random.Rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					var y = j + CellSize/2 + _map(Random.Rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
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

