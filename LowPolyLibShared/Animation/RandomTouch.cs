using System;
using System.Collections.Generic;
using System.Text;

namespace LowPolyLibrary.Animation
{
    class RandomTouch : Touch
    {
        public RandomTouch(Triangulation triangulation, int numFrames, float x, float y, int radius) : base(triangulation, numFrames, x, y, radius)
        {
        }

        protected override void DoPointDisplacement(AnimatedPoint point, int currentFrame)
        {
            var direction = (int)Geometry.GetPolarCoordinates(TouchLocation, point.Point);

            var distCanMove = 20;

            point.XDisplacement = Random.Rand.Next(-10, 10);
            point.YDisplacement = Random.Rand.Next(-10, 10);

            //limiting the total dist this point can travel
            var maxXComponent = Geometry.getXComponent(direction, distCanMove);
            var maxYComponent = Geometry.getYComponent(direction, distCanMove);
            point.SetMaxDisplacement(maxXComponent, maxYComponent);
        }
    }
}
