using System;
using System.Collections.Generic;
using System.Text;

namespace LowPolyLibrary.Animation
{
    class PushTouch : Touch
    {
        public PushTouch(Triangulation triangulation, int numFrames, float x, float y, int radius) : base(triangulation, numFrames, x, y, radius)
        {
        }

        protected override void DoPointDisplacement(AnimatedPoint point, int currentFrame)
        {
            var direction = (int)Geometry.GetPolarCoordinates(TouchLocation, point.Point);

            var distCanMove = shortestDistanceFromPoints(point.Point);
            var frameDistCanMove = frameLocation(currentFrame, NumFrames, distCanMove);

            point.XDisplacement = Geometry.getXComponent(direction, frameDistCanMove);
            point.YDisplacement = Geometry.getYComponent(direction, frameDistCanMove);

            //limiting the total dist this point can travel
            var maxXComponent = Geometry.getXComponent(direction, distCanMove);
            var maxYComponent = Geometry.getYComponent(direction, distCanMove);
            point.SetMaxDisplacement(maxXComponent, maxYComponent);
        }
    }
}
