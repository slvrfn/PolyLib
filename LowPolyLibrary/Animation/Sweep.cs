using System;
using System.Collections.Generic;
using System.Text;
using Android.Graphics;
using Android.Graphics.Drawables;
using DelaunayTriangulator;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
    public class Sweep : Animation
    {
        internal Sweep(Triangulation triangulation): base(triangulation) {}

        public AnimationDrawable Animation
        {
            get { return MakeAnimation(); }
        }

        public Bitmap CreateSweepAnimBitmap(int frame, int direction)
        {
            var frameList = makeSweepPointsFrame(frame, direction);
            var frameBitmap = drawPointFrame(frameList);
            return frameBitmap;
        }

        public AnimationDrawable MakeAnimation()
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42 * 2;//roughly how many milliseconds each frame will be for 24fps
            var direction = Geometry.get360Direction();

            for (int i = 0; i < numFrames; i++)
            {
                Bitmap frameBitmap = CreateSweepAnimBitmap(i, direction);
                
                BitmapDrawable frame = new BitmapDrawable(frameBitmap);

                animation.AddFrame(frame, duration);
            }

            return animation;
        }

        internal List<PointF>[] makeSweepPointsFrame(int frameNum, int direction)
        {
            var framePoints = new List<PointF>();
            //all the points will move within 15 degrees of the same general direction
            direction = Geometry.getAngleInRange(direction, 15);
            //workingFrameList is set either to the initial List<PointF>[] of points, or the one passed into this method
            List<PointF>[] workingFrameList = new List<PointF>[FramedPoints.Length];
            for (int i = 0; i < FramedPoints.Length; i++)
            {
                workingFrameList[i] = new List<PointF>();
                workingFrameList[i].AddRange(FramedPoints[i]);
            }

            foreach (var point in workingFrameList[frameNum])
            {
                //created bc cant modify point
                var wPoint = new PointF(point.X, point.Y);

                var distCanMove = shortestDistanceFromPoints(wPoint, workingFrameList, direction, frameNum);
                var xComponent = Geometry.getXComponent(direction, distCanMove);
                var yComponent = Geometry.getYComponent(direction, distCanMove);

                wPoint.X += (float)xComponent;
                wPoint.Y += (float)yComponent;
                framePoints.Add(wPoint);
            }
            workingFrameList[frameNum] = framePoints;

            for (int i = 0; i < numFrames; i++)
            {
                workingFrameList[i].AddRange(WideFramedPoints[i]);
            }

            return workingFrameList;
        }
        
        internal Bitmap drawPointFrame(List<PointF>[] frameList)
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

            var convertedPoints = new List<DelaunayTriangulator.Vertex>();
            //can we just stay in PointF's?
            foreach (var frame in frameList)
            {
                foreach (var point in frame)
                {
                    var currentlyExists = convertedPoints.Exists(x =>
                        x.x.CompareTo(point.X) == 0 &&
                        x.y.CompareTo(point.Y) == 0
                    );
                    if (!currentlyExists)
                        convertedPoints.Add(new DelaunayTriangulator.Vertex(point.X, point.Y));
                }
            }
            var angulator = new Triangulator();
            var triangulatedPoints = angulator.Triangulation(convertedPoints);
            for (int i = 0; i < triangulatedPoints.Count; i++)
            {
                var a = new PointF(convertedPoints[triangulatedPoints[i].a].x, convertedPoints[triangulatedPoints[i].a].y);
                var b = new PointF(convertedPoints[triangulatedPoints[i].b].x, convertedPoints[triangulatedPoints[i].b].y);
                var c = new PointF(convertedPoints[triangulatedPoints[i].c].x, convertedPoints[triangulatedPoints[i].c].y);

                Path trianglePath = drawTrianglePath(a, b, c);

                var center = Geometry.centroid(triangulatedPoints[i], convertedPoints);

                paint.Color = getTriangleColor(gradient, center);

                canvas.DrawPath(trianglePath, paint);
            }
            return drawingCanvas;
        }
    }
}
