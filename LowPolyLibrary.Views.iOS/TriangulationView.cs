using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;
using LowPolyLibrary;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace LowPolyLibrary.Views.iOS
{
    public class TriangulationView : SKCanvasView
    {
        public LowPolyLibrary.Triangulation Triangulation { get; private set; }

        float Variance = .75f;
        int CellSize = 150;

#region Constructors
        public TriangulationView()
        {
            Initialize();
        }

        public TriangulationView(CGRect frame) : base(frame)
        {
            Initialize();
        }

        public TriangulationView(IntPtr p) : base(p)
        {
            Initialize();
        }
#endregion

        void Initialize()
        {
            Triangulation = new LowPolyLibrary.Triangulation((int)UIScreen.MainScreen.Bounds.Width, (int)UIScreen.MainScreen.Bounds.Height, Variance, CellSize);
        }

        public override void DrawInSurface(SKSurface surface, SKImageInfo info)
        {
            base.DrawInSurface(surface, info);
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            if (Triangulation != null)
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
            SetNeedsDisplay();
        }
    }
}