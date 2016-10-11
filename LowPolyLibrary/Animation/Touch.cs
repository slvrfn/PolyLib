using System;
using System.Collections.Generic;
using System.Text;
using Android.Graphics;
using DelaunayTriangulator;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
    class Touch : Animation
    {
        internal Touch()
        {
            
        }


        private int _lowerBound;
        private int _upperBound;

        internal struct TouchPoints
        {
            public List<PointF> inRange;
            public List<PointF> inRangOfRecs;
            public List<PointF> outOfRange;
            public PointF touchLocation;
            public int touchRadius;
        }

        

        public Bitmap createTouchAnimBitmap(PointF touch, int radius)
        {
            var frameList = makeTouchPointsFrame(touch, radius);
            var frameBitmap = drawPointFrame(frameList);
            return frameBitmap;
        }

        public void setPointsaroundTouch(PointF touch, int radius)
        {
            setPointsAroundTouch(touch, radius);
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
                if (currentIndex != firstFrame &&
                    viewRectangles[0][currentIndex].circleContainsPoints(touchPointLists.touchLocation,
                                                                         touchPointLists.touchRadius,
                                                                         viewRectangles[0][currentIndex].A,
                                                                         viewRectangles[0][currentIndex].D))
                {
                    touch.AddRange(getTouchAreaRecPoints(currentIndex, -1));
                }
                if (currentIndex != lastFrame &&
                    viewRectangles[0][currentIndex].circleContainsPoints(touchPointLists.touchLocation,
                                                                         touchPointLists.touchRadius,
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
                        viewRectangles[0][currentIndex].circleContainsPoints(touchPointLists.touchLocation,
                                                                             touchPointLists.touchRadius,
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
                             viewRectangles[0][currentIndex].circleContainsPoints(touchPointLists.touchLocation,
                                                                                  touchPointLists.touchRadius,
                                                                                  viewRectangles[0][currentIndex].B,
                                                                                  viewRectangles[0][currentIndex].C))
                    {
                        touch.AddRange(getTouchAreaRecPoints(currentIndex, 1));
                    }
                }
            }



            touch.AddRange(framedPoints[currentIndex]);
            return touch;
        }

        internal void setPointsAroundTouch(PointF touch, int radius)
        {

            touchPointLists = new TouchPoints { inRange = new List<PointF>(), inRangOfRecs = new List<PointF>(), outOfRange = new List<PointF>(), touchLocation = new PointF(), touchRadius = new int() };
            touchPointLists.touchLocation = touch;
            touchPointLists.touchRadius = radius;
            //index of the smaller rectangle that contains the touch point
            //var index = Array.FindIndex(viewRectangles[0], rec => rec.isInsideCircle(touch, radius));
            var index = Array.FindIndex(viewRectangles[0], rec => rec.Contains(touch));
            //get all points in the same rec as the touch area
            touchPointLists.inRange = getTouchAreaRecPoints(index);

            //add the points from recs outside of the touch area to a place we can handle them later
            for (int i = 0; i < framedPoints.Length; i++)
            {
                if (!(_lowerBound < i && i < _upperBound))
                    touchPointLists.outOfRange.AddRange(framedPoints[i]);
                touchPointLists.outOfRange.AddRange(wideFramedPoints[i]);
            }

            //actually ween down the points in the touch area to the points inside the "circle" touch area
            var removeFromTouchPoints = new List<PointF>();
            foreach (var point in touchPointLists.inRange)
            {

                if (!Geometry.pointInsideCircle(point, touch, radius))
                {
                    //touchPointLists.inRange.Remove(point);
                    removeFromTouchPoints.Add(point);
                    touchPointLists.inRangOfRecs.Add(point);
                }
            }
            foreach (var point in removeFromTouchPoints)
            {
                touchPointLists.inRange.Remove(point);
            }

        }

        internal TouchPoints makeTouchPointsFrame(PointF touch, int radius)
        {
            var removePoints = new List<PointF>();
            var newPoints = new List<PointF>();
            var pointsForMeasure = new List<PointF>();
            pointsForMeasure.AddRange(touchPointLists.inRange);
            pointsForMeasure.AddRange(touchPointLists.inRangOfRecs);
            var rand = new Random();
            foreach (var point in touchPointLists.inRange)
            {
                var wPoint = new PointF(point.X, point.Y);
                //created bc cant modify point
                removePoints.Add(point);
                //var direction = (int)getPolarCoordinates(touch, wPoint);

                //var distCanMove = shortestDistanceFromPoints(point, pointsForMeasure, direction);
                var distCanMove = 20;
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
                touchPointLists.inRange.Remove(removePoints[i]);
                touchPointLists.inRange.Add(newPoints[i]);
            }

            return touchPointLists;
        }

        private Bitmap drawPointFrame(Touch.TouchPoints frameList)
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
            measurePoints.AddRange(frameList.inRange);
            measurePoints.AddRange(frameList.inRangOfRecs);
            measurePoints.AddRange(frameList.outOfRange);

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

                //animation logic
                //divyTris(a, overlays, i);
                //divyTris(b, overlays, i);
                //divyTris(c,overlays, i);

                paint.Color = getTriangleColor(gradient, center);

                canvas.DrawPath(trianglePath, paint);
            }
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Android.Graphics.Color.Crimson;
            canvas.DrawCircle(frameList.touchLocation.X, frameList.touchLocation.Y, frameList.touchRadius, paint);
            return drawingCanvas;
        }
    }
}
