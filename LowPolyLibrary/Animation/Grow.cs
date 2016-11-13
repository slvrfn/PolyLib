using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using Android.Graphics.Drawables;
using DelaunayTriangulator;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
    class Grow : Animation
    {
        internal Grow(Triangulation triangulation): base(triangulation) {}

        public AnimationDrawable Animation => makeAnimation();

        private List<Bitmap> createGrowAnimBitmap()
        {
            var frameList = makeGrowFrame();
            var frameBitmaps = drawPointFrame(frameList);
            return frameBitmaps;
        }

        private List<Bitmap> drawPointFrame(List<List<DelaunayTriangulator.Vertex>> edgeFrameList)
        {
            var outBitmaps = new List<Bitmap>();

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

            //foreach (var frame in edgeFrameList)
            for (int i = 0; i < edgeFrameList.Count; i++)
            {
                Bitmap drawingCanvas;
#warning Trycatch for bitmap memory error
                //TODO this trycatch is temp to avoid out of memory on grow animation
                try
                {
                    drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Argb4444);
                }
                catch (Exception)
                {

                    continue;
                }
                Canvas canvas = new Canvas(drawingCanvas);

                var thisFrame = new List<Vertex>();

                for (int j = 0; j <= i; j++)
                {
                    thisFrame.AddRange(edgeFrameList[j]);
                }

                foreach (var tri in triangulatedPoints)
                {
                    if (thisFrame.Contains(InternalPoints[tri.a]) && thisFrame.Contains(InternalPoints[tri.b]) && thisFrame.Contains(InternalPoints[tri.c]))
                    {
                        var a = new PointF(InternalPoints[tri.a].x, InternalPoints[tri.a].y);
                        var b = new PointF(InternalPoints[tri.b].x, InternalPoints[tri.b].y);
                        var c = new PointF(InternalPoints[tri.c].x, InternalPoints[tri.c].y);

                        Path trianglePath = drawTrianglePath(a, b, c);

                        var center = Geometry.centroid(tri, InternalPoints);

                        paint.Color = getTriangleColor(Gradient, center);

                        canvas.DrawPath(trianglePath, paint);
                    }
                }
                outBitmaps.Add(drawingCanvas);
            }

            return outBitmaps;
        }

        private AnimationDrawable makeAnimation()
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42 * 2;//roughly how many milliseconds each frame will be for 24fps

            List<Bitmap> frameBitmaps = createGrowAnimBitmap();
            foreach (var frame in frameBitmaps)
            {
                BitmapDrawable conv = new BitmapDrawable(frame);
                animation.AddFrame(conv, duration);
            }
            return animation;
        }

        private List<List<Vertex>> makeGrowFrame()
        {
            var outEdges = new List<List<Vertex>>();
            var pointHolder = new List<Vertex>();

            bool[] pointUsed = new bool[InternalPoints.Count];
            for (int i = 0; i < pointUsed.Length; i++)
            {
                pointUsed[i] = false;
            }

            var rand = new Random();
            //visible rec so that the start of the anim is from a point visible on screen
            var visibleRecIndex = rand.Next(FramedPoints.Length);
            //index of a randoom point on the random visible rec
            var index = rand.Next(FramedPoints[visibleRecIndex].Count);
            //pointF version of the point
            var pointT = FramedPoints[visibleRecIndex][index];
            //vertex version of the point
            var point = new Vertex(pointT.X,pointT.Y);
            //index of the chosen point in the overall points list
            var indexT = InternalPoints.IndexOf(point);
            //set the first point as used
            pointUsed[indexT] = true;
            //save the first point
            pointHolder.Add(point);

            var animateList = new Queue<Vertex>();
            animateList.Enqueue(point);
            while (animateList.Count > 0)
            {
                var tempEdges = new List<Vertex>();

                var currentPoint = animateList.Dequeue();
                var drawList = new List<Triad>();
                try
                {
                    drawList = poTriDic[new PointF(currentPoint.x, currentPoint.y)];
                }
                catch (Exception)
                {

                }
                foreach (var tri in drawList)
                {
                    //if the point is not used
                    if (!pointUsed[tri.a])
                    {
                        //the point is now used
                        pointUsed[tri.a] = true;

                        //if currentPoint is not equal to the tri vertex
                        if (!currentPoint.Equals(InternalPoints[tri.a]))
                        {
                            //work on the point next iteration
                            animateList.Enqueue(InternalPoints[tri.a]);
                            //save the point
                            tempEdges.Add(InternalPoints[tri.a]);
                        }
                    }
                    if (!pointUsed[tri.b])
                    {
                        pointUsed[tri.b] = true;

                        if (!currentPoint.Equals(InternalPoints[tri.b]))
                        {
                            animateList.Enqueue(InternalPoints[tri.b]);
                            tempEdges.Add(InternalPoints[tri.b]);
                        }
                    }
                    if (!pointUsed[tri.c])
                    {
                        pointUsed[tri.c] = true;

                        if (!currentPoint.Equals(InternalPoints[tri.c]))
                        {
                            animateList.Enqueue(InternalPoints[tri.c]);
                            tempEdges.Add(InternalPoints[tri.c]);
                        }
                    }

                }
                //add the points from this iteration to the animation frame's list
                //tolist to ensure list copy
                pointHolder.AddRange(tempEdges.ToList());
                //if pointholder has more points than the average number of points per number of frames
                if (pointHolder.Count>InternalPoints.Count/numFrames)
                {
                    outEdges.Add(pointHolder.ToList());
                    pointHolder.Clear();
                }
            }
            //if there are any points left over that weren't added (x<InternalPoints.Count/numFrames)
            if (pointHolder.Count > 0)
            {
                outEdges.Add(pointHolder.ToList());
                pointHolder.Clear();
            }

            return outEdges;
        }
    }
}
