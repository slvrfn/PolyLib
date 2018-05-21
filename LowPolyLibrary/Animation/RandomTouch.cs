using System;
using System.Collections.Generic;
using System.Text;

namespace LowPolyLibrary.Animation
{
    public class RandomTouch : Touch
    {
        //distance a point is allowed to travel in 1 direction
        //point displacement will be in the range of: (-_displacementRange, _displacementRange)
        private int _displacementRange;

        public RandomTouch(Triangulation triangulation, int numFrames, float x, float y, int radius, int displacement = 10, int gridDirection = -1) : base(triangulation, numFrames, x, y, radius, gridDirection)
        {
            _displacementRange = displacement;
        }

        protected override void DoPointDisplacement(AnimatedPoint point, int currentFrame)
        {
            var direction = (int)Geometry.GetPolarCoordinates(TouchLocation, point.Point);

            var distCanMove = 20;

            point.XDisplacement = Random.Rand.Next(-_displacementRange, _displacementRange);
            point.YDisplacement = Random.Rand.Next(-_displacementRange, _displacementRange);

            //limiting the total dist this point can travel
            var maxXComponent = Geometry.getXComponent(direction, distCanMove);
            var maxYComponent = Geometry.getYComponent(direction, distCanMove);
            point.SetMaxDisplacement(maxXComponent, maxYComponent);
        }
    }
}
