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

namespace LowPoly
{
    public class CustomCanvasView : SKCanvasView, View.IOnTouchListener
    {
        private SKPoint touchLocation;

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
            touchLocation = new SKPoint(1,1);
            SetOnTouchListener(this);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            Console.WriteLine($"touch recorded as x:{touchLocation.X} Y:{touchLocation.Y}");
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Down:
                    break;
                case MotionEventActions.Move:
                    touchLocation.X = e.GetX();
                    touchLocation.Y = e.GetY();
                    break;
                case MotionEventActions.Up:
                    break;
            }
            Invalidate();
            return true;
        }
    }
}