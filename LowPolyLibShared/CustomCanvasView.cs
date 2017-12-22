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
        private LowPolyLibrary.Animation.AnimationEngine _animationFlowEngine;
        private LowPolyLibrary.Triangulation _lowPoly;
        public int numAnimFrames = 12;

        float Variance = .75f;
        int CellSize = 150;

        public CustomCanvasView(Context context) : base(context)
        {
            Initialize();
        }

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
            _animationFlowEngine = new LowPolyLibrary.Animation.AnimationEngine(this);
            SetOnTouchListener(this);
            ViewTreeObserver.AddOnGlobalLayoutListener(new GlobalLayoutListener((obj) =>
            {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(obj);
                _lowPoly = new LowPolyLibrary.Triangulation(Width, Height, Variance, CellSize);
                Invalidate();
            }));
        }

        protected override void OnDraw(SKSurface surface, SKImageInfo info)
        {
            base.OnDraw(surface, info);
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            if (_animationFlowEngine.HasFrameToDraw)
            {
                _animationFlowEngine.DrawOnMe(surface);
                Console.WriteLine("Animation Frame drawn in: " + watch.ElapsedTicks + " ticks\n");
            }
            else
            {
                if (_lowPoly != null)
                {
                    _lowPoly.GeneratedBitmap(surface);
                    Console.WriteLine("Triangulation Frame drawn in: " + watch.ElapsedTicks + " ticks\n");
                }
            }
            watch.Stop();
            
        }
        //necessary?
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            ViewTreeObserver.AddOnGlobalLayoutListener(new GlobalLayoutListener((obj) =>
            {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(obj);
                _lowPoly = new LowPolyLibrary.Triangulation(Width, Height, Variance, CellSize);
                Invalidate();
            }));
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            var touch = new SKPoint(e.GetX(), e.GetY());
            bool startAnim = false;
            switch (e.Action)
            {
                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Down:
                    startAnim = true;
                    break;
                case MotionEventActions.Move:
                    startAnim = true;
                    break;
                case MotionEventActions.Up:
                    break;
            }

            if (startAnim)
            {
                var touchAnimation = new Touch(_lowPoly, 6, touch.X, touch.Y, 250);
                _animationFlowEngine.AddAnimation(touchAnimation);
            }

            return true;
        }

        public CustomCanvasView Generate(int boundsWidth, int boundsHeight, float variance, int cellSize)
        {
            //SKCanvasView cannot change size. Instead, generate a new one in this views place

            if (!boundsWidth.Equals(Width) || !boundsHeight.Equals(Height))
            {
                var parent = ((ViewGroup)Parent);
                var index = parent.IndexOfChild(this);
                parent.RemoveView(this);
                var newCanvasView = new CustomCanvasView(Context);
                newCanvasView.Variance = variance;
                newCanvasView.CellSize = cellSize;
                parent.AddView(newCanvasView, index, new FrameLayout.LayoutParams(boundsWidth, boundsHeight));
                return newCanvasView;
            }
            else
            {
                Variance = variance;
                CellSize = cellSize;
                _lowPoly = new LowPolyLibrary.Triangulation(Width, Height, Variance, CellSize);
                Invalidate();
                return this;
            }
        }

        public void sweepAnimation()
        {
            var sweepAnim = new Sweep(_lowPoly, numAnimFrames);
            _animationFlowEngine.AddAnimation(sweepAnim);
        }

        public void growAnimation()
        {
            var growAnim = new Grow(_lowPoly, numAnimFrames);
            _animationFlowEngine.AddAnimation(growAnim);
        }
    }
}