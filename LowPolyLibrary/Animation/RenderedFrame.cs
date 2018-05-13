using System;
using System.Collections.Generic;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    //class used to store data necessary to draw a rendered frame
    public class RenderedFrame
    {
        internal readonly int FrameIdentifier = Guid.NewGuid().GetHashCode(); 

        public List<AnimatedPoint> FramePoints = null;

        public Action<SKSurface, List<AnimatedPoint>> DrawFunction = null;

        public int currFrame, totalFrame;

        public RenderedFrame(Action<SKSurface, List<AnimatedPoint>> funct, int currentframe, int totalframes)
        {
            DrawFunction = funct;
            totalFrame = totalframes;
            currFrame = currentframe;
        }

        public RenderedFrame(Action<SKSurface, List<AnimatedPoint>> funct, List<AnimatedPoint> points)
        {
            FramePoints = points;
            DrawFunction = funct;
        }
    }
}
