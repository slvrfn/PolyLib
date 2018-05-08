using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    public class Sweep : AnimationBase
    {
        private int Direction = -1;

        public Sweep(Triangulation triangulation, int numFrames) : base(triangulation, numFrames)
        {

        }

        //necessary to prevent animationbase from "setting up" multiple times
        internal override void SetupAnimation()
        {
            base.SetupAnimation();
            IsSetup = true;
        }

        internal override HashSet<AnimatedPoint> RenderFrame(int currentFrame)
        {
            var animatedPoints = new HashSet<AnimatedPoint>();
            //all the points will move within 15 degrees of the same direction
            var localDirection = Geometry.getAngleInRange(Direction, 15);

            //accumulate all points in the current column represented by frame index
            List<SkiaSharp.SKPoint> framePoints = new List<SkiaSharp.SKPoint>();
            for (int i = 0; i < NumFrames; i++)
            {
                var p = new SkiaSharp.SKPointI(currentFrame, i);
                if (SeperatedPoints.ContainsKey(p))
                {
                    framePoints.AddRange(SeperatedPoints[p]);
                }
            }

            foreach (var point in framePoints)
            {
                var distCanMove = shortestDistanceFromPoints(point);
                var xComponent = Geometry.getXComponent(localDirection, distCanMove);
                var yComponent = Geometry.getYComponent(localDirection, distCanMove);
                var p = new AnimatedPoint(point, (float)xComponent, (float)yComponent);
                animatedPoints.Add(p);
            }
            return animatedPoints;
        }
    }
}
