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

        public LowPolyView GenerateNewTriangulation(int boundsWidth, int boundsHeight, float variance, int cellSize)
        {
            //SKCanvasView cannot change size. Instead, generate a new one in this views place

            if (!boundsWidth.Equals(Frame.Size.Width) || !boundsHeight.Equals(Frame.Size.Height))
            {
                
                RemoveFromSuperview();
                var newCanvasView = new LowPolyView(Frame);
                newCanvasView.TriangulationView.Generate(boundsWidth, boundsHeight, variance, cellSize);
                Superview.AddSubview(newCanvasView);
                return newCanvasView;
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