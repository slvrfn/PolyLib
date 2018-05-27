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
    [Register("PolyLibView"), DesignTimeVisible(true)]
    public class PolyLibView : UIView
    {
        public TriangulationView TriangulationView;
        public AnimationUpdateView AnimationUpdateView;


        public Triangulation CurrentTriangulation => TriangulationView.Triangulation;

        #region Constructors
        public PolyLibView()
        {
            Initialize();
        }

        public PolyLibView(CGRect frame) : base(frame)
        {
            Initialize();
        }

        public PolyLibView(IntPtr p) : base(p)
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

        public PolyLibView ResizeView(int boundsWidth, int boundsHeight, List<UIGestureRecognizer> recognizers)//, View.IOnTouchListener listener = null)
        {
            //SKCanvasView cannot change size. Instead, generate a new one in this views place
            var newFrame = new CGRect(Frame.Location, new CGSize(boundsWidth, boundsHeight));

            var newCanvasView = new PolyLibView(newFrame);

            //setup gesture recognizers
            foreach (var recognizer in recognizers)
            {
                newCanvasView.AddGestureRecognizer(recognizer);
                RemoveGestureRecognizer(recognizer);
            }

            Superview.InsertSubviewAbove(newCanvasView, this);
            RemoveFromSuperview();

            return newCanvasView;
        }

        public void UpdateTriangulation(Triangulation tri)
        {
            TriangulationView.UpdateTriangulation(tri);
            SetNeedsDisplay();
        }

        public void AddAnimation(AnimationBase anim) => AnimationUpdateView.AddAnimation(anim);

    }
}