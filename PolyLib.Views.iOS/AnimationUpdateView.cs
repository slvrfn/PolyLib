using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using SkiaSharp.Views.iOS;
using UIKit;
using PolyLib.Animation;
using SkiaSharp;


namespace PolyLib.Views.iOS
{
    [Register("AnimationUpdateView"), DesignTimeVisible(true)]
    public class AnimationUpdateView : SKCanvasView, IAnimationUpdateView
    {
        public AnimationEngine Engine { get; private set; }

        #region Constructors
        public AnimationUpdateView()
        {
            Initialize();
        }

        public AnimationUpdateView(CGRect frame) : base(frame)
        {
            Initialize();
        }

        public AnimationUpdateView(IntPtr p) : base(p)
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
            Engine = new AnimationEngine(this);
            this.Opaque = false;
        }

        public override void DrawInSurface(SKSurface surface, SKImageInfo info)
        {
            base.DrawInSurface(surface, info);
            DrawOnMe(surface);
        }

        public void DrawOnMe(SKSurface surf)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Engine.DrawOnMe(surf);
            Console.WriteLine("Animation Frame drawn in: " + watch.ElapsedMilliseconds + " ms\n");
        }

        public void SignalRedraw()
        {
            SetNeedsDisplay();
        }

        public void AddAnimation(AnimationBase anim)
        {
            Engine.AddAnimation(anim);
        }
    }
}