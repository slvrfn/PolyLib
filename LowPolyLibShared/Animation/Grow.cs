using System;
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
		List<AnimatedPoint> TotalAnimatedPoints;

        private bool previouslySetup;

        internal Grow(Triangulation triangulation): base(triangulation) 
		{
			AnimationType = AnimationTypes.Type.Grow;

			TotalAnimatedPoints = new List<AnimatedPoint>();
			pointUsed = new bool[InternalPoints.Count];
			for (int i = 0; i < pointUsed.Length; i++)
			{
				pointUsed[i] = false;
			}
		}

        internal override void SetupAnimation()
        {
            base.SetupAnimation();

            var rand = new Random();
			//visible rec so that the start of the anim is from a point visible on screen
			var visibleRecIndex = rand.Next(FramedPoints.Length);
			//index of a randoom point on the random visible rec
			var index = rand.Next(FramedPoints[visibleRecIndex].Count);
			//pointF version of the point
			var pointT = FramedPoints[visibleRecIndex][index];
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

        internal override void DrawPointFrame(SKSurface surface, List<AnimatedPoint> edgeFrameList)
        {
            using (var canvas = surface.Canvas)
            {
                using (var paint = new SKPaint())
                {
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
                            paint.Color = Geometry.GetTriangleColor(Gradient, triAngleColorCenter);

                            using (SKPath trianglePath = Geometry.DrawTrianglePath(a, b, c))
                            {
                                canvas.DrawPath(trianglePath, paint);
                            }
                        }
                    }
                }
                
            }
		}

        internal override List<AnimatedPoint> RenderFrame()
        {
			var outPoints = new List<AnimatedPoint>();

			for (int i = 0; i < InternalPoints.Count / numFrames; i++)
			{
                var tempEdges = new List<AnimatedPoint>();

                var currentPoint = animateList.Dequeue();

				//save the first point
				outPoints.Add(new AnimatedPoint(currentPoint));

                var drawList = new List<Triad>();
                drawList = poTriDic[new SKPoint(currentPoint.x, currentPoint.y)];
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
                outPoints.AddRange(tempEdges.ToList());
            }
			TotalAnimatedPoints.AddRange(outPoints);
			AnimatedPoints = TotalAnimatedPoints;
			return TotalAnimatedPoints;
        }
    }
}
