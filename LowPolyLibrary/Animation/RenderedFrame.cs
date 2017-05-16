using System;
using System.Collections.Generic;
using Android.Graphics;

namespace LowPolyLibrary.Animation
{
    //class used to store data necessary to draw a rendered frame
    public class RenderedFrame
    {
        public List<AnimatedPoint> FramePoints = null;

        public Func<List<AnimatedPoint>, Bitmap> DrawFunction = null;

        public RenderedFrame(Func<List<AnimatedPoint>, Bitmap> funct)
        {
            DrawFunction = funct;
        }

        public RenderedFrame(Func<List<AnimatedPoint>, Bitmap> funct, List<AnimatedPoint> points )
        {
            FramePoints = points;
            DrawFunction = funct;
        }
    }
}
