using System;
using System.ComponentModel;
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
    [Register("TriangulationView"), DesignTimeVisible(true)]
    public class TriangulationView : SKCanvasView
    {
        public LowPolyLibrary.Triangulation Triangulation { get; private set; }

        float _variance = .75f;
        int _cellSize = 150;
        float _frequency = .01f;
        float _seed = 0;

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

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            // Called when loaded from xib or storyboard.
            Initialize();
        }
#endregion

        void Initialize()
        {


            //Triangulation = new LowPolyLibrary.Triangulation(1920, 1080, Variance, CellSize);

            Triangulation = new LowPolyLibrary.Triangulation(
                (int)(Frame.Size.Width * UIScreen.MainScreen.Scale),
                (int)(Frame.Size.Height * UIScreen.MainScreen.Scale))
            {
                Variance = _variance,
                CellSize = _cellSize,
                Frequency = _frequency,
                Seed = _seed
            };

            //SetNeedsDisplay();
        }

        public override void DrawInSurface(SKSurface surface, SKImageInfo info)
        {
            base.DrawInSurface(surface, info);
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            if (Triangulation != null)
            {
                Triangulation.DrawFrame(surface);
                Console.WriteLine("Triangulation drawn in: " + watch.ElapsedMilliseconds + " ms\n");
            }
            watch.Stop();
        }



        public void Generate(int boundsWidth, int boundsHeight, float variance, int cellSize, float frequency, float seed)
        {
            _variance = variance;
            _cellSize = cellSize;
            _frequency = frequency;
            _seed = seed;

            Triangulation = new LowPolyLibrary.Triangulation(boundsWidth, boundsHeight)
            {
                Variance = _variance,
                CellSize = _cellSize,
                Frequency = frequency,
                Seed = seed
            };
            SetNeedsDisplay();
        }
    }
}