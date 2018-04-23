using System;
using System.ComponentModel;
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
    [Register("LowPolyView"), DesignTimeVisible(true)]
    public class LowPolyView : UIView
    {
        TriangulationView TriangulationView;
        AnimationUpdateView AnimationUpdateView;


        public Triangulation CurrentTriangulation => TriangulationView.Triangulation;

#region Constructors
        public LowPolyView()
        {
            Initialize();
        }

        public LowPolyView(CGRect frame) : base(frame)
        {
            Initialize();
        }

        public LowPolyView(IntPtr p) : base(p)
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
            
            TriangulationView = new TriangulationView(Frame);

            AnimationUpdateView = new AnimationUpdateView(Frame);

            AddSubview(TriangulationView);
            AddSubview(AnimationUpdateView);
        }

#region Touch
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var loc = touch.LocationInView(this);
                var touchAnimation = new RandomTouch(CurrentTriangulation, 6, (float)(loc.X * UIScreen.MainScreen.Scale), (float)(loc.Y* UIScreen.MainScreen.Scale), 250);
                AddAnimation(touchAnimation);
            }

        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var loc = touch.LocationInView(this);
                var touchAnimation = new RandomTouch(CurrentTriangulation, 6, (float)(loc.X * UIScreen.MainScreen.Scale), (float)(loc.Y * UIScreen.MainScreen.Scale), 250);
                AddAnimation(touchAnimation);
            }
        }
#endregion

        public LowPolyView GenerateNewTriangulation(int boundsWidth, int boundsHeight, float variance, int cellSize)
        {
            //SKCanvasView cannot change size. Instead, generate a new one in this views place

            if (!boundsWidth.Equals(Frame.Size.Width) || !boundsHeight.Equals(Frame.Size.Height))
            {
                
                //var newCanvasView = new LowPolyView(Frame);
                //newCanvasView.TriangulationView.Generate(boundsWidth, boundsHeight, variance, cellSize);
                TriangulationView.Generate(boundsWidth, boundsHeight, variance, cellSize);
                //InsertSubviewAbove(newCanvasView, this);
                //RemoveFromSuperview();
                AnimationUpdateView.SetNeedsDisplay();
                return this;
            }
            else
            {
                TriangulationView.Generate(boundsWidth, boundsHeight, variance, cellSize);
                //only called here since a whole new Lowpoly view is created in the other case
                AnimationUpdateView.SetNeedsDisplay();
                return this;
            }
        }

        public void AddAnimation(AnimationBase anim) => AnimationUpdateView.AddAnimation(anim);

    }
}