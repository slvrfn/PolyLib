using System;
using System.Collections.Generic;
using SkiaSharp;

namespace PolyLib.Animation
{
    //class used to store data necessary to draw a rendered frame
    public class RenderedFrame
    {
        public List<AnimatedPoint> FramePoints = null;

        public Action<SKSurface, List<AnimatedPoint>> DrawFunction = null;

        public RenderedFrame(Action<SKSurface, List<AnimatedPoint>> funct)
        {
            DrawFunction = funct;
        }

        public RenderedFrame(Action<SKSurface, List<AnimatedPoint>> funct, List<AnimatedPoint> points)
        {
            FramePoints = points;
            DrawFunction = funct;
        }
    }
}
