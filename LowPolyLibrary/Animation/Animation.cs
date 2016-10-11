using System.Drawing;
using System.Collections.Generic;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Math = System.Math;
using PointF = System.Drawing.PointF;
using System;
using System.Linq;

namespace LowPolyLibrary
{
    public class Animation : Triangulation
    {
        internal static int numFrames = 12; //static necessary for creation of framedPoints list
        internal List<PointF>[] framedPoints = new List<PointF>[numFrames];
        internal List<PointF>[] wideFramedPoints = new List<PointF>[numFrames];
        internal Dictionary<PointF, List<Triad>> poTriDic = new Dictionary<PointF, List<Triad>>();
        public List<Triad> triangulatedPoints;

		internal List<cRectangleF[]> viewRectangles;
        internal Touch.TouchPoints touchPointLists;


        public enum Animations
		{
			Sweep,Touch,Grow
		}

		internal void seperatePointsIntoRectangleFrames(List<DelaunayTriangulator.Vertex> points, int boundsWidth, int boundsHeight, int angle)
        {
            viewRectangles = createRectangleOverlays(boundsWidth, boundsHeight, angle);
            framedPoints = new List<PointF>[numFrames];
            wideFramedPoints = new List<PointF>[numFrames];

            //if this number is above zero it means a point is not being captured in a rectangle
            //should break(debug) if greater than 0
            var missingPoints = 0;

            for (int i = 0; i < framedPoints.Length; i++)
            {
                framedPoints[i] = new List<PointF>();
                wideFramedPoints[i] = new List<PointF>();
            }

            foreach (var point in points)
            {
                var newPoint = new PointF();
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
                        if (!framedPoints[i].Contains(newPoint))
                            //add it
                            framedPoints[i].Add(newPoint);
                    }
                    //if overlays[i] does not contain the point, but wideOverlays does, add it. (The point lies outside the visible area and still needs to be maintained).
                    else if (viewRectangles[1][i].Contains(newPoint))
                    {
                        missing = false;
                        //if the point has not already been added to the overlay's point list
                        if (!wideFramedPoints[i].Contains(newPoint))
                            //add it
                            wideFramedPoints[i].Add(newPoint);
                    }
                }
                if (missing)
                    ++missingPoints;
            }
        }
        
        internal List<DelaunayTriangulator.Vertex> makeTrisFrame(int frameNum, List<DelaunayTriangulator.Vertex> points)
        {
            //temporary copy of the frame's points. This copy will serve as a 'frame' in the animationFrames array
            var tempPoList = new List<DelaunayTriangulator.Vertex>(points);
            //get array of points contained in a specified frame
            var pointList = framedPoints[frameNum];

            var direction = Geometry.get360Direction();

            foreach (var workingPoint in pointList)
            {
                //get list of tris at given workingPoint in given frame
                var tris = new List<Triad>(poTriDic[workingPoint]);
				var distCanMove = frameLocation(frameNum, numFrames, shortestDistanceFromTris(workingPoint, tris, direction, points));
                var xComponent = Geometry.getXComponent(direction, distCanMove);
                var yComponent = Geometry.getYComponent(direction, distCanMove);
                foreach (var triangle in tris)
                {
                    //animate each triangle
                    //triangle.animate();
                    if (points[triangle.a].x.CompareTo(workingPoint.X) == 0 && points[triangle.a].y.CompareTo(workingPoint.Y) == 0)
                    {
                        points[triangle.a].x += (float)xComponent;//frameLocation(frameNum, totalFrames, xComponent);
                        points[triangle.a].y += (float)yComponent;//frameLocation(frameNum, totalFrames, yComponent);
                    }
                    else if (points[triangle.b].x.CompareTo(workingPoint.X) == 0 && points[triangle.b].y.CompareTo(workingPoint.Y) == 0)
                    {
                        points[triangle.b].x += (float)xComponent;//frameLocation(frameNum, totalFrames, xComponent);
                        points[triangle.b].y += (float)yComponent;//frameLocation(frameNum, totalFrames, yComponent);
                    }
                    else if (points[triangle.c].x.CompareTo(workingPoint.X) == 0 && points[triangle.c].y.CompareTo(workingPoint.Y) == 0)
                    {
                        points[triangle.c].x += (float)xComponent;//frameLocation(frameNum, totalFrames, xComponent);
                        points[triangle.c].y += (float)yComponent;//frameLocation(frameNum, totalFrames, yComponent);
                    }
                }
            }
            return points;
        }

        internal double frameLocation(int frame, int totalFrames, Double distanceToCcover)
        {
            //method to be used when a final destination is known, and you want to get a proportional distance to have moved up to that point at this point in time
            var ratioToFinalMovement = frame / (Double)totalFrames;
            var thisCoord = ratioToFinalMovement * distanceToCcover;
            return thisCoord;
        }
        
        internal List<PointF> quadListFromPoints(List<PointF>[] framePoints, int degree, PointF workingPoint, int frameNum)
        {
			var pointList = new List<PointF>();
			////this frame checking handles adding all the points that are close to a working point
			if (frameNum == 0)
			{
				pointList.AddRange(framePoints[frameNum]);
				pointList.AddRange(framePoints[frameNum + 1]);
			}
			else if (frameNum == numFrames - 1)
			{
				pointList.AddRange(framePoints[frameNum]);
			    pointList.AddRange(framePoints[frameNum - 1]);
			}
			else
			{
				pointList.AddRange(framePoints[frameNum]);
				pointList.AddRange(framePoints[frameNum + 1]);
				pointList.AddRange(framePoints[frameNum - 1]);
			}

			var direction = 0;

            if (degree > 270)
                direction = 4;
            else if (degree > 180)
                direction = 3;
            else if (degree > 90)
                direction = 2;
            else
                direction = 1;

            var quad1 = new List<PointF>();
            var quad2 = new List<PointF>();
            var quad3 = new List<PointF>();
            var quad4 = new List<PointF>();

            foreach (var point in pointList)
            {
                //if x,y of new triCenter > x,y of working point, then in the 1st quardant
                if (point.X > workingPoint.X && point.Y > workingPoint.Y)
                    quad1.Add(point);
                else if (point.X < workingPoint.X && point.Y > workingPoint.Y)
                    quad2.Add(point);
                else if (point.X > workingPoint.X && point.Y < workingPoint.Y)
                    quad4.Add(point);
                else if (point.X < workingPoint.X && point.Y < workingPoint.Y)
                    quad3.Add(point);
            }
            switch (direction)
            {
                case 1:
                    return quad1;
                case 2:
                    return quad2;
                case 3:
                    return quad3;
                case 4:
                    return quad4;
                default:
                    return quad1;
            }

        }

		internal List<PointF> quadListFromPoints(List<PointF> framePoints, int degree, PointF workingPoint)
		{
			var direction = 0;

			if (degree > 270)
				direction = 4;
			else if (degree > 180)
				direction = 3;
			else if (degree > 90)
				direction = 2;
			else
				direction = 1;

			var quad1 = new List<PointF>();
			var quad2 = new List<PointF>();
			var quad3 = new List<PointF>();
			var quad4 = new List<PointF>();

			foreach (var point in framePoints)
			{
				//if x,y of new triCenter > x,y of working point, then in the 1st quardant
				if (point.X > workingPoint.X && point.Y > workingPoint.Y)
					quad1.Add(point);
				else if (point.X < workingPoint.X && point.Y > workingPoint.Y)
					quad2.Add(point);
				else if (point.X > workingPoint.X && point.Y < workingPoint.Y)
					quad4.Add(point);
				else if (point.X < workingPoint.X && point.Y < workingPoint.Y)
					quad3.Add(point);
			}
			switch (direction)
			{
				case 1:
					return quad1;
				case 2:
					return quad2;
				case 3:
					return quad3;
				case 4:
					return quad4;
				default:
					return quad1;
			}

		}

        internal List<Triad> quadListFromTris(List<Triad> tris, int degree, PointF workingPoint, List<DelaunayTriangulator.Vertex> points)
        {
            var direction = 0;

            if (degree > 270)
                direction = 4;
            else if (degree > 180)
                direction = 3;
            else if (degree > 90)
                direction = 2;
            else
                direction = 1;

            var quad1 = new List<Triad>();
            var quad2 = new List<Triad>();
            var quad3 = new List<Triad>();
            var quad4 = new List<Triad>();

            foreach (var tri in tris)
            {
                //var angle = getAngle(workingPoint, centroid(tri));
                var triCenter = Geometry.centroid(tri, points);
                //if x,y of new triCenter > x,y of working point, then in the 1st quardant
                if (triCenter.X > workingPoint.X && triCenter.Y > workingPoint.Y)
                    quad1.Add(tri);
                else if (triCenter.X < workingPoint.X && triCenter.Y > workingPoint.Y)
                    quad2.Add(tri);
                else if (triCenter.X > workingPoint.X && triCenter.Y < workingPoint.Y)
                    quad3.Add(tri);
                else if (triCenter.X > workingPoint.X && triCenter.Y < workingPoint.Y)
                    quad4.Add(tri);
            }
            switch (direction)
            {
                case 1:
                    return quad1;
                case 2:
                    return quad2;
                case 3:
                    return quad3;
                case 4:
                    return quad4;
                default:
                    return quad1;
            }

        }

        internal double shortestDistanceFromPoints(PointF workingPoint, List<PointF>[] framePoints, int degree, int frameNum)
        {
			//this list consists of all the points in the same directional quardant as the working point.
			var quadPoints = quadListFromPoints(framePoints, degree, workingPoint, frameNum);//just changed to quad points

            //shortest distance between a workingPoint and all points of a given list
            double shortest = -1;
            foreach (var point in quadPoints)
            {
                //get distances between a workingPoint and the point
                var vertDistance = Geometry.dist(workingPoint, point);

                //if this is the first run (shortest == -1) then tempShortest is the vertDistance
                if (shortest.CompareTo(-1) == 0) //if shortest == -1
                    shortest = vertDistance;
                //if not the first run, only assign shortest if vertDistance is smaller
                else
                    if (vertDistance < shortest && vertDistance.CompareTo(0) != 0)//if the vertDistance < current shortest distance and not equal to 0
                    shortest = vertDistance;
            }
            return shortest;
        }

		internal double shortestDistanceFromPoints(PointF workingPoint, List<PointF> framePoints, int degree)
		{
			//this list consists of all the points in the same directional quardant as the working point.
			var quadPoints = quadListFromPoints(framePoints, degree, workingPoint);//just changed to quad points

			//shortest distance between a workingPoint and all points of a given list
			double shortest = -1;
			foreach (var point in quadPoints)
			{
				//get distances between a workingPoint and the point
				var vertDistance = Geometry.dist(workingPoint, point);

				//if this is the first run (shortest == -1) then tempShortest is the vertDistance
				if (shortest.CompareTo(-1) == 0) //if shortest == -1
					shortest = vertDistance;
				//if not the first run, only assign shortest if vertDistance is smaller
				else
					if (vertDistance < shortest && vertDistance.CompareTo(0) != 0)//if the vertDistance < current shortest distance and not equal to 0
					shortest = vertDistance;
			}
			return shortest;
		}

        internal double shortestDistanceFromTris(PointF workingPoint, List<Triad> tris, int degree, List<DelaunayTriangulator.Vertex> points)
        {
            var quadTris = quadListFromTris(tris, degree, workingPoint, points);

            //shortest distance between a workingPoint and all points of a tri
            double shortest = -1;
            foreach (var tri in quadTris)
            {
                //get distances between a workingPoint and each vertex of a tri
                var vert1Distance = Geometry.dist(workingPoint, points[tri.a]);
                var vert2Distance = Geometry.dist(workingPoint, points[tri.a]);
                var vert3Distance = Geometry.dist(workingPoint, points[tri.a]);

                double tempShortest;
                //only one vertex distance can be 0. So if vert1 is 0, assign vert 2 for initial distance comparrison
                //(will be changed later if there is a shorter distance)
                if (vert1Distance.CompareTo(0) == 0) // if ver1Distance == 0
                    tempShortest = vert2Distance;
                else
                    tempShortest = vert1Distance;
                //if a vertex distance is less than the current tempShortest and not 0, it is the new shortest distance
                if (vert1Distance < tempShortest && vert1Distance.CompareTo(0) == 0)// or if vertice == 0
                    tempShortest = vert1Distance;
                if (vert2Distance < tempShortest && vert2Distance.CompareTo(0) == 0)// or if vertice == 0
                    tempShortest = vert2Distance;
                if (vert3Distance < tempShortest && vert3Distance.CompareTo(0) == 0)// or if vertice == 0
                    tempShortest = vert3Distance;
                //tempshortest is now the shortest distance between a workingPoint and tri vertices, save it
                //if this is the first run (shortest == -1) then tempShortest is the smalled distance
                if (shortest.CompareTo(-1) == 0) //if shortest == -1
                    shortest = tempShortest;
                //if not the first run, only assign shortest if tempShortest is smaller
                else
                    if (tempShortest < shortest && tempShortest.CompareTo(0) != 0)//if the tempShortest < current shortest distance and not equal to 0
                    shortest = tempShortest;

            }
            return shortest;
        }

        internal void divyTris(System.Drawing.PointF point, int arrayLoc)
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
                var a = new PointF(points[triangulatedPoints[i].a].x, points[triangulatedPoints[i].a].y);
                var b = new PointF(points[triangulatedPoints[i].b].x, points[triangulatedPoints[i].b].y);
                var c = new PointF(points[triangulatedPoints[i].c].x, points[triangulatedPoints[i].c].y);

                //animation logic
                divyTris(a, i);
                divyTris(b, i);
                divyTris(c, i);
            }
        }

        internal List<cRectangleF[]> createRectangleOverlays(int boundsWidth, int boundsHeight, int angle)
        {
            //array size numFrames of rectangles. each array entry serves as a rotated cRectangleF
            cRectangleF[] frames = new cRectangleF[numFrames];

            //slope of the given angle
			var slope = (float)Math.Tan(Geometry.degreesToRadians(angle));
            var recipSlope = -1/slope;

			PointF ADIntersection;
			PointF DCIntersection;
			var drawingAreaA = new PointF(0, boundsHeight);
			var drawingAreaB = new PointF(boundsWidth, boundsHeight);
			var drawingAreaC = new PointF(boundsWidth, 0);
			var drawingAreaD = new PointF(0, 0);

            PointF cornerA;
            PointF cornerB;
            PointF cornerC;
            PointF cornerD;

            if (angle < 90)
            {
                //quad1
                cornerA = drawingAreaA;
                cornerB = drawingAreaB;
                cornerC = drawingAreaC;
                cornerD = drawingAreaD;
            }
            else if (angle < 180)
            {
                //quad2
                cornerA = drawingAreaD;
                cornerB = drawingAreaA;
                cornerC = drawingAreaB;
                cornerD = drawingAreaC;
            }
            else if (angle < 270)
            {
                //quad3
                cornerA = drawingAreaC;
                cornerB = drawingAreaD;
                cornerC = drawingAreaA;
                cornerD = drawingAreaB;
            }
            else
            {
                //quad4
                cornerA = drawingAreaB;
                cornerB = drawingAreaC;
                cornerC = drawingAreaD;
                cornerD = drawingAreaA;
            }

            ADIntersection = Geometry.getIntersection(slope, cornerA, cornerD);
            DCIntersection = Geometry.getIntersection(recipSlope, cornerD, cornerC);
            //ABIntersection used to calculate framewidth
            var ABIntersection = Geometry.getIntersection(slope, cornerA, cornerB);
            var frameWidth = (float)Geometry.dist(ADIntersection, ABIntersection)/numFrames;
            var wideOverlays = createWideRectangleOverlays(frameWidth, ADIntersection, DCIntersection, angle,boundsWidth, boundsHeight);

            var walkedB = Geometry.walkAngle(angle, frameWidth, ADIntersection);
            var walkedC = Geometry.walkAngle(angle, frameWidth, DCIntersection);
            frames[0] = new cRectangleF
            {
                A = new PointF(ADIntersection.X, ADIntersection.Y),
                B = new PointF(walkedB.X, walkedB.Y),
                C = new PointF(walkedC.X, walkedC.Y),
                D = new PointF(DCIntersection.X, DCIntersection.Y)
            };

            //starts from second array entry because first entry is assigned above
            for (int i = 1; i < numFrames; i++)
            {
                var overlay = new cRectangleF();
                overlay.A = frames[i - 1].B;
                overlay.D = frames[i - 1].C;
                overlay.B = Geometry.walkAngle(angle, frameWidth, overlay.A);
                overlay.C = Geometry.walkAngle(angle, frameWidth, overlay.D);
                frames[i] = overlay;
            }
            var returnList = new List<cRectangleF[]>();
            returnList.Add(frames);
            returnList.Add(wideOverlays);
            return returnList;
        }

        internal cRectangleF[] createWideRectangleOverlays(float frameWidth, PointF A, PointF D, int angle, int boundsWidth, int boundsHeight)
        {
            //first and last rectangles need to be wider to cover points that are outside to the left and right of the pic bounds
            //all rectangles need to be higher and lower than the pic bounds to cover points above and below the pic bounds

            //array size numFrames of rectangles. each array entry serves as a rectangle(i) starting from the left
            cRectangleF[] frames = new cRectangleF[numFrames];
            

            //represents the corner A of the regular overlays
            var overlayA = new PointF(A.X, A.Y);
            var overlayD = new PointF(D.X, D.Y);

            var tempWidth = boundsWidth / 2;
            var tempHeight = boundsHeight / 2;

            frames[0] = new cRectangleF();
            frames[0].A = Geometry.walkAngle(angle +90, tempHeight, overlayA);
            frames[0].B = Geometry.walkAngle(angle, frameWidth, frames[0].A);
            frames[0].A = Geometry.walkAngle(angle + 180, tempWidth, frames[0].A);
            frames[0].D = Geometry.walkAngle(angle + 270, tempHeight, overlayD);
            frames[0].C = Geometry.walkAngle(angle, frameWidth, frames[0].D);
            frames[0].D = Geometry.walkAngle(angle + 180, tempWidth, frames[0].D);


            //this logic is for grabbing all points (even those outside the visible drawing area)
            //starts at 1 cause first array spot handled above
            for (int i = 1; i < numFrames; i++)
            {
                cRectangleF overlay = new cRectangleF();
                if (i == numFrames - 1)
                {
                    overlay.A = new PointF(frames[i - 1].B.X, frames[i - 1].B.Y);
                    overlay.D = new PointF(frames[i - 1].C.X, frames[i - 1].C.Y);
                    overlay.B = Geometry.walkAngle(angle, frameWidth + tempWidth, overlay.A);
                    overlay.C = Geometry.walkAngle(angle, frameWidth + tempWidth, overlay.D);
                }
                else
                {
                    overlay.A = new PointF(frames[i - 1].B.X, frames[i - 1].B.Y);
                    overlay.D = new PointF(frames[i - 1].C.X, frames[i - 1].C.Y);
                    overlay.B = Geometry.walkAngle(angle, frameWidth, overlay.A);
                    overlay.C = Geometry.walkAngle(angle, frameWidth, overlay.D);
                }
                frames[i] = overlay;
            }

            return frames;
        }
    }
}
