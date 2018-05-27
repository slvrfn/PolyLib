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
    [Register("PassthroughView"), DesignTimeVisible(true)]
    public class PassthroughView : UIView
    {
        public PassthroughView()
        {

        }

        public PassthroughView(CGRect frame) : base(frame)
        {

        }

        public PassthroughView(IntPtr p) : base(p)
        {

        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var v = base.HitTest(point, uievent);

            return v == this ? null : v;
        }

    }
}