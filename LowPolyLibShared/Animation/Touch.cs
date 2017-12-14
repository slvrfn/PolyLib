using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    class Touch : AnimationBase
    {
        public List<SKPoint> InRange;
        public SKPoint TouchLocation;
        public int TouchRadius;

        internal Touch(Triangulation triangulation, float x, float y, int radius) : base(triangulation)
        {
			AnimationType = AnimationTypes.Type.Touch;

            InRange = new List<SKPoint>();
            TouchLocation = new SKPoint(x, y);
            TouchRadius = radius;
        }

        //saves 
        internal List<SKPoint> getTouchAreaRecPoints()
        {
            var touch = new List<SKPoint>();

            var BL = new SKPoint(TouchLocation.X - TouchRadius, TouchLocation.Y - TouchRadius);
            var TR = new SKPoint(TouchLocation.X + TouchRadius, TouchLocation.Y + TouchRadius);

            var BLindex = new SKPointI();
            var TRindex = new SKPointI();

            GridRotation.CellCoordsFromOriginPoint(ref BLindex, BL);
            GridRotation.CellCoordsFromOriginPoint(ref TRindex, TR);

            var upperX = TRindex.X > BLindex.X ? TRindex.X : BLindex.X;
            var lowerX = TRindex.X < BLindex.X ? TRindex.X : BLindex.X;

            var upperY = TRindex.Y > BLindex.Y ? TRindex.Y : BLindex.Y;
            var lowerY = TRindex.Y < BLindex.Y ? TRindex.Y : BLindex.Y;

            for (int i = lowerX; i <= upperX; i++)
            {
                for (int j = lowerY; j <= upperY; j++)
                {
                    var p = new SKPointI(i, j);

                    if (SeperatedPoints.ContainsKey(p))
                        touch.AddRange(SeperatedPoints[p]);
                }
            }

            return touch;
        }

        internal void setPointsAroundTouch()
        {
            //get all points in the same rec as the touch area
            InRange = getTouchAreaRecPoints();

            //actually ween down the points in the touch area to the points inside the "circle" touch area
            var removeFromTouchPoints = new List<SKPoint>();
            foreach (var point in InRange)
            {

                if (!Geometry.pointInsideCircle(point, TouchLocation, TouchRadius))
                {
                    removeFromTouchPoints.Add(point);
                }
            }
            foreach (var point in removeFromTouchPoints)
            {
                InRange.Remove(point);
            }
        }

		//helper for "push away from touch"
		private float frameLocation(int frame, int totalFrames, float distanceToCcover)
		{
			//method to be used when a final destination is known, and you want to get a proportional distance to have moved up to that point at this point in time
			var ratioToFinalMovement = frame / (float)totalFrames;
			var thisCoord = ratioToFinalMovement * distanceToCcover;
			return thisCoord;
		}

        internal override void SetupAnimation()
        {
            base.SetupAnimation();

            setPointsAroundTouch();

            IsSetup = true;
        }

        internal override List<AnimatedPoint> RenderFrame()
        {
			var animatedPoints = new List<AnimatedPoint>();

            //var rand = new Random();
            foreach (var point in InRange)
            {
                var direction = (int)Geometry.GetPolarCoordinates(TouchLocation, point);

                var distCanMove = shortestDistanceFromPoints(point);
                //var distCanMove = 20;
                var frameDistCanMove = frameLocation(CurrentFrame, numFrames, distCanMove);
                
                var xComponent = Geometry.getXComponent(direction, frameDistCanMove);
                var yComponent = Geometry.getYComponent(direction, frameDistCanMove);

                //var xComponent = rand.Next(-10, 10);
                //var yComponent = rand.Next(-10, 10);
                
                var animPoint = new AnimatedPoint(point, xComponent, yComponent);

                //limiting the total dist this point can travel
                var maxXComponent = Geometry.getXComponent(direction, distCanMove);
                var maxYComponent = Geometry.getYComponent(direction, distCanMove);
                animPoint.SetMaxDisplacement(maxXComponent, maxYComponent);

                animatedPoints.Add(animPoint);
            }
			return animatedPoints;
        }

		//only overriding to force display of red ring of current touch area
        internal override void DrawPointFrame(SKSurface surface, List<AnimatedPoint> pointChanges)
        {
            //base DrawSKPointrame will render the animation correctly, get the bitmap
            base.DrawPointFrame(surface, pointChanges);

            //Create a canvas to draw touch location on the bitmap
            using (var canvas = surface.Canvas)
            {
                //canvas not cleared here bc it is done in the base method above
                using (var paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = new SKColor(247, 77, 77);

                    canvas.DrawCircle(TouchLocation.X, TouchLocation.Y, TouchRadius, paint);
                }
            }
        }
    }
}
