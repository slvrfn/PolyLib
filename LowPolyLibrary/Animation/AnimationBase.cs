using System.Collections.Generic;
using System.Collections.Concurrent;
using DelaunayTriangulator;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Math = System.Math;
using System;
using System.Linq;
using LowPolyLibrary.Threading;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    public abstract class AnimationBase
    {
        #region Variables
        public readonly int NumFrames;
        public Dictionary<SKPointI, HashSet<SKPoint>> SeperatedPoints;
        public Dictionary<Vertex, HashSet<Triad>> PointToTriangleDic => CurrentTriangulation.PointToTriangleDic;

        public readonly Triangulation CurrentTriangulation;

        public List<Vertex> InternalPoints => CurrentTriangulation.InternalPoints;

        internal bool IsSetup = false;

        protected readonly SKPaint strokePaint, fillPaint;

        //pull out variables that can be reused to prevent excessive mallocs
        protected SKPoint PathPointA;
        protected SKPoint PathPointB;
        protected SKPoint PathPointC;
        protected SKPoint Center;
        protected SKPath TrianglePath;

        public int BoundsWidth => CurrentTriangulation.BoundsWidth;
        public int BoundsHeight => CurrentTriangulation.BoundsHeight;

        public Geometry.RotatedGrid GridRotation
        {
            get;
            private set;
        }
        public int CurrentFrame{
            get;
            internal set;
        }
        public bool HideLines
        {
            get;
            internal set;
        }
        #endregion

        #region Constructor
        public AnimationBase(Triangulation triangulation, int frames)
        {
            NumFrames = frames;
            CurrentTriangulation = triangulation;
            SeperatedPoints = new Dictionary<SKPointI, HashSet<SKPoint>>();
            HideLines = triangulation.HideLines;

            strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                IsAntialias = true
            };

            //color set later
            fillPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            };
        }

        ~AnimationBase()
        {
            strokePaint.Dispose();
            fillPaint.Dispose();
            TrianglePath.Dispose();
        }
        #endregion

        #region Animation Methods

        public virtual void SetupAnimation()
        {
            var direction = Geometry.get360Direction();
            seperatePointsIntoGridCells(InternalPoints, direction);
            if (!CurrentTriangulation.HasPointsToTrianglesSetup())
            {
                CurrentTriangulation.SetupPointsToTriangles();
            }

            PathPointA = new SKPoint();
            PathPointB = new SKPoint();
            PathPointC = new SKPoint();
            Center = new SKPoint();
            TrianglePath = new SKPath { FillType = SKPathFillType.EvenOdd };
        }

        public abstract HashSet<AnimatedPoint> RenderFrame(int currentFrame);

        public virtual void DrawPointFrame(SKSurface surface, List<AnimatedPoint> pointChanges)
        {
            using (var canvas = surface.Canvas)
            {
                canvas.Clear();
                //case in frame immediately after animation has completed, nothing needs to be drawn
                if (CurrentFrame > NumFrames)
                    return;

                //vertex and its original vertex in InternalPoints
                var updatedPoints = new Tuple<Vertex, Vertex>[InternalPoints.Count];

                //for quick lookup to check if a specified point index has been modified
                var updatedIndices = new int[pointChanges.Count];

                for (var i = 0; i < pointChanges.Count; i++)
                {
                    var animatedPoint = pointChanges[i];

                    //find index of animated point in InternalPoints
                    var index = InternalPoints.FindIndex(v =>
                        v.x.Equals(animatedPoint.Point.X) && v.y.Equals(animatedPoint.Point.Y));

                    //only malloc if null or item2 is different
                    if (updatedPoints[index] != null && updatedPoints[index].Item2.Equals(InternalPoints[index]))
                    {
                        updatedPoints[index].Item1.x = animatedPoint.Point.X + animatedPoint.XDisplacement;
                        updatedPoints[index].Item1.y = animatedPoint.Point.Y + animatedPoint.YDisplacement;
                    }
                    else
                    {
                        updatedPoints[index] = new Tuple<Vertex, Vertex>(
                            new Vertex(animatedPoint.Point.X + animatedPoint.XDisplacement,
                                animatedPoint.Point.Y + animatedPoint.YDisplacement), InternalPoints[index]);
                    }

                    //mark this point's index as used
                    updatedIndices[i] = index;
                }

                //increment updated points
                foreach (var updatedPoint in updatedPoints)
                {
                    //non-updated points will be null
                    if (updatedPoint == null)
                        continue;

                    //increment each triad that contains this updatedPoint
                    foreach (var tri in PointToTriangleDic[updatedPoint.Item2])
                    {
                        GetCorrectPoint(updatedPoints, updatedIndices, tri.a, ref PathPointA);
                        GetCorrectPoint(updatedPoints, updatedIndices, tri.b, ref PathPointB);
                        GetCorrectPoint(updatedPoints, updatedIndices, tri.c, ref PathPointC);

                        Geometry.centroid(tri, InternalPoints, ref Center);
                        //triangle color center
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

        //gets the correct point to draw. Either a point updated this frame, or the original point in InternalPoints
        private void GetCorrectPoint(Tuple<Vertex, Vertex>[] updatedPoints, int[] updatedIndices, int internalPointIndex, ref SKPoint p)
        {
            if (updatedIndices.Contains(internalPointIndex))
            {
                p.X = updatedPoints[internalPointIndex].Item1.x;
                p.Y = updatedPoints[internalPointIndex].Item1.y;
            }
            else
            {
                p.X = InternalPoints[internalPointIndex].x;
                p.Y = InternalPoints[internalPointIndex].y;
            }
        }
        #endregion

        #region Animation Helper Functions

        // seperates the internal points into a logical grid of cells
        internal void seperatePointsIntoGridCells(List<DelaunayTriangulator.Vertex> points, int angle)
        {
            GridRotation = Geometry.createGridTransformation(angle, BoundsWidth, BoundsHeight, NumFrames);

            SeperatedPoints = new Dictionary<SKPointI, HashSet<SKPoint>>();

            SKPointI gridIndex = new SKPointI();

            var newPoint = new SKPoint();
            foreach (var point in points)
            {
                newPoint.X = point.x;
                newPoint.Y = point.y;

                GridRotation.CellCoordsFromOriginPoint(ref gridIndex, newPoint);

                //if the SeperatedPoints distionary does not have a point already, initialize the list at that key
                if (!SeperatedPoints.ContainsKey(gridIndex))
                    SeperatedPoints[gridIndex] = new HashSet<SKPoint>();
                SeperatedPoints[gridIndex].Add(newPoint);
            }
        }

        //adds first layer of points surrounding the points in the sourceList to the destination list, optionally allowing to limit displacement
        protected void AddConnectedPoints(SKPoint point, IEnumerable<SKPoint> sourceList, ICollection<AnimatedPoint> destinationList, bool limitDisplacement = true)
        {
            var v = new Vertex(point.X, point.Y);
            //get points v is connected to
            var triadsContaingV = PointToTriangleDic[v];

            foreach (var triad in triadsContaingV)
            {
                //check if points connected to v are not in the source list
                //if they are not, adds them to the destination list

                //no need to create SKPoint until actually added to the animated point
                if (!sourceList.Any(p => p.X.Equals(InternalPoints[triad.a].x) &&
                                         p.Y.Equals(InternalPoints[triad.a].y)))
                {
                    destinationList.Add(new AnimatedPoint(new SKPoint(InternalPoints[triad.a].x, InternalPoints[triad.a].y), limitDisplacement: limitDisplacement));
                }
                if (!sourceList.Any(p => p.X.Equals(InternalPoints[triad.b].x) &&
                                         p.Y.Equals(InternalPoints[triad.b].y)))
                {
                    destinationList.Add(new AnimatedPoint(new SKPoint(InternalPoints[triad.b].x, InternalPoints[triad.b].y), limitDisplacement: limitDisplacement));
                }
                if (!sourceList.Any(p => p.X.Equals(InternalPoints[triad.c].x) &&
                                         p.Y.Equals(InternalPoints[triad.c].y)))
                {
                    destinationList.Add(new AnimatedPoint(new SKPoint(InternalPoints[triad.c].x, InternalPoints[triad.c].y), limitDisplacement: limitDisplacement));
                }
            }
        }

        public float shortestDistanceFromPoints(SKPoint workingPoint)
        {
            //this list consists of all the triangles containing the point.
            var v = new Vertex(workingPoint.X, workingPoint.Y);
            var tris = PointToTriangleDic[v];

            //shortest distance between a workingPoint and all vertices of the given triangle list
            float shortest = -1;
            foreach (var tri in tris)
            {
                //get distances between a workingPoint and the close triangle vertices
                float vertDistance = float.MinValue;

                var vertDistance1 = Geometry.dist(workingPoint, InternalPoints[tri.a]);
                var vertDistance2 = Geometry.dist(workingPoint, InternalPoints[tri.b]);
                var vertDistance3 = Geometry.dist(workingPoint, InternalPoints[tri.c]);

                if (vertDistance1.Equals(0))
                    vertDistance = Math.Min(vertDistance2, vertDistance3);
                else if (vertDistance2.Equals(0))
                {
                    vertDistance = Math.Min(vertDistance1, vertDistance3);
                }
                else if (vertDistance3.Equals(0))
                {
                    vertDistance = Math.Min(vertDistance1, vertDistance2);
                }

                //if this is the first run (shortest == -1) then tempShortest is the vertDistance
                if (shortest.CompareTo(-1) == 0) //if shortest == -1
                    shortest = vertDistance;
                //if not the first run, only assign shortest if vertDistance is smaller
                else
                    if (vertDistance < shortest)//if the vertDistance < current shortest distance and not equal to 0
                    shortest = vertDistance;
            }
            return shortest;
        }

        #endregion
    }
}
