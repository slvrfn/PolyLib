using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    abstract class Touch : AnimationBase
    {
        protected HashSet<SKPoint> InRange;
        protected SKPoint TouchLocation;
        protected readonly int TouchRadius;

        protected HashSet<AnimatedPoint> animatedPoints;

        internal Touch(Triangulation triangulation, int numFrames, float x, float y, int radius) : base(triangulation, numFrames)
        {
            AnimationType = AnimationTypes.Type.RandomTouch;

            InRange = new HashSet<SKPoint>();
            TouchLocation = new SKPoint(x, y);
            TouchRadius = radius;
        }

        //saves 
        internal HashSet<SKPoint> getTouchAreaRecPoints()
        {
            var touch = new HashSet<SKPoint>();

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

            var p = new SKPointI();

            for (var i = lowerX; i <= upperX; i++)
            {
                for (var j = lowerY; j <= upperY; j++)
                {
                    p.X = i;
                    p.Y = j;

                    if (SeperatedPoints.ContainsKey(p))
                        touch.UnionWith(SeperatedPoints[p]);
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
        protected float frameLocation(int frame, int totalFrames, float distanceToCcover)
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

            animatedPoints = new HashSet<AnimatedPoint>();

            IsSetup = true;
        }

        protected abstract void DoPointDisplacement(AnimatedPoint point);

        internal override HashSet<AnimatedPoint> RenderFrame()
        {
            if (animatedPoints.Count == 0)
            {
                foreach (var point in InRange)
                {
                    var animPoint = new AnimatedPoint(point);

                    DoPointDisplacement(animPoint);

                    animatedPoints.Add(animPoint);

                    //AddConnectedPoints(point, InRange, animatedPoints, true);
                }
            }
            else
            {
                foreach (var animPoint in animatedPoints)
                {
                    DoPointDisplacement(animPoint);
                }
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
                //used to temp change stroke color
                var c = strokePaint.Color;
                strokePaint.Color = new SKColor(247, 77, 77);

                //canvas not cleared here bc it is done in the base method above
                canvas.DrawCircle(TouchLocation.X, TouchLocation.Y, TouchRadius, strokePaint);

                strokePaint.Color = c;
            }
        }
    }
}
