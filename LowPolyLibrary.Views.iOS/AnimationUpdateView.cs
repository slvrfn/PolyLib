using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using SkiaSharp.Views.iOS;
using UIKit;
using LowPolyLibrary.Animation;
using SkiaSharp;


namespace LowPolyLibrary.Views.iOS
{
    public class AnimationUpdateView : SKCanvasView, IAnimationUpdateView
    {
        private AnimationEngine _animationFlowEngine;

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
#endregion

        void Initialize()
        {
            _animationFlowEngine = new LowPolyLibrary.Animation.AnimationEngine(this);
        }

        public void DrawOnMe(SKSurface surf)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            _animationFlowEngine.DrawOnMe(surf);
            Console.WriteLine("Animation Frame drawn in: " + watch.ElapsedMilliseconds + " ms\n");
        }

        public void SignalRedraw()
        {
            SetNeedsDisplay();
        }

        public void AddAnimation(AnimationBase anim)
        {
            _animationFlowEngine.AddAnimation(anim);
        }
    }
}