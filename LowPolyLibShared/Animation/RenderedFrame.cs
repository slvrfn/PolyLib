using System;
using System.Collections.Generic;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    //class used to store data necessary to draw a rendered frame
    public class RenderedFrame
    {
        public List<AnimatedPoint> CurrentFramePoints = null;
        public List<AnimatedPoint> PreviousFramePoints = null;

        public Action<SKSurface, List<AnimatedPoint>, bool> DrawFunction = null;

        public RenderedFrame(Action<SKSurface, List<AnimatedPoint>, bool> funct)
        {
            DrawFunction = funct;
        }
    }
}
