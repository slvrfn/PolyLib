using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DelaunayTriangulator;
using SkiaSharp;

namespace PolyLib.Animation
{
    public class Sweep : AnimationBase
    {
        private int _direction;

        public Sweep(Triangulation triangulation, int numFrames, int gridDirection = -1) : base(triangulation, numFrames, gridDirection)
        {
            //all the points will move within 15 degrees of the same direction
            _direction = Geometry.getAngleInRange(GridDirection, 15);
        }

        //necessary to prevent animationbase from "setting up" multiple times
        public override void SetupAnimation()
        {
            base.SetupAnimation();
            IsSetup = true;
        }

        public override HashSet<AnimatedPoint> RenderFrame(int currentFrame)
        {
            var animatedPoints = new HashSet<AnimatedPoint>();

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
                var xComponent = Geometry.getXComponent(_direction, distCanMove);
                var yComponent = Geometry.getYComponent(_direction, distCanMove);
                var p = new AnimatedPoint(point, (float)xComponent, (float)yComponent);
                animatedPoints.Add(p);
            }
            return animatedPoints;
        }
    }
}
