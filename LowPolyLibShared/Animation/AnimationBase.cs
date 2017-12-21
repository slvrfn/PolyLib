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

		internal abstract List<AnimatedPoint> RenderFrame();

		internal virtual void DrawPointFrame(SKSurface surface, List<AnimatedPoint> pointChanges)
		{
			using (var canvas = surface.Canvas)
			{
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
			    //canvas.Clear();

                WatchMeasure(watch, $"Canvas clear");
                var trianglePath = new SKPath();
                using (var paint = new SKPaint())
                using (trianglePath)
		        {
                    trianglePath.FillType = SKPathFillType.EvenOdd;
		            paint.IsAntialias = true;
                    paint.Style = SKPaintStyle.StrokeAndFill;
                    //ensure copy of internal points because it will be modified
                    //var convertedPoints = InternalPoints.ToList();
                    var convertedPoints = new List<Vertex>();

                    WatchMeasure(watch, $"InternalPoints.ToList");
		            //can we just stay in PointF's?
		            foreach (var animatedPoint in pointChanges)
		            {
		                var oldPoint = new Vertex(animatedPoint.Point.X, animatedPoint.Point.Y);
		                var newPoint = new Vertex(oldPoint.x + animatedPoint.XDisplacement, oldPoint.y + animatedPoint.YDisplacement);
		                //convertedPoints.Remove(oldPoint);
		                convertedPoints.Add(newPoint);
		            }
                    WatchMeasure(watch, $"Converted points update");
		            var angulator = new Triangulator();
		            var newTriangulatedPoints = angulator.Triangulation(convertedPoints);
                    WatchMeasure(watch, $"new triangulation");
		            for (int i = 0; i < newTriangulatedPoints.Count; i++)
		            {
		                var a = new SKPoint(convertedPoints[newTriangulatedPoints[i].a].x, convertedPoints[newTriangulatedPoints[i].a].y);
		                var b = new SKPoint(convertedPoints[newTriangulatedPoints[i].b].x, convertedPoints[newTriangulatedPoints[i].b].y);
		                var c = new SKPoint(convertedPoints[newTriangulatedPoints[i].c].x, convertedPoints[newTriangulatedPoints[i].c].y);

		                var center = Geometry.centroid(newTriangulatedPoints[i], convertedPoints);

		                var triAngleColorCenter = Geometry.KeepInPicBounds(center, bleed_x, bleed_y, boundsWidth, boundsHeight);
		                paint.Color = CurrentTriangulation.GetTriangleColor(triAngleColorCenter);
                        Geometry.DrawTrianglePath(ref trianglePath, a, b, c);
                        canvas.DrawPath(trianglePath, paint);

		            }
                    WatchMeasure(watch, $"path drawing");
                }
            }
		}

        private void WatchMeasure(System.Diagnostics.Stopwatch watch, string s)
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
