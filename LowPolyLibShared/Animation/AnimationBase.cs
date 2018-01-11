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
		#region Global Variables
		internal readonly int NumFrames;
	    internal int CurrentFrame = 0;
        internal Dictionary<SKPointI,HashSet<SKPoint>> SeperatedPoints;
		internal Dictionary<Vertex, HashSet<Triad>> PoTriDic => CurrentTriangulation.pointToTriangleDic;
	    internal double BleedX => CurrentTriangulation.bleed_x;
	    internal double BleedY => CurrentTriangulation.bleed_y;

	    protected readonly Triangulation CurrentTriangulation;

        internal Geometry.RotatedGrid GridRotation;
		internal List<Vertex> InternalPoints => CurrentTriangulation.InternalPoints;

	    internal AnimationTypes.Type AnimationType;

	    protected readonly SKPaint strokePaint, fillPaint;

        internal bool IsSetup = false;

	    //pull out variables that can be reused to prevent excessive mallocs
	    protected SKPoint PathPointA;
	    protected SKPoint PathPointB;
	    protected SKPoint PathPointC;
	    protected SKPoint Center;
	    //vertex and its original vertex in InternalPoints
	    private Tuple<Vertex, Vertex>[] _updatedPoints;
	    protected SKPath TrianglePath;

        protected int BoundsWidth => CurrentTriangulation.BoundsWidth;
	    protected int BoundsHeight => CurrentTriangulation.BoundsHeight;
        #endregion

		#region Constructor
		protected AnimationBase(Triangulation triangulation, int frames)
		{
		    NumFrames = frames;
            CurrentTriangulation = triangulation;
            SeperatedPoints = new Dictionary<SKPointI, HashSet<SKPoint>>();

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

        internal virtual void SetupAnimation()
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
            _updatedPoints = new Tuple<Vertex, Vertex>[InternalPoints.Count];
            TrianglePath = new SKPath {FillType = SKPathFillType.EvenOdd};
        }

		internal abstract HashSet<AnimatedPoint> RenderFrame();

        internal virtual void DrawPointFrame(SKSurface surface, List<AnimatedPoint> pointChanges)
		{
            using (var canvas = surface.Canvas)
			{
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
			    canvas.Clear();
                
                //case in frame immediately after animation has completed, nothing needs to be drawn
			    if (CurrentFrame > NumFrames)
			        return;

                WatchMeasure(watch, $"Canvas clear");

                //only reallocate if necessary
                if (_updatedPoints.Length != InternalPoints.Count)
                {
                    _updatedPoints = new Tuple<Vertex, Vertex>[InternalPoints.Count];
                }

                //for quick lookup to check if a specified point index has been modified
                var updatedIndices = new int[pointChanges.Count];

			    for (var i = 0; i < pointChanges.Count; i++)
                {
                    var animatedPoint = pointChanges[i];

                    //find index of animated point in InternalPoints
                    var index = InternalPoints.FindIndex(v =>
                        v.x.Equals(animatedPoint.Point.X) && v.y.Equals(animatedPoint.Point.Y));

                    //only malloc if null or item2 is different
                    if (_updatedPoints[index] != null && _updatedPoints[index].Item2.Equals(InternalPoints[index]))
                    {
                        _updatedPoints[index].Item1.x = animatedPoint.Point.X + animatedPoint.XDisplacement;
                        _updatedPoints[index].Item1.y = animatedPoint.Point.Y + animatedPoint.YDisplacement;
                    }
                    else
                    {
                        _updatedPoints[index] = new Tuple<Vertex, Vertex>(
                            new Vertex(animatedPoint.Point.X + animatedPoint.XDisplacement,
                                animatedPoint.Point.Y + animatedPoint.YDisplacement), InternalPoints[index]);
                    }

                    //mark this point's index as used
                    updatedIndices[i] = index;
                }

                WatchMeasure(watch, $"Frame points updated");

                //increment updated points
                foreach (var updatedPoint in _updatedPoints)
                {
                    //non-updated points will be null
                    if (updatedPoint == null)
                        continue;

                    //increment each triad that contains this updatedPoint
                    foreach (var tri in PoTriDic[updatedPoint.Item2])
                    {
                        GetCorrectPoint(_updatedPoints, updatedIndices, tri.a, ref PathPointA);
                        GetCorrectPoint(_updatedPoints, updatedIndices, tri.b, ref PathPointB);
                        GetCorrectPoint(_updatedPoints, updatedIndices, tri.c, ref PathPointC);

                        Geometry.centroid(tri, InternalPoints, ref Center);
                        //triAngleColorCenter
                        Geometry.KeepInPicBounds(ref Center, BleedX, BleedY, BoundsWidth, BoundsHeight);
                        fillPaint.Color = CurrentTriangulation.GetTriangleColor(Center);
                        Geometry.DrawTrianglePath(ref TrianglePath, PathPointA, PathPointB, PathPointC);
                        canvas.DrawPath(TrianglePath, fillPaint);
                    }
                }
                WatchMeasure(watch, $"path drawing");
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

        protected void WatchMeasure(System.Diagnostics.Stopwatch watch, string s)
        {
            //watch.Stop();
            Console.WriteLine(s + $" took: {watch.ElapsedTicks} ticks");
            watch.Restart();
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

		internal float shortestDistanceFromPoints(SKPoint workingPoint)
		{
			//this list consists of all the triangles containing the point.
            var v = new Vertex(workingPoint.X, workingPoint.Y);
			var tris = PoTriDic[v];

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
