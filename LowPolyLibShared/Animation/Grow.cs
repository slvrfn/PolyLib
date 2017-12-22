﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    class Grow : AnimationBase
    {
		bool[] pointUsed;
		Queue<Vertex> animateList;
		HashSet<AnimatedPoint> TotalAnimatedPoints;

        internal Grow(Triangulation triangulation, int numFrames): base(triangulation, numFrames) 
		{
			AnimationType = AnimationTypes.Type.Grow;

			TotalAnimatedPoints = new HashSet<AnimatedPoint>();
			pointUsed = new bool[InternalPoints.Count];
			for (int i = 0; i < pointUsed.Length; i++)
			{
				pointUsed[i] = false;
			}
		}

        internal override void SetupAnimation()
        {
            base.SetupAnimation();
			//visible rec so that the start of the anim is from a point visible on screen
			var visibleRecX = Random.Rand.Next(numFrames);
            var visibleRecY = Random.Rand.Next(numFrames);

            var recIndex = new SKPointI(visibleRecX, visibleRecY);

            //keep geting a random index until one exists
            while (!SeperatedPoints.ContainsKey(recIndex))
            {
                visibleRecX = Random.Rand.Next(numFrames);
                visibleRecY = Random.Rand.Next(numFrames);
                recIndex = new SKPointI(visibleRecX, visibleRecY);
            }

            //index of a randoom point on the random visible rec
            var index = Random.Rand.Next(SeperatedPoints[recIndex].Count);
			//pointF version of the point
            var pointT = SeperatedPoints[recIndex].ToArray()[index];
			//vertex version of the point
			var point = new Vertex(pointT.X, pointT.Y);
			//index of the chosen point in the overall points list
			var indexT = InternalPoints.IndexOf(point);
			//set the first point as used
			pointUsed[indexT] = true;

			animateList = new Queue<Vertex>();
			animateList.Enqueue(point);

            IsSetup = true;
        }

        internal override void DrawPointFrame(SKSurface surface, List<AnimatedPoint> edgeFrameList, bool ignorePointChanges = false)
        {
            using (var canvas = surface.Canvas)
            {
                canvas.Clear();
                var trianglePath = new SKPath();

                using(trianglePath)
                using (var paint = new SKPaint())
                {
                    trianglePath.FillType = SKPathFillType.EvenOdd;
                    paint.Style = SKPaintStyle.StrokeAndFill;
                    paint.IsAntialias = true;

                    var thisFrame = edgeFrameList.Select((input) => { return new Vertex(input.Point.X, input.Point.Y); });

                    foreach (var tri in triangulatedPoints)
                    {
                        if (thisFrame.Contains(InternalPoints[tri.a]) && thisFrame.Contains(InternalPoints[tri.b]) && thisFrame.Contains(InternalPoints[tri.c]))
                        {
                            var a = new SKPoint(InternalPoints[tri.a].x, InternalPoints[tri.a].y);
                            var b = new SKPoint(InternalPoints[tri.b].x, InternalPoints[tri.b].y);
                            var c = new SKPoint(InternalPoints[tri.c].x, InternalPoints[tri.c].y);

                            var center = Geometry.centroid(tri, InternalPoints);

                            var triAngleColorCenter = Geometry.KeepInPicBounds(center, bleed_x, bleed_y, boundsWidth, boundsHeight);
                            paint.Color = CurrentTriangulation.GetTriangleColor(triAngleColorCenter);
                            Geometry.DrawTrianglePath(ref trianglePath, a, b, c);
                            canvas.DrawPath(trianglePath, paint);
                        }
                    }
                }
            }
		}

        internal override HashSet<AnimatedPoint> RenderFrame()
        {
			var outPoints = new HashSet<AnimatedPoint>();

			for (int i = 0; i < InternalPoints.Count / numFrames; i++)
			{
                var tempEdges = new HashSet<AnimatedPoint>();

                var currentPoint = animateList.Dequeue();

				//save the first point
				outPoints.Add(new AnimatedPoint(currentPoint));

                var drawList = new HashSet<Triad>();
                drawList = poTriDic[currentPoint];
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
							tempEdges.Add(new AnimatedPoint(InternalPoints[tri.a]));
                        }
                    }
                    if (!pointUsed[tri.b])
                    {
                        pointUsed[tri.b] = true;

                        if (!currentPoint.Equals(InternalPoints[tri.b]))
                        {
                            animateList.Enqueue(InternalPoints[tri.b]);
							tempEdges.Add(new AnimatedPoint(InternalPoints[tri.b]));
                        }
                    }
                    if (!pointUsed[tri.c])
                    {
                        pointUsed[tri.c] = true;

                        if (!currentPoint.Equals(InternalPoints[tri.c]))
                        {
                            animateList.Enqueue(InternalPoints[tri.c]);
							tempEdges.Add(new AnimatedPoint(InternalPoints[tri.c]));
                        }
                    }

                }
                //add the points from this iteration to the animation frame's list
                //tolist to ensure list copy
                outPoints.UnionWith(tempEdges.ToList());
            }
			TotalAnimatedPoints.UnionWith(outPoints);
			return TotalAnimatedPoints;
        }
    }
}
