using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SkiaSharp;
using SkiaSharp.Views.Android;
using LowPolyLibrary.Animation;

namespace LowPolyLibrary
{
    public class CustomCanvasView : SKCanvasView, View.IOnTouchListener
    {
        private LowPolyLibrary.Animation.Animation animation;
        private LowPolyLibrary.Triangulation _lowPoly;

        public CustomCanvasView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public CustomCanvasView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            animation = new LowPolyLibrary.Animation.Animation(this);
            SetOnTouchListener(this);
        }

        protected override void OnDraw(SKSurface surface, SKImageInfo info)
        {
            base.OnDraw(surface, info);
            animation.DrawOnMe(surface);
        }



        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Down:
                    var touch = new SKPoint(e.GetX(), e.GetY());

                    uuuu(AnimationTypes.Type.Touch, touch);
                    break;
                case MotionEventActions.Move:
                    break;
                case MotionEventActions.Up:
                    break;
            }
            Invalidate();
            return true;
        }

        public void Generate(int boundsWidth, int boundsHeight, double variance, double cellSize)
        {
            _lowPoly = new LowPolyLibrary.Triangulation(boundsWidth, boundsHeight, variance, cellSize);
        }

        public void sweepAnimation()
        {
            uuuu(AnimationTypes.Type.Sweep, new SKPoint(0,0));
        }

        public void growAnimation()
        {
            uuuu(AnimationTypes.Type.Grow, new SKPoint(0, 0));
        }

        private void uuuu(AnimationTypes.Type anim, SKPoint touch)
        {
            switch (anim)
            {
                case AnimationTypes.Type.Grow:
                    animation.AddEvent(_lowPoly, AnimationTypes.Type.Grow);
                    break;
                case AnimationTypes.Type.Sweep:
                    animation.AddEvent(_lowPoly, AnimationTypes.Type.Sweep);
                    break;
                case AnimationTypes.Type.Touch:
                    animation.AddEvent(_lowPoly, AnimationTypes.Type.Touch, touch.X, touch.Y, 500);
                    break;
            }
        }
    }
}