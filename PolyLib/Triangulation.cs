using System.Collections.Generic;
using DelaunayTriangulator;
using System;
using SkiaSharp;
using System.Linq;
using WEB;
using System.ComponentModel;

namespace PolyLib
{
    public class Triangulation : INotifyPropertyChanged
    {
        //allow triangulation views to update when a property is changed
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly int BoundsWidth;
        public readonly int BoundsHeight;

        private Triangulator _angulator;

        private bool _pointsDirty = false;
        private bool _gradientDirty = false;

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
                MarkPointsDirty();
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
                MarkPointsDirty();
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
                MarkPointsDirty();
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
                MarkPointsDirty();
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
                _calcVariance = _cellSize * Variance;
                MarkPointsDirty();
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
                _calcVariance = CellSize * _variance;
                MarkPointsDirty();
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

        private SKColor _strokeColor = SKColors.Black; 
 
        public SKColor StrokeColor 
        { 
            get => _strokeColor; 
            set
            {
                if (_gradientShader.Equals(value))
                    return;
                _strokeColor = value;
                _strokePaint.Color = _strokeColor;
            }
        } 

        public SKShader GradientShader
        {
            get => _gradientShader;
            set
            {
                if (_gradientShader.Equals(value))
                    return;
                _gradientShader = value;
                MarkGradientDirty();
                OnPropertyChanged("GradientShader");
            }
        }

        public List<Vertex> Points 
        {
            get => _internalPoints.Select((arg) => arg.Clone()).ToList();
        }

        //allow animations to use original list for speed
        internal List<Vertex> InternalPoints
        {
            get => _internalPoints;
        }

        public List<Triad> TriangulatedPoints
        {
            get => _triangulatedPoints.Select((arg) => arg.Clone()).ToList();
        }

        //allow animations to use original list for speed
        internal List<Triad> InternalTriangulatedPoints
        {
            get => _triangulatedPoints;
        }

        public Dictionary<Vertex, HashSet<Triad>> PointToTriangleDic
        {
            get
            {
                Dictionary<Vertex, HashSet<Triad>> newDic = new Dictionary<Vertex, HashSet<Triad>>(_pointToTriangleDic.Count, _pointToTriangleDic.Comparer);
                foreach (var entry in _pointToTriangleDic)
                {
                    //clone key
                    //create new hashset, and clone each of its values
                    newDic.Add(entry.Key.Clone(), new HashSet<Triad>(entry.Value.Select(arg => arg.Clone())));
                }
                return newDic;
            }
        }

        //allow animations to use original dict for speed
        internal Dictionary<Vertex, HashSet<Triad>> InternalPointToTriangleDic
        {
            get => _pointToTriangleDic;
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
        private List<DelaunayTriangulator.Vertex> _internalPoints;
        private Dictionary<Vertex, HashSet<Triad>> _pointToTriangleDic = null;
        private List<Triad> _triangulatedPoints;

        //randomly seed the triangulation from the start
        private float _seed = Guid.NewGuid().GetHashCode();
        private float _frequency = .01f;

        private SKShader _gradientShader = SKShader.CreateEmpty();

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

        public Triangulation(int boundsWidth, int boundsHeight) : this(boundsWidth, boundsHeight, null, null) { }
        public Triangulation(int boundsWidth, int boundsHeight, SKColor[] gradientColors) : this(boundsWidth, boundsHeight, gradientColors, null) { }
        public Triangulation(int boundsWidth, int boundsHeight, SKShader gradientShader) : this(boundsWidth, boundsHeight, null, gradientShader) { }

        private Triangulation(int boundsWidth, int boundsHeight, SKColor[] gradientColors, SKShader gradientShader)
        {
            BoundsWidth = boundsWidth;
            BoundsHeight = boundsHeight;

            //use colors provided by user for gradient. If none provided, get some random colors
            var colorsForGradient = gradientColors == null ? getRandomColorBruColors(6) : gradientColors;
            //use gradient shader provided by user for gradient. If none provided, get some random shader
            GradientShader = gradientShader == null ? GetRandomGradientShader(colorsForGradient, BoundsWidth, BoundsHeight) : gradientShader;

            var info = new SKImageInfo(boundsWidth, boundsHeight);
            _gradient = GetGradient(info, GradientShader);

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
                Color = _strokeColor,
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
            if (_gradientDirty)
            {
                var info = new SKImageInfo(BoundsWidth, BoundsHeight);
                _gradient = GetGradient(info, GradientShader);
                _gradientDirty = false;
            }

            using (var canvas = surface.Canvas)
            {
                canvas.Clear();

                foreach (Triad tri in _triangulatedPoints)
                {
                    _pathPointA.X = _internalPoints[tri.a].x;
                    _pathPointA.Y = _internalPoints[tri.a].y;
                    _pathPointB.X = _internalPoints[tri.b].x;
                    _pathPointB.Y = _internalPoints[tri.b].y;
                    _pathPointC.X = _internalPoints[tri.c].x;
                    _pathPointC.Y = _internalPoints[tri.c].y;

                    Geometry.centroid(tri, _internalPoints, ref _center);

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
            return _pointToTriangleDic != null;
        }

        internal void SetupPointsToTriangles()
        {
            _pointToTriangleDic = new Dictionary<Vertex, HashSet<Triad>>();
            divyTris(_internalPoints);
        }

        private void divyTris(List<Vertex> points)
        {
            void divyTris(Vertex point, int arrayLoc)
            {
                //if the point/triList distionary has a point already, add that triangle to the list at that key(point)
                if (_pointToTriangleDic.ContainsKey(point))
                    _pointToTriangleDic[point].Add(_triangulatedPoints[arrayLoc]);
                //if the point/triList distionary doesnt not have a point, initialize it, and add that triangle to the list at that key(point)
                else
                {
                    _pointToTriangleDic[point] = new HashSet<Triad> { _triangulatedPoints[arrayLoc] };
                }
            }


            for (var i = 0; i < _triangulatedPoints.Count; i++)
            {
                //animation logic
                divyTris(points[_triangulatedPoints[i].a], i);
                divyTris(points[_triangulatedPoints[i].b], i);
                divyTris(points[_triangulatedPoints[i].c], i);
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

        public void KeepInBounds(ref SKPoint center)
        {
            if (center.X < 0)
                center.X = 0;
            else if (center.X > BoundsWidth)
                center.X = BoundsWidth;
            //else if (center.X.Equals(BoundsWidth))
            //center.X -= (int)BleedX - 1;
            if (center.Y < 0)
                center.Y = 0;
            else if (center.Y > BoundsHeight)
                center.Y = BoundsHeight;
            //else if (center.Y.Equals(BoundsHeight))
                //center.Y -= (int)BleedY - 1;
        }

        public static SKColor[] getRandomColorBruColors(int colorCount)
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

        public static SKShader GetRandomGradientShader(SKColor[] colorArray, int boundsWidth, int boundsHeight)
        {
            SKShader gradientShader;
            //set to 2, bc want to temporarily not make sweep gradient
            switch (Random.Rand.Next(2))
            {
                case 0:
                    gradientShader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(boundsWidth, boundsHeight),
                        colorArray,
                        null,
                        SKShaderTileMode.Repeat
                    );
                    break;
                case 1:
                    gradientShader = SKShader.CreateRadialGradient(
                        new SKPoint(boundsWidth / 2, boundsHeight / 2),
                        ((float)boundsWidth / 2),
                        colorArray,
                        null,
                        SKShaderTileMode.Clamp
                    );
                    break;
                case 2:
                    gradientShader = SKShader.CreateSweepGradient(
                        new SKPoint(boundsWidth / 2, boundsHeight / 2),
                        colorArray,
                        null
                    );
                    break;
                default:
                    gradientShader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(boundsWidth, boundsHeight),
                        colorArray,
                        null,
                        SKShaderTileMode.Repeat
                    );
                    break;
            }

            return gradientShader;
        }

        private SKSurface GetGradient(SKImageInfo info, SKShader gradientShader)
        {
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

        //used to allow the points to be regenerated on the next frame draw
        private void MarkGradientDirty()
        {
            //lock object to avoid race conditions?
            _gradientDirty = true;
        }

        private void GeneratePoints()
        {
            _internalPoints = GenerateUntriangulatedPoints();
            _triangulatedPoints = _angulator.Triangulation(_internalPoints);
            //allow this to be recreated
            _pointToTriangleDic = null;
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}

