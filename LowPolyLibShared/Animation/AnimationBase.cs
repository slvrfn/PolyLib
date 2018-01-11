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
		internal readonly int numFrames;
	    internal int CurrentFrame = 0;
        internal Dictionary<SKPointI,HashSet<SKPoint>> SeperatedPoints;
		internal Dictionary<Vertex, HashSet<Triad>> poTriDic => CurrentTriangulation.pointToTriangleDic;
	    internal double bleed_x => CurrentTriangulation.bleed_x;
	    internal double bleed_y => CurrentTriangulation.bleed_y;

	    protected Triangulation CurrentTriangulation;

        internal Geometry.RotatedGrid GridRotation;

	    internal List<Triad> triangulatedPoints => CurrentTriangulation.TriangulatedPoints;

        internal SKSurface Gradient => CurrentTriangulation.Gradient;
		internal List<Vertex> InternalPoints => CurrentTriangulation.InternalPoints;

	    internal AnimationTypes.Type AnimationType;

	    protected readonly SKPaint strokePaint, fillPaint;

        internal bool IsSetup = false;

	    protected int boundsWidth => CurrentTriangulation.BoundsWidth;
	    protected int boundsHeight => CurrentTriangulation.BoundsHeight;
        #endregion

		#region Constructor
		protected AnimationBase(Triangulation _triangulation, int frames)
		{
		    numFrames = frames;
            CurrentTriangulation = _triangulation;
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
        }

		internal abstract HashSet<AnimatedPoint> RenderFrame();

        internal virtual void DrawPointFrame(SKSurface surface, List<AnimatedPoint> pointChanges)
		{
            using (var canvas = surface.Canvas)
			{
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
			    canvas.Clear();

                WatchMeasure(watch, $"Canvas clear");
                var trianglePath = new SKPath();
                using (trianglePath)
		        {
                    trianglePath.FillType = SKPathFillType.EvenOdd;

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

		                updatedPoints[index] = new Tuple<Vertex, Vertex>(
		                    new Vertex(animatedPoint.Point.X + animatedPoint.XDisplacement,
		                        animatedPoint.Point.Y + animatedPoint.YDisplacement), InternalPoints[index]);
		                //mark this point's index as used
		                updatedIndices[i] = index;
		            }

		            WatchMeasure(watch, $"Frame points updated");
                    //increment updated points
		            foreach (var updatedPoint in updatedPoints)
		            {
                        //non-updated points will be null
		                if (updatedPoint == null)
		                    continue;

		                //increment each triad that contains this updatedPoint
		                foreach (var tri in poTriDic[updatedPoint.Item2])
                        {
                            var a = new SKPoint();
                            GetCorrectPoint(updatedPoints, updatedIndices, tri.a, ref a);
                            var b = new SKPoint();
                            GetCorrectPoint(updatedPoints, updatedIndices, tri.b, ref b);
                            var c = new SKPoint();
                            GetCorrectPoint(updatedPoints, updatedIndices, tri.c, ref c);

                            var center = Geometry.centroid(tri, InternalPoints);

                            var triAngleColorCenter = Geometry.KeepInPicBounds(center, bleed_x, bleed_y, boundsWidth, boundsHeight);
                            fillPaint.Color = CurrentTriangulation.GetTriangleColor(triAngleColorCenter);
                            Geometry.DrawTrianglePath(ref trianglePath, a, b, c);
                            canvas.DrawPath(trianglePath, fillPaint);
                        }
                    }
                    WatchMeasure(watch, $"path drawing");
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
            GridRotation = Geometry.createGridTransformation(angle, boundsWidth, boundsHeight, numFrames);

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
			var tris = poTriDic[v];

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
