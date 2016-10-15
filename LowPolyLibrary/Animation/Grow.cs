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

        public AnimationDrawable Animation
        {
            get { return makeAnimation(); }
        }

        public List<Bitmap> createGrowAnimBitmap()
        {
            var frameList = makeGrowFrame(_points);
            var frameBitmaps = drawPointFrame(frameList);
            return frameBitmaps;
        }

        private List<Bitmap> drawPointFrame(List<List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>> edgeFrameList)
        {
            var outBitmaps = new List<Bitmap>();

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;
            paint.Color = Android.Graphics.Color.Crimson;
            paint.StrokeWidth = 5f;

            //foreach (var frame in edgeFrameList)
            for (int j = 0; j < edgeFrameList.Count; j++)
            {
                Bitmap drawingCanvas;
                //TODO this trycatch is temp to avoid out of memory on grow animation
                try
                {
                    drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Argb4444);
                }
                catch (Exception)
                {

                    continue;
                }
                //Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Argb4444);

                Canvas canvas = new Canvas(drawingCanvas);

                for (int i = 0; i < edgeFrameList[j].Count; i++)
                {
                    var point1 = new PointF(edgeFrameList[j][i].Item1.x, edgeFrameList[j][i].Item1.y);
                    var point2 = new PointF(edgeFrameList[j][i].Item2.x, edgeFrameList[j][i].Item2.y);
                    Path path = drawPath(point1, point2);
                    canvas.DrawPath(path, paint);
                }

                //foreach (var point in frame)
                //{
                //	var point1 = new PointF(point.Item1.x, point.Item1.y);
                //	var point2 = new PointF(point.Item2.x, point.Item2.y);
                //	Path path = drawPath(point1, point2);
                //	canvas.DrawPath(path, paint);
                //}
                outBitmaps.Add(drawingCanvas);
            }

            return outBitmaps;
        }

        public AnimationDrawable makeAnimation()
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

        internal List<List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>> makeGrowFrame(List<DelaunayTriangulator.Vertex> generatedPoints)
        {
            var outEdges = new List<List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>>();
            var edgeHolder = new List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>();
            //for (int i = 0; i < 30; i++)
            //{
            //	outEdges.Add(new List<Tuple<Vertex, Vertex>>());
            //}
            //for tracking which points have been used
            bool[] pointUsed = new bool[generatedPoints.Count];
            for (int i = 0; i < pointUsed.Length; i++)
            {
                pointUsed[i] = false;
            }

            var rand = new Random();
            var index = rand.Next(generatedPoints.Count);
            var point = generatedPoints[index];
            pointUsed[index] = true;

            var animateList = new Queue<DelaunayTriangulator.Vertex>();

            animateList.Enqueue(point);
            var odd = 0;
            while (animateList.Count > 0)
            {
                var tempEdges = new List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>();

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

                        //if p is not equal to the tri vertex
                        if (!currentPoint.Equals(generatedPoints[tri.a]))
                        {
                            //work on the point next iteration
                            animateList.Enqueue(generatedPoints[tri.a]);
                            //create an edge
                            var edge = new Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>(currentPoint, generatedPoints[tri.a]);
                            //save the edge
                            tempEdges.Add(edge);
                        }
                    }
                    if (!pointUsed[tri.b])
                    {
                        pointUsed[tri.b] = true;

                        if (!currentPoint.Equals(generatedPoints[tri.b]))
                        {
                            animateList.Enqueue(generatedPoints[tri.b]);
                            var edge = new Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>(currentPoint, generatedPoints[tri.b]);
                            tempEdges.Add(edge);
                        }
                    }
                    if (!pointUsed[tri.c])
                    {
                        pointUsed[tri.c] = true;

                        if (!currentPoint.Equals(generatedPoints[tri.c]))
                        {
                            animateList.Enqueue(generatedPoints[tri.c]);
                            var edge = new Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>(currentPoint, generatedPoints[tri.c]);
                            tempEdges.Add(edge);
                        }
                    }

                }
                //add the edges from this iteration to the animation frame's edge list
                //tolist to ensure list copy
                edgeHolder.AddRange(tempEdges.ToList());
                odd++;
                if (odd > 5)
                {
                    outEdges.Add(edgeHolder.ToList());
                    edgeHolder.Clear();
                    odd = 0;
                }

                //if not the first frame, add the edges from the frame before so that we are not only displaying the growth
                //if(i>0)
                //    outEdges[i].AddRange(outEdges[i-1]);
            }
            //this makes it not just show the growth
            for (int i = 0; i < outEdges.Count; i++)
            {
                if (i > 0)
                    outEdges[i].AddRange(outEdges[i - 1]);
            }

            return outEdges;
        }

        //internal List<List<Tuple<DelaunayTriangulator.Vertex,DelaunayTriangulator.Vertex>>> makeGrowFrame(List<DelaunayTriangulator.Vertex> generatedPoints, bool onlyGrowth)
        //{
        //	var outEdges = new List<List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>>();
        //    for (int i = 0; i < 30; i++)
        //    {
        //        outEdges.Add(new List<Tuple<Vertex, Vertex>>());
        //    }
        //          //for tracking which points have been used
        //    bool[] pointUsed = new bool[generatedPoints.Count];
        //    for (int i = 0; i < pointUsed.Length; i++)
        //    {
        //        pointUsed[i] = false;
        //    }

        //	var rand = new Random();
        //          var index = rand.Next(generatedPoints.Count);
        //          var point = generatedPoints[index];
        //    pointUsed[index] = true;

        //	var animateList = new List<DelaunayTriangulator.Vertex>();
        //	var nextTime = new List<DelaunayTriangulator.Vertex>();

        //	nextTime.Add(point);
        //	while (nextTime.Count > 0)
        //	{
        //		var tempEdges =new List<Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>>();
        //		DelaunayTriangulator.Vertex currentPoint = null;
        //              animateList.Clear();
        //		animateList.AddRange(nextTime);
        //		nextTime.Clear();
        //	    for (int i = 0; i < animateList.Count; i++)
        //		{
        //                  currentPoint = animateList[i];
        //                  var drawList = new List<Triad>();
        //                  try
        //                  {
        //                      drawList = poTriDic[new PointF(animateList[i].x, animateList[i].y)];
        //                  }
        //                  catch (Exception)
        //                  {

        //                  }
        //                  foreach (var tri in drawList)
        //                  {
        //                      //if the point is not used
        //                      if (!pointUsed[tri.a])
        //                      {
        //                          //the point is now used
        //                          pointUsed[tri.a] = true;

        //                          //if p is not equal to the tri vertex
        //                          if (!animateList[i].Equals(generatedPoints[tri.a]))
        //                          {
        //						//work on the point next iteration
        //						nextTime.Add(generatedPoints[tri.a]);
        //                              //create an edge
        //                              var edge = new Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>(animateList[i], generatedPoints[tri.a]);
        //                              //save the edge
        //                              tempEdges.Add(edge);
        //                          }
        //                      }
        //                      if (!pointUsed[tri.b])
        //                      {
        //                          pointUsed[tri.b] = true;

        //                          if (!animateList[i].Equals(generatedPoints[tri.b]))
        //                          {
        //						nextTime.Add(generatedPoints[tri.b]);
        //                              var edge = new Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>(animateList[i], generatedPoints[tri.b]);
        //                              tempEdges.Add(edge);
        //                          }
        //                      }
        //                      if (!pointUsed[tri.c])
        //                      {
        //                          pointUsed[tri.c] = true;

        //                          if (!animateList[i].Equals(generatedPoints[tri.c]))
        //                          {
        //						nextTime.Add(generatedPoints[tri.c]);
        //                              var edge = new Tuple<DelaunayTriangulator.Vertex, DelaunayTriangulator.Vertex>(animateList[i], generatedPoints[tri.c]);
        //                              tempEdges.Add(edge);
        //                          }
        //                      }

        //                  }
        //                  //add the edges from this iteration to the animation frame's edge list
        //			if(i<30)
        //		    	outEdges[i].AddRange(tempEdges);
        //                  //if not the first frame, add the edges from the frame before so that we are not only displaying the growth
        //                  //if(i>0)
        //                  //    outEdges[i].AddRange(outEdges[i-1]);
        //		}
        //		if (onlyGrowth)
        //			animateList.Remove(currentPoint);
        //	}
        //	return outEdges;
        //}
    }
}
