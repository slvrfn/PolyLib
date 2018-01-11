using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using LowPolyLibrary.Animation;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace LowPolyLibrary.Views
{
    public class AnimationUpdateView : SKCanvasView
    {
        private LowPolyLibrary.Animation.AnimationEngine _animationFlowEngine;

#region Constructors

        public AnimationUpdateView(Context context) : base(context)
        {
            Initialize();
        }

        public AnimationUpdateView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public AnimationUpdateView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

#endregion

        private void Initialize()
        {
            _animationFlowEngine = new LowPolyLibrary.Animation.AnimationEngine(this);
        }

        protected override void OnDraw(SKSurface surface, SKImageInfo info)
        {
            base.OnDraw(surface, info);
            
            if (_animationFlowEngine.HasFrameToDraw)
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                _animationFlowEngine.DrawOnMe(surface);
                Console.WriteLine("Animation Frame drawn in: " + watch.ElapsedMilliseconds + " ms\n");
            }
        }

        public void AddAnimation(AnimationBase anim)
        {
            _animationFlowEngine.AddAnimation(anim);
        }
    }
}