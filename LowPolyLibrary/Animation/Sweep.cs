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
		private int Direction = -1;

        internal Sweep(Triangulation triangulation): base(triangulation) {}

        public AnimationDrawable Animation
        {
            get {  Direction = Geometry.get360Direction();
				return MakeAnimation(); }
        }

		public Bitmap CreateBitmap()
        {
            var frameList = makeSweepPointsFrame();
            var frameBitmap = drawPointFrame(frameList);
            return frameBitmap;
        }

        public AnimationDrawable MakeAnimation()
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42 * 2;//roughly how many milliseconds each frame will be for 24fps
            

            for (int i = 0; i < numFrames; i++)
            {
				CurrentFrame = i;
                Bitmap frameBitmap = CreateBitmap();
                
                BitmapDrawable frame = new BitmapDrawable(frameBitmap);

                animation.AddFrame(frame, duration);
            }

            return animation;
        }

		internal List<AnimatedPoint> makeSweepPointsFrame()
        {
			var animatedPoints = new List<AnimatedPoint>();
            //all the points will move within 15 degrees of the same direction
            var localDirection = Geometry.getAngleInRange(Direction, 15);

            foreach (var point in FramedPoints[CurrentFrame])
            {
                var distCanMove = shortestDistanceFromPoints(point);
                var xComponent = Geometry.getXComponent(localDirection, distCanMove);
                var yComponent = Geometry.getYComponent(localDirection, distCanMove);
				var p = new AnimatedPoint(point, (float)xComponent, (float)yComponent);
                animatedPoints.Add(p);
            }

			return animatedPoints;
        }
        
		internal Bitmap drawPointFrame(List<AnimatedPoint> pointChanges)
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

			//ensure copy of internal points because it will be modified
			var convertedPoints = InternalPoints.ToList();
            //can we just stay in PointF's?
			foreach (var animatedPoint in pointChanges)
            {
				var oldPoint = new Vertex(animatedPoint.Point.X, animatedPoint.Point.Y);
				var newPoint = new Vertex(oldPoint.x + animatedPoint.XDisplacement, oldPoint.y + animatedPoint.YDisplacement);
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
