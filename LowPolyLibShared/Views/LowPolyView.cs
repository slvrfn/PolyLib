using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LowPolyLibrary.Animation;
using SkiaSharp;

namespace LowPolyLibrary.Views
{
    class LowPolyView : FrameLayout, View.IOnTouchListener
    {
        public int numAnimFrames = 12;

        public TriangulationView triView { get; private set; }
        public AnimationUpdateView animView { get; private set; }

        protected LowPolyView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        public LowPolyView(Context context) : base(context)
        {
            Init();
        }

        public LowPolyView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public LowPolyView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init();
        }

        private void Init()
        {
            SetOnTouchListener(this);

            triView = new TriangulationView(Context);
            animView = new AnimationUpdateView(Context);

            var layoutParams = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            triView.LayoutParameters = layoutParams;
            animView.LayoutParameters = layoutParams;

            AddView(triView);
            AddView(animView);
        }

        public LowPolyView Generate(int boundsWidth, int boundsHeight, float variance, int cellSize)
        {
            //SKCanvasView cannot change size. Instead, generate a new one in this views place

            if (!boundsWidth.Equals(Width) || !boundsHeight.Equals(Height))
            {
                var parent = ((ViewGroup)Parent);
                var index = parent.IndexOfChild(this);
                parent.RemoveView(this);
                var newCanvasView = new LowPolyView(Context);
                newCanvasView.triView.Generate(boundsWidth, boundsHeight, variance, cellSize);
                parent.AddView(newCanvasView, index, new FrameLayout.LayoutParams(boundsWidth, boundsHeight));
                return newCanvasView;
            }
            else
            {
                triView.Generate(boundsWidth, boundsHeight, variance, cellSize);
                return this;
            }
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
                var touchAnimation = new Touch(triView.Triangulation, 6, touch.X, touch.Y, 250);
                animView.AddAnimation(touchAnimation);
            }

            return true;
        }

        public void sweepAnimation()
        {
            var sweepAnim = new Sweep(triView.Triangulation, numAnimFrames);
            animView.AddAnimation(sweepAnim);
        }

        public void growAnimation()
        {
            var growAnim = new Grow(triView.Triangulation, numAnimFrames);
            animView.AddAnimation(growAnim);
        }
    }
}
