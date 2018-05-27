using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;
using PolyLib;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace PolyLib.Views.iOS
{
    [Register("TriangulationView"), DesignTimeVisible(true)]
    public class TriangulationView : SKCanvasView
    {
        public Triangulation Triangulation { get; private set; }

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
            //converting to pixels for library's use

            Triangulation = new Triangulation(
                (int)(Frame.Size.Width * UIScreen.MainScreen.Scale),
                (int)(Frame.Size.Height * UIScreen.MainScreen.Scale));
            Triangulation.PropertyChanged += Triangulation_PropertyChanged;
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

            SetNeedsDisplay();
        }

        void Triangulation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //force redraw of entire polylibview
            Superview.SetNeedsDisplay();
            SetNeedsDisplay();
        }
    }
}