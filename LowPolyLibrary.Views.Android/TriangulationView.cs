using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using SkiaSharp;
using SkiaSharp.Views.Android;
using LowPolyLibrary.Animation;
using LowPolyLibrary.Views;

namespace LowPolyLibrary.Views.Android
{
    public class TriangulationView : SKCanvasView
    {
        public LowPolyLibrary.Triangulation Triangulation { get; private set; }
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

        ~TriangulationView()
        {
            Triangulation.PropertyChanged -= Triangulation_PropertyChanged;
        }

        #endregion

        private void Initialize()
        {
            ViewTreeObserver.AddOnGlobalLayoutListener(new GlobalLayoutListener((obj) =>
            {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(obj);
                // Generate simple Triangulation for initial draw
                Triangulation = new LowPolyLibrary.Triangulation(Width, Height);
                Triangulation.PropertyChanged += Triangulation_PropertyChanged;
                Invalidate();
            }));
        }

        protected override void OnDraw(SKSurface surface, SKImageInfo info)
        {
            base.OnDraw(surface, info);
            if (Triangulation != null)
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();

                Triangulation.DrawFrame(surface);
                Console.WriteLine("Triangulation drawn in: " + watch.ElapsedMilliseconds + " ms\n");

                watch.Stop();
            }
        }

        public void UpdateTriangulation(Triangulation triangulation)
        {
            //no need to set this to null
            if (triangulation == null)
            {
                return;
            }

            //clear previous event
            if (Triangulation != null)
            {
                Triangulation.PropertyChanged -= Triangulation_PropertyChanged;
            }

            Triangulation = triangulation;
            Triangulation.PropertyChanged += Triangulation_PropertyChanged;

            Invalidate();
        }

        void Triangulation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //force redraw of entire lowpolyview
            ((View)Parent).Invalidate();
            Invalidate();
        }

    }
}