using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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

		public Bitmap CreateBitmap(int frame, int direction)
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
                Bitmap frameBitmap = CreateBitmap(i, direction);
                
                BitmapDrawable frame = new BitmapDrawable(frameBitmap);

                animation.AddFrame(frame, duration);
            }

            return animation;
        }

        internal List<Tuple<PointF,PointF>> makeSweepPointsFrame(int frameNum, int direction)
        {
            var framePoints = new List<Tuple<PointF, PointF>>();
            //all the points will move within 15 degrees of the same direction
            direction = Geometry.getAngleInRange(direction, 15);

            foreach (var point in FramedPoints[frameNum])
            {
                //created bc cant modify point
                var wPoint = new PointF(point.X, point.Y);

                var distCanMove = shortestDistanceFromPoints(wPoint);
                var xComponent = Geometry.getXComponent(direction, distCanMove);
                var yComponent = Geometry.getYComponent(direction, distCanMove);

                wPoint.X += (float)xComponent;
                wPoint.Y += (float)yComponent;
				var tup = new Tuple<PointF, PointF>(point,wPoint);
                framePoints.Add(tup);
            }

			return framePoints;
        }
        
		internal Bitmap drawPointFrame(List<Tuple<PointF, PointF>> pointChanges)
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

			//ensure copy of internal points because it will be modified
			var convertedPoints = InternalPoints.ToList();
            //can we just stay in PointF's?
            foreach (var point in pointChanges)
            {
				var oldPoint = new Vertex(point.Item1.X, point.Item1.Y);
				var newPoint = new Vertex(point.Item2.X, point.Item2.Y);
				convertedPoints.Remove(oldPoint);
				convertedPoints.Add(newPoint);
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

                paint.Color = getTriangleColor(Gradient, center);

                canvas.DrawPath(trianglePath, paint);
            }
            return drawingCanvas;
        }
    }
}
