using System;
using System.Collections.Generic;
using System.Text;
using Android.Graphics;
using Android.Graphics.Drawables;
using DelaunayTriangulator;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
    class Touch : Animation
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
            InRange = new List<PointF>();
            InRangOfRecs = new List<PointF>();
            OutOfRange = new List<PointF>();
            TouchLocation = new PointF(x, y);
            TouchRadius = radius;

            setPointsAroundTouch();
        }

        public AnimationDrawable Animation => MakeAnimation();

        private AnimationDrawable MakeAnimation()
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42 * 2;//roughly how many milliseconds each frame will be for 24fps

            for (int i = 0; i < numFrames; i++)
            {
                Bitmap frameBitmap = createTouchAnimBitmap();

                BitmapDrawable frame = new BitmapDrawable(frameBitmap);
                animation.AddFrame(frame, duration);
            }

            return animation;
        }

        private Bitmap createTouchAnimBitmap()
        {
            makeTouchPointsFrame();
            var frameBitmap = drawPointFrame();
            return frameBitmap;
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

        internal void makeTouchPointsFrame()
        {
            var removePoints = new List<PointF>();
            var newPoints = new List<PointF>();
            //var pointsForMeasure = new List<PointF>();
            //pointsForMeasure.AddRange(InRange);
            //pointsForMeasure.AddRange(InRangOfRecs);
            var rand = new Random();
            foreach (var point in InRange)
            {
                var wPoint = new PointF(point.X, point.Y);
                //created bc cant modify point
                removePoints.Add(point);
                //var direction = (int)getPolarCoordinates(touch, wPoint);

                //var distCanMove = shortestDistanceFromPoints(point, pointsForMeasure, direction);
                //var distCanMove = 20;
                //var xComponent = getXComponent(direction, distCanMove);
                //var yComponent = getYComponent(direction, distCanMove);

                var xComponent = rand.Next(-10, 10);
                var yComponent = rand.Next(-10, 10);

                wPoint.X += (float)xComponent;
                wPoint.Y += (float)yComponent;
                newPoints.Add(wPoint);
            }
            //removePoints and newPoints should always be the same length
            for (int i = 0; i < removePoints.Count - 1; i++)
            {
                InRange.Remove(removePoints[i]);
                InRange.Add(newPoints[i]);
            }
        }

        private Bitmap drawPointFrame()
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

            //generating a new base triangulation. if an old one exists get rid of it
            //if (poTriDic != null)
            //    poTriDic = new Dictionary<System.Drawing.PointF, List<Triad>>();

            var convertedPoints = new List<DelaunayTriangulator.Vertex>();
            //can we just stay in PointF's?

            var measurePoints = new List<PointF>();
            measurePoints.AddRange(InRange);
            measurePoints.AddRange(InRangOfRecs);
            measurePoints.AddRange(OutOfRange);

            foreach (var point in measurePoints)
            {
                var currentlyExists = convertedPoints.Exists(x =>
                        x.x.CompareTo(point.X) == 0 &&
                        x.y.CompareTo(point.Y) == 0
                    );
                if (!currentlyExists)
                    convertedPoints.Add(new DelaunayTriangulator.Vertex(point.X, point.Y));
            }
            var angulator = new Triangulator();
            var newTriangulatedPoints = angulator.Triangulation(convertedPoints);
            for (int i = 0; i < newTriangulatedPoints.Count; i++)
            {
                var a = new PointF(convertedPoints[newTriangulatedPoints[i].a].x, convertedPoints[newTriangulatedPoints[i].a].y);
                var b = new PointF(convertedPoints[newTriangulatedPoints[i].b].x, convertedPoints[newTriangulatedPoints[i].b].y);
                var c = new PointF(convertedPoints[newTriangulatedPoints[i].c].x, convertedPoints[newTriangulatedPoints[i].c].y);

                Path trianglePath = drawTrianglePath(a, b, c);

                var center = Geometry.centroid(newTriangulatedPoints[i], convertedPoints);

                paint.Color = getTriangleColor(gradient, center);

                canvas.DrawPath(trianglePath, paint);
            }
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Android.Graphics.Color.Crimson;
            canvas.DrawCircle(TouchLocation.X, TouchLocation.Y, TouchRadius, paint);
            return drawingCanvas;
        }
    }
}
