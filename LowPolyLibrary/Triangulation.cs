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
using System.ComponentModel;

namespace LowPolyLibrary
{
    public class Triangulation : INotifyPropertyChanged
    {
        //allow triangulation views to update when a property is changed
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly int BoundsWidth;
        public readonly int BoundsHeight;

        internal List<DelaunayTriangulator.Vertex> InternalPoints;

        public Dictionary<Vertex, HashSet<Triad>> pointToTriangleDic = null;

        public List<Triad> TriangulatedPoints;

        private Triangulator _angulator;

        private bool _pointsDirty = false;

        #region Triangulation Properties
        public float Seed
        {
            get => _seed;
            set
            {
                if (_seed.Equals(value))
                    return;
                _seed = value;
                fastNoise.SetSeed((int)_seed);
                OnPropertyChanged("Seed");
            }
        }

        public float Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency.Equals(value))
                    return;
                _frequency = value;
                fastNoise.SetFrequency(Frequency);
                OnPropertyChanged("Frequency");
            }
        }

        public float BleedY
        {
            get => _bleedY;
            set
            {
                if (_bleedY.Equals(value))
                    return;
                _bleedY = value;
                OnPropertyChanged("BleedY");
            }
        }
        public float BleedX
        {
            get => _bleedX;
            set
            {
                if (_bleedX.Equals(value))
                    return;
                _bleedX = value;
                OnPropertyChanged("BleedX");
            }
        }
        public float CellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize.Equals(value))
                    return;
                _cellSize = value;
                _calcVariance = _cellSize * Variance / 2;
                OnPropertyChanged("CellSize");
            }
        }
        public float Variance
        {
            get => _variance;
            set
            {
                if (_variance.Equals(value))
                    return;
                _variance = value;
                _calcVariance = CellSize * _variance / 2;
                OnPropertyChanged("Variance");
            }
        }
        public bool HideLines
        {
            get => _hideLines;
            set
            {
                if (_hideLines.Equals(value))
                    return;
                _hideLines = value;
                OnPropertyChanged("HideLines");
            }
        }

        public SKColor[] GradientColors
        {
            get => _gradientColors;
            set
            {
                if (_gradientColors.Equals(value))
                    return;
                _gradientColors = value;
                var info = new SKImageInfo(BoundsWidth, BoundsHeight);
                _gradient = GetGradient(info, GradientColors);
                OnPropertyChanged("GradientColors");
            }
        }
        #endregion

        #region Private variables
        //default values
        private float _cellSize = 150;
        private float _variance = .75f;
        private float _calcVariance;
        private float _bleedX;
        private float _bleedY;
        private bool _hideLines = false;
        //randomly seed the triangulation from the start
        private float _seed = Guid.NewGuid().GetHashCode();
        private float _frequency = .01f;
        private SKColor[] _gradientColors = new SKColor[0];
        private bool gradientSetByUser = false;

        private SKSurface _gradient;
        //used to speed up color access time from gradient
        private SKImageInfo _readColorImageInfo;
        private SKBitmap _readColorBitmap;
        private IntPtr _pixelBuffer;

        //noise generator
        private FastNoise fastNoise;

        //paints for drawing
        private readonly SKPaint _strokePaint, _fillPaint;

        //reuseable variables to speed up operations
        private SKPoint _pathPointA;
        private SKPoint _pathPointB;
        private SKPoint _pathPointC;
        private SKPoint _center;
        private SKPath _trianglePath;
        #endregion

        public Triangulation(int boundsWidth, int boundsHeight, SKColor[] gradientColors = null)
        {
            BoundsWidth = boundsWidth;
            BoundsHeight = boundsHeight;

            //use colors provided by user for gradient. If none provided, get some random colors
            GradientColors = gradientColors == null ? getRandomColorBruColors(6) : gradientColors;

            _calcVariance = CellSize * Variance / 2;
            // how the bleeds are initially set
            BleedX = BoundsWidth / 3;
            BleedY = BoundsHeight / 3;

            fastNoise = new FastNoise((int)Seed);
            fastNoise.SetFrequency(Frequency);
            fastNoise.SetNoiseType(FastNoise.NoiseType.Perlin);

            _angulator = new Triangulator();
            GeneratePoints();

            //https://forums.xamarin.com/discussion/92899/read-a-pixel-info-from-a-canvas

            _readColorImageInfo = new SKImageInfo();
            _readColorImageInfo.ColorType = SKColorType.Argb4444;
            _readColorImageInfo.AlphaType = SKAlphaType.Premul;

            _readColorImageInfo.Width = 1;
            _readColorImageInfo.Height = 1;

            // create the 1x1 bitmap (auto allocates the pixel buffer)
            _readColorBitmap = new SKBitmap(_readColorImageInfo);
            _readColorBitmap.LockPixels();
            // get the pixel buffer for the bitmap

            _pixelBuffer = _readColorBitmap.GetPixels();

            _strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                IsAntialias = true
            };

            //color set later
            _fillPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            };

            _trianglePath = new SKPath { FillType = SKPathFillType.EvenOdd };
        }

        ~Triangulation()
        {
            //need to release bitmap
            _readColorBitmap.Dispose();
            _gradient.Dispose();
            _strokePaint.Dispose();
            _fillPaint.Dispose();
        }

        public void DrawFrame(SKSurface surface)
        {
            if (_pointsDirty)
            {
                GeneratePoints();
                _pointsDirty = false;
            }


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
                    _fillPaint.Color = GetTriangleColor(_center);

                    Geometry.DrawTrianglePath(ref _trianglePath, _pathPointA, _pathPointB, _pathPointC);
                    canvas.DrawPath(_trianglePath, _fillPaint);
                    if (HideLines)
                    {
                        //need to maintain the strokepaint reguardless if we are just hiding its display
                        var backup = _strokePaint.Color;
                        _strokePaint.Color = _fillPaint.Color;
                        canvas.DrawPath(_trianglePath, _strokePaint);
                        _strokePaint.Color = backup;
                    }
                    else
                    {
                        canvas.DrawPath(_trianglePath, _strokePaint);
                    }

                }
            }
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
                pointToTriangleDic[point] = new HashSet<Triad> { TriangulatedPoints[arrayLoc] };
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
            _gradient.ReadPixels(_readColorImageInfo, _pixelBuffer, _readColorImageInfo.RowBytes, (int)center.X, (int)center.Y);

            // access the color
            return _readColorBitmap.GetPixel(0, 0);
        }

        internal void KeepInBounds(ref SKPoint center)
        {
            if (center.X < 0)
                center.X += (int)BleedX;
            else if (center.X > BoundsWidth)
                center.X -= (int)BleedX;
            else if (center.X.Equals(BoundsWidth))
                center.X -= (int)BleedX - 1;
            if (center.Y < 0)
                center.Y += (int)BleedY;
            else if (center.Y > BoundsHeight)
                center.Y -= (int)BleedY + 1;
            else if (center.Y.Equals(BoundsHeight))
                center.Y -= (int)BleedY - 1;
        }

        private SKColor[] getRandomColorBruColors(int colorCount)
        {
            //get all gradient codes
            var values = Enum.GetValues(typeof(ColorBru.Code));
            ColorBru.Code randomCode = (ColorBru.Code)values.GetValue(Random.Rand.Next(values.Length));
            //gets specified colors in gradient length: #
            //not all ColorBru.Code(s) have HtmlCodes with the desired length
            while (!ColorBru.Palettes.Single(c => c.Code == randomCode).HtmlCodes.Any(c => c.Length == colorCount))
            {
                randomCode = (ColorBru.Code)values.GetValue(Random.Rand.Next(values.Length));
            }

            var brewColors = ColorBru.GetHtmlCodes(randomCode, (byte)colorCount);
            //array of ints converted from brewColors
            var colorArray = new SKColor[brewColors.Length];
            for (int i = 0; i < brewColors.Length; i++)
            {
                colorArray[i] = SKColor.Parse(brewColors[i]);
            }
            return colorArray;
        }

        private SKSurface GetGradient(SKImageInfo info, SKColor[] colorArray)
        {
            SKShader gradientShader;
            //set to 2, bc want to temporarily not make sweep gradient
            switch (Random.Rand.Next(2))
            {
                case 0:
                    gradientShader = SKShader.CreateLinearGradient(
                                              new SKPoint(0, 0),
                                              new SKPoint(BoundsWidth, BoundsHeight),
                                              colorArray,
                                              null,
                                              SKShaderTileMode.Repeat
                                          );
                    break;
                case 1:
                    gradientShader = SKShader.CreateRadialGradient(
                                                new SKPoint(BoundsWidth / 2, BoundsHeight / 2),
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

        //used to allow the points to be regenerated on the next frame draw
        private void MarkPointsDirty()
        {
            //lock object to avoid race conditions?
            _pointsDirty = true;
        }

        public void GeneratePoints()
        {
            InternalPoints = GenerateUntriangulatedPoints();
            TriangulatedPoints = _angulator.Triangulation(InternalPoints);
            //allow this to be recreated
            pointToTriangleDic = null;
        }

        private List<DelaunayTriangulator.Vertex> GenerateUntriangulatedPoints()
        {
            // avoid duplicate points
            var points = new HashSet<DelaunayTriangulator.Vertex>();
            // range of perlin noise is sqrt(dimension)/2
            // https://stackoverflow.com/a/18263038/3344317
            // 2D
            var in_range = new float[] { -.7071f, .7071f };
            // 3d
            //var in_range = new float[] { -.866f, .866f };
            var variance = new float[] { -_calcVariance, _calcVariance };

            for (float i = -BleedX; i < BoundsWidth + BleedX; i += CellSize)
            {
                for (float j = -BleedY; j < BoundsHeight + BleedY; j += CellSize)
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

        protected void OnPropertyChanged(string name)
        {
            //here bc this covers all property changes
            MarkPointsDirty();
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}

