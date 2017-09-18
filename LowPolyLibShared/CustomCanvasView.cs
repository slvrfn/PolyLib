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
        private LowPolyLibrary.Animation.Animation animationEngine;
        private LowPolyLibrary.Triangulation _lowPoly;

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
            animationEngine = new LowPolyLibrary.Animation.Animation(this);
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
            if (animationEngine.HasFrameToDraw)
            {
                animationEngine.DrawOnMe(surface);
            }
            else
            {
                if (_lowPoly != null)
                {
                    _lowPoly.GeneratedBitmap(surface);
                }
            }
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
            switch (e.Action)
            {
                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Down:
                    var touch = new SKPoint(e.GetX(), e.GetY());

                    animationEngine.AddEvent(_lowPoly, AnimationTypes.Type.Touch, touch.X, touch.Y, 500);
                    break;
                case MotionEventActions.Move:
                    break;
                case MotionEventActions.Up:
                    break;
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
            animationEngine.AddEvent(_lowPoly, AnimationTypes.Type.Sweep);
        }

        public void growAnimation()
        {
            animationEngine.AddEvent(_lowPoly, AnimationTypes.Type.Grow);
        }
    }
}