using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using SkiaSharp;
using SkiaSharp.Views.Android;
using LowPolyLibrary.Animation;

namespace LowPolyLibrary
{
    public class TriangulationView : SKCanvasView
    {
        public LowPolyLibrary.Triangulation Triangulation { get; private set; }

        float Variance = .75f;
        int CellSize = 150;

        #region Constructors

        public TriangulationView(Context context) : base(context)
        {
            Initialize();
        }

        public TriangulationView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public TriangulationView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

#endregion

        private void Initialize()
        {
            ViewTreeObserver.AddOnGlobalLayoutListener(new GlobalLayoutListener((obj) =>
            {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(obj);
                Triangulation = new LowPolyLibrary.Triangulation(Width, Height, Variance, CellSize);
                Invalidate();
            }));
        }

        protected override void OnDraw(SKSurface surface, SKImageInfo info)
        {
            base.OnDraw(surface, info);
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            if(Triangulation != null)
            {
                Triangulation.GeneratedBitmap(surface);
                Console.WriteLine("Triangulation drawn in: " + watch.ElapsedMilliseconds + " ms\n");
            }
            watch.Stop();
        }

        public void Generate(int boundsWidth, int boundsHeight, float variance, int cellSize)
        {
            Variance = variance;
            CellSize = cellSize;

            Triangulation = new LowPolyLibrary.Triangulation(boundsWidth, boundsHeight, Variance, CellSize);
            Invalidate();
        }
    }
}