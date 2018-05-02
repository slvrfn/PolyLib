using System.Collections.Generic;
using Triangulator = DelaunayTriangulator.Triangulator;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Enum = System.Enum;
using Math = System.Math;
using System;
using DelaunayTriangulator;
using SkiaSharp;
using System.Linq;
using WEB;

namespace LowPolyLibrary
{
	public class Triangulation
	{
        internal int BoundsWidth;
		internal int BoundsHeight;
		internal float CellSize = 150;
		public float Variance = .75f;
        private float calcVariance;
		internal float bleed_x, bleed_y;
        internal SKSurface Gradient;
		internal readonly List<DelaunayTriangulator.Vertex> InternalPoints;
        
	    internal Dictionary<Vertex, HashSet<Triad>> pointToTriangleDic = null;

        public readonly List<Triad> TriangulatedPoints;

        //used to speed up color access time from gradient
	    SKImageInfo readColorImageInfo;
	    SKBitmap readColorBitmap;
	    IntPtr pixelBuffer;

        FastNoise fastNoise;

	    private readonly SKPaint strokePaint, fillPaint;

        //reuseable variables to speed up operations
	    private SKPoint _pathPointA;
	    private SKPoint _pathPointB;
	    private SKPoint _pathPointC;
	    private SKPoint _center;
	    private SKPath _trianglePath;

        public Triangulation(int boundsWidth, int boundsHeight, float variance, float cellSize, float frequency, float seed)
        {
            BoundsWidth = boundsWidth;
            BoundsHeight = boundsHeight;
            Variance = variance;
            CellSize = cellSize;
            var info = new SKImageInfo(boundsWidth, boundsHeight);
            UpdateVars(info);
            fastNoise = new FastNoise((int)seed);
            fastNoise.SetFrequency(frequency);
            fastNoise.SetNoiseType(FastNoise.NoiseType.Perlin);

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

            strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                IsAntialias = true
            };

            //color set later
            fillPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            };

            _trianglePath = new SKPath {FillType = SKPathFillType.EvenOdd};
        }

        ~Triangulation()
        {
            //need to release bitmap
            readColorBitmap.Dispose();
            Gradient.Dispose();
            strokePaint.Dispose();
            fillPaint.Dispose();
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

		        foreach (Triad tri in TriangulatedPoints)
		        {
		            _pathPointA.X = InternalPoints[tri.a].x;
		            _pathPointA.Y = InternalPoints[tri.a].y;
		            _pathPointB.X = InternalPoints[tri.b].x;
		            _pathPointB.Y = InternalPoints[tri.b].y;
		            _pathPointC.X = InternalPoints[tri.c].x;
		            _pathPointC.Y = InternalPoints[tri.c].y;

		            Geometry.centroid(tri, InternalPoints, ref _center);

		            KeepInBounds(ref _center);
		            fillPaint.Color = GetTriangleColor(_center);
		            strokePaint.Color = fillPaint.Color;
		            Geometry.DrawTrianglePath(ref _trianglePath, _pathPointA, _pathPointB, _pathPointC);
		            canvas.DrawPath(_trianglePath, fillPaint);
		            canvas.DrawPath(_trianglePath, strokePaint);
		        }
		    }
        }

        private void UpdateVars(SKImageInfo info)
        {
            calcVariance = CellSize * Variance / 2;
            bleed_x =  BoundsWidth / 3;
            bleed_y = BoundsHeight / 3;
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

	    internal void KeepInBounds(ref SKPoint center)
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
	    }

        private SKColor[] getGradientColors()
		{
            //get all gradient codes
            var values = Enum.GetValues(typeof(ColorBru.Code));
			ColorBru.Code randomCode = (ColorBru.Code)values.GetValue(Random.Rand.Next(values.Length));
			//gets specified colors in gradient length: #
            while (!ColorBru.Palettes.Single(c => c.Code == randomCode).HtmlCodes.Any(c => c.Length == 6)){
                randomCode = (ColorBru.Code)values.GetValue(Random.Rand.Next(values.Length));
            }

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
            // avoid duplicate points
            var points = new HashSet<DelaunayTriangulator.Vertex>();
            // range of perlin noise is sqrt(dimension)/2
            // https://stackoverflow.com/a/18263038/3344317
            // 2D
            var in_range = new float[] { -.7071f, .7071f };
            // 3d
             //var in_range = new float[] { -.866f, .866f };
            var variance = new float[] {-calcVariance, calcVariance};

            for (float i = -bleed_x; i < BoundsWidth + bleed_x; i += CellSize) 
            {
              for (float j = -bleed_y; j < BoundsHeight + bleed_y; j += CellSize) 
              {
                    var noiseX = fastNoise.GetNoise(i, j);
                    var noiseY = fastNoise.GetNoise(j, i);

                    var x = i + Geometry.ConvertBetweenRanges(noiseX, in_range[0], in_range[1], variance[0], variance[1]);
                    var y = j + Geometry.ConvertBetweenRanges(noiseY, in_range[0], in_range[1], variance[0], variance[1]);
                    points.Add(new DelaunayTriangulator.Vertex(x, y));
              }
            }

			return points.ToList();
		}
        
	}
}

