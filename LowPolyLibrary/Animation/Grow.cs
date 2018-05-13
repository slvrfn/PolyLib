using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelaunayTriangulator;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    public class Grow : AnimationBase
    {
        private bool[] _pointUsed;
        private Queue<Vertex> _animateList;
        private HashSet<AnimatedPoint> _totalAnimatedPoints;

        private bool _backgroundNeedsToBeSet = true;

        public Grow(Triangulation triangulation, int numFrames) : base(triangulation, numFrames)
        {
            _totalAnimatedPoints = new HashSet<AnimatedPoint>();
            _pointUsed = new bool[InternalPoints.Count];
            for (int i = 0; i < _pointUsed.Length; i++)
            {
                _pointUsed[i] = false;
            }
        }

        public override void SetupAnimation()
        {
            base.SetupAnimation();
            //visible rec so that the start of the anim is from a point visible on screen
            var visibleRecX = Random.Rand.Next(NumFrames);
            var visibleRecY = Random.Rand.Next(NumFrames);

            var recIndex = new SKPointI(visibleRecX, visibleRecY);

            //keep geting a random index until one exists
            while (!SeperatedPoints.ContainsKey(recIndex))
            {
                visibleRecX = Random.Rand.Next(NumFrames);
                visibleRecY = Random.Rand.Next(NumFrames);
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
            _pointUsed[indexT] = true;

            _animateList = new Queue<Vertex>();
            _animateList.Enqueue(point);

            IsSetup = true;
        }

        public override void DrawPointFrame(SKSurface surface, List<AnimatedPoint> edgeFrameList)
        {
            using (var canvas = surface.Canvas)
            {
                //case in frame immediately after animation has completed, nothing needs to be drawn
                if (CurrentFrame >= NumFrames)
                    canvas.Clear();
                else if (_backgroundNeedsToBeSet)
                {
                    canvas.Clear(SKColors.Black);
                    _backgroundNeedsToBeSet = false;
                }

                var thisFrame = edgeFrameList.Select((input) => new Vertex(input.Point.X, input.Point.Y));

                foreach (var updatedPoint in thisFrame)
                {
                    //increment each triad that contains this updatedPoint
                    foreach (var tri in PointToTriangleDic[updatedPoint])
                    {
                        PathPointA.X = InternalPoints[tri.a].x;
                        PathPointA.Y = InternalPoints[tri.a].y;
                        PathPointB.X = InternalPoints[tri.b].x;
                        PathPointB.Y = InternalPoints[tri.b].y;
                        PathPointC.X = InternalPoints[tri.c].x;
                        PathPointC.Y = InternalPoints[tri.c].y;

                        Geometry.centroid(tri, InternalPoints, ref Center);

                        CurrentTriangulation.KeepInBounds(ref Center);
                        fillPaint.Color = CurrentTriangulation.GetTriangleColor(Center);
                        Geometry.DrawTrianglePath(ref TrianglePath, PathPointA, PathPointB, PathPointC);
                        canvas.DrawPath(TrianglePath, fillPaint);
                        if (HideLines)
                        {
                            //need to maintain the strokepaint reguardless if we are just hiding its display
                            var backup = strokePaint.Color;
                            strokePaint.Color = fillPaint.Color;
                            canvas.DrawPath(TrianglePath, strokePaint);
                            strokePaint.Color = backup;
                        }
                        else
                        {
                            canvas.DrawPath(TrianglePath, strokePaint);
                        }
                    }
                }
            }
        }

        public override HashSet<AnimatedPoint> RenderFrame(int currentFrame)
        {
            var outPoints = new HashSet<AnimatedPoint>();

            for (int i = 0; i < InternalPoints.Count / NumFrames; i++)
            {
                var tempEdges = new HashSet<AnimatedPoint>();

                //frame may not have any points to draw in the frame
                if (_animateList.Count == 0)
                    return new HashSet<AnimatedPoint>();

                var currentPoint = _animateList.Dequeue();

                //save the first point
                outPoints.Add(new AnimatedPoint(currentPoint));

                var drawList = new HashSet<Triad>();
                drawList = PointToTriangleDic[currentPoint];
                foreach (var tri in drawList)
                {
                    //if the point is not used
                    if (!_pointUsed[tri.a])
                    {
                        //the point is now used
                        _pointUsed[tri.a] = true;

                        //if currentPoint is not equal to the tri vertex
                        if (!currentPoint.Equals(InternalPoints[tri.a]))
                        {
                            //work on the point next iteration
                            _animateList.Enqueue(InternalPoints[tri.a]);
                            //save the point
                            tempEdges.Add(new AnimatedPoint(InternalPoints[tri.a]));
                        }
                    }
                    if (!_pointUsed[tri.b])
                    {
                        _pointUsed[tri.b] = true;

                        if (!currentPoint.Equals(InternalPoints[tri.b]))
                        {
                            _animateList.Enqueue(InternalPoints[tri.b]);
                            tempEdges.Add(new AnimatedPoint(InternalPoints[tri.b]));
                        }
                    }
                    if (!_pointUsed[tri.c])
                    {
                        _pointUsed[tri.c] = true;

                        if (!currentPoint.Equals(InternalPoints[tri.c]))
                        {
                            _animateList.Enqueue(InternalPoints[tri.c]);
                            tempEdges.Add(new AnimatedPoint(InternalPoints[tri.c]));
                        }
                    }

                }
                //add the points from this iteration to the animation frame's list
                //tolist to ensure list copy
                outPoints.UnionWith(tempEdges.ToList());
            }
            _totalAnimatedPoints = outPoints;

            return _totalAnimatedPoints;
        }
    }
}
