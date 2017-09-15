﻿using System.Collections.Generic;
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
		internal int numFrames = 12;
		internal int CurrentFrame = 0;
		internal List<SKPoint>[] FramedPoints;
		internal List<SKPoint>[] WideFramedPoints;
		internal Dictionary<SKPoint, List<Triad>> poTriDic = new Dictionary<SKPoint, List<Triad>>();
		internal double bleed_x, bleed_y;

		internal List<Triad> triangulatedPoints;
		internal SKSurface Gradient;
		internal List<DelaunayTriangulator.Vertex> InternalPoints;

		internal List<cRectangleF[]> viewRectangles;

		internal AnimationTypes.Type AnimationType;

		internal List<AnimatedPoint> AnimatedPoints;

        internal bool IsSetup = false;

		public int boundsWidth;
		public int boundsHeight;
        #endregion

		#region Constructor
		protected AnimationBase(Triangulation triangulation)
		{
            bleed_x = triangulation.bleed_x;
			bleed_y = triangulation.bleed_y;
			InternalPoints = triangulation.InternalPoints;
			Gradient = triangulation.Gradient;
			triangulatedPoints = triangulation.TriangulatedPoints;
			boundsHeight = triangulation.BoundsHeight;
			boundsWidth = triangulation.BoundsWidth;

			FramedPoints = new List<SKPoint>[numFrames];
			WideFramedPoints = new List<SKPoint>[numFrames];
		}
        #endregion

        #region Animation Methods

        internal virtual void SetupAnimation()
        {
			var direction = Geometry.get360Direction();
			seperatePointsIntoRectangleFrames(InternalPoints, direction);
			divyTris(InternalPoints);
        }

		internal abstract List<AnimatedPoint> RenderFrame();

		internal virtual void DrawPointFrame(SKSurface surface, List<AnimatedPoint> pointChanges)
		{
			using (var canvas = surface.Canvas)
		    {
		        using (var paint = new SKPaint())
		        {
		            paint.IsAntialias = true;
                    paint.Style = SKPaintStyle.StrokeAndFill;

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
		                var a = new SKPoint(convertedPoints[newTriangulatedPoints[i].a].x, convertedPoints[newTriangulatedPoints[i].a].y);
		                var b = new SKPoint(convertedPoints[newTriangulatedPoints[i].b].x, convertedPoints[newTriangulatedPoints[i].b].y);
		                var c = new SKPoint(convertedPoints[newTriangulatedPoints[i].c].x, convertedPoints[newTriangulatedPoints[i].c].y);

		                var center = Geometry.centroid(newTriangulatedPoints[i], convertedPoints);

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
		#endregion

		#region Animation Helper Functions
		internal void seperatePointsIntoRectangleFrames(List<DelaunayTriangulator.Vertex> points, int angle)
		{
			viewRectangles = Geometry.createRectangleOverlays(angle, numFrames, boundsWidth, boundsHeight);
			FramedPoints = new List<SKPoint>[numFrames];
			WideFramedPoints = new List<SKPoint>[numFrames];

			//if this number is above zero it means a point is not being captured in a rectangle
			//should break(debug) if greater than 0
			var missingPoints = 0;

			for (int i = 0; i < FramedPoints.Length; i++)
			{
				FramedPoints[i] = new List<SKPoint>();
				WideFramedPoints[i] = new List<SKPoint>();
			}

			foreach (var point in points)
			{
				var newPoint = new SKPoint();
				newPoint.X = point.x;
				newPoint.Y = point.y;

				var missing = true;

				for (int i = 0; i < viewRectangles[1].Length; i++)
				{
					//if the rectangle overlay contains a point
					if (viewRectangles[0][i].Contains(newPoint))
					{
						missing = false;
						//if the point has not already been added to the overlay's point list
						if (!FramedPoints[i].Contains(newPoint))
							//add it
							FramedPoints[i].Add(newPoint);
					}
					//if overlays[i] does not contain the point, but wideOverlays does, add it. (The point lies outside the visible area and still needs to be maintained).
					else if (viewRectangles[1][i].Contains(newPoint))
					{
						missing = false;
						//if the point has not already been added to the overlay's point list
						if (!WideFramedPoints[i].Contains(newPoint))
							//add it
							WideFramedPoints[i].Add(newPoint);
					}
				}
				if (missing)
					++missingPoints;
			}
		}

		internal double shortestDistanceFromPoints(SKPoint workingPoint)
		{
			//this list consists of all the triangles containing the point.
			var tris = poTriDic[workingPoint];

			//shortest distance between a workingPoint and all vertices of the given triangle list
			double shortest = -1;
			foreach (var tri in tris)
			{
				//get distances between a workingPoint and the close triangle vertices
				double vertDistance = double.MinValue;

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

		private void divyTris(SKPoint point, int arrayLoc)
		{
			//if the point/triList distionary has a point already, add that triangle to the list at that key(point)
			if (poTriDic.ContainsKey(point))
				poTriDic[point].Add(triangulatedPoints[arrayLoc]);
			//if the point/triList distionary doesnt not have a point, initialize it, and add that triangle to the list at that key(point)
			else
			{
				poTriDic[point] = new List<Triad>();
				poTriDic[point].Add(triangulatedPoints[arrayLoc]);
			}
		}

		internal void divyTris(List<DelaunayTriangulator.Vertex> points)
		{
			for (int i = 0; i < triangulatedPoints.Count; i++)
			{
				var a = new SKPoint(points[triangulatedPoints[i].a].x, points[triangulatedPoints[i].a].y);
				var b = new SKPoint(points[triangulatedPoints[i].b].x, points[triangulatedPoints[i].b].y);
				var c = new SKPoint(points[triangulatedPoints[i].c].x, points[triangulatedPoints[i].c].y);

				//animation logic
				divyTris(a, i);
				divyTris(b, i);
				divyTris(c, i);
			}
		}

		#endregion
	}
}