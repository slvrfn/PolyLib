using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using DelaunayTriangulator;
using LowPolyLibrary.BitmapPool;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary.Animation
{
    class Touch : AnimationBase
    {
        private int _lowerBound;
        private int _upperBound;

        public List<PointF> InRange;
        public List<PointF> InRangOfRecs;
        public List<PointF> OutOfRange;
        public PointF TouchLocation;
        public int TouchRadius;

        internal Touch(Triangulation triangulation, float x, float y, int radius) : base(triangulation)
        {
			AnimationType = AnimationTypes.Type.Touch;

            InRange = new List<PointF>();
            InRangOfRecs = new List<PointF>();
            OutOfRange = new List<PointF>();
            TouchLocation = new PointF(x, y);
            TouchRadius = radius;

            setPointsAroundTouch();
        }

        internal List<PointF> getTouchAreaRecPoints(int currentIndex, int displacement = 0)
        {
            var touch = new List<PointF>();

            currentIndex += displacement;

            var firstFrame = 0;
            var lastFrame = viewRectangles[0].Length - 1;

            if (currentIndex < firstFrame)
            {
                currentIndex++;
            }
            if (currentIndex > lastFrame)
            {
                currentIndex--;
            }

            if (displacement == 0)
            {
                _lowerBound = currentIndex;
                _upperBound = currentIndex;
                if (currentIndex != firstFrame &&
                    viewRectangles[0][currentIndex].circleContainsPoints(TouchLocation,
                                                                         TouchRadius,
                                                                         viewRectangles[0][currentIndex].A,
                                                                         viewRectangles[0][currentIndex].D))
                {
                    touch.AddRange(getTouchAreaRecPoints(currentIndex, -1));
                }
                if (currentIndex != lastFrame &&
                    viewRectangles[0][currentIndex].circleContainsPoints(TouchLocation,
                                                                         TouchRadius,
                                                                         viewRectangles[0][currentIndex].B,
                                                                         viewRectangles[0][currentIndex].C))
                {
                    touch.AddRange(getTouchAreaRecPoints(currentIndex, 1));
                }
            }
            else
            {

                if (displacement < 0)
                {
                    _lowerBound = currentIndex;
                    if (currentIndex != firstFrame &&
                        viewRectangles[0][currentIndex].circleContainsPoints(TouchLocation,
                                                                             TouchRadius,
                                                                             viewRectangles[0][currentIndex].A,
                                                                             viewRectangles[0][currentIndex].D))
                    {
                        touch.AddRange(getTouchAreaRecPoints(currentIndex, -1));
                    }
                }
                else if (displacement > 0)
                {
                    _upperBound = currentIndex;
                    if (currentIndex != lastFrame &&
                             viewRectangles[0][currentIndex].circleContainsPoints(TouchLocation,
                                                                                  TouchRadius,
                                                                                  viewRectangles[0][currentIndex].B,
                                                                                  viewRectangles[0][currentIndex].C))
                    {
                        touch.AddRange(getTouchAreaRecPoints(currentIndex, 1));
                    }
                }
            }



            touch.AddRange(FramedPoints[currentIndex]);
            return touch;
        }

        internal void setPointsAroundTouch()
        {
            //index of the smaller rectangle that contains the touch point
            //var index = Array.FindIndex(viewRectangles[0], rec => rec.isInsideCircle(touch, radius));
            var index = Array.FindIndex(viewRectangles[0], rec => rec.Contains(TouchLocation));
            //get all points in the same rec as the touch area
            InRange = getTouchAreaRecPoints(index);

            //add the points from recs outside of the touch area to a place we can handle them later
            for (int i = 0; i < FramedPoints.Length; i++)
            {
                if (!(_lowerBound < i && i < _upperBound))
                    OutOfRange.AddRange(FramedPoints[i]);
                OutOfRange.AddRange(WideFramedPoints[i]);
            }

            //actually ween down the points in the touch area to the points inside the "circle" touch area
            var removeFromTouchPoints = new List<PointF>();
            foreach (var point in InRange)
            {

                if (!Geometry.pointInsideCircle(point, TouchLocation, TouchRadius))
                {
                    //touchPointLists.inRange.Remove(point);
                    removeFromTouchPoints.Add(point);
                    InRangOfRecs.Add(point);
                }
            }
            foreach (var point in removeFromTouchPoints)
            {
                InRange.Remove(point);
            }
        }

		//helper for "push away from touch"
		internal double frameLocation(int frame, int totalFrames, Double distanceToCcover)
		{
			//method to be used when a final destination is known, and you want to get a proportional distance to have moved up to that point at this point in time
			var ratioToFinalMovement = frame / (Double)totalFrames;
			var thisCoord = ratioToFinalMovement * distanceToCcover;
			return thisCoord;
		}

        internal override List<AnimatedPoint> RenderFrame()
        {
			var animatedPoints = new List<AnimatedPoint>();
            //var pointsForMeasure = new List<PointF>();
            //pointsForMeasure.AddRange(InRange);
            //pointsForMeasure.AddRange(InRangOfRecs);
            var rand = new Random();
            foreach (var point in InRange)
            {
                //var direction = (int)getPolarCoordinates(touch, wPoint);

                //var distCanMove = shortestDistanceFromPoints(point, pointsForMeasure, direction);
                //var distCanMove = 20;
                //var xComponent = getXComponent(direction, distCanMove);
                //var yComponent = getYComponent(direction, distCanMove);

                var xComponent = rand.Next(-10, 10);
                var yComponent = rand.Next(-10, 10);

				var animPoint = new AnimatedPoint(point, xComponent, yComponent);
				animatedPoints.Add(animPoint);
            }
			AnimatedPoints = animatedPoints;
			return animatedPoints;
        }

		//only overriding to force display of red ring of current touch area
        internal override IManagedBitmap DrawPointFrame(List<AnimatedPoint> pointChanges)
        {
			//base DrawPointFrame will render the animation correctly, get the bitmap
			var renderedBitmap = base.DrawPointFrame(pointChanges);

			



            var paint = new Paint();
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Android.Graphics.Color.Crimson;

            //Create a canvas to draw touch location on the bitmap
            using (Canvas canvas = new Canvas(renderedBitmap.GetBitmap()))
            {
                canvas.DrawCircle(TouchLocation.X, TouchLocation.Y, TouchRadius, paint);
            }

            

            return renderedBitmap;
        }
    }
}
