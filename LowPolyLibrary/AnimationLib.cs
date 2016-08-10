using System.Drawing;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Java.Lang;
using Java.Util;
using Triangulator = DelaunayTriangulator.Triangulator;
using Triad = DelaunayTriangulator.Triad;
using Double = System.Double;
using Enum = System.Enum;
using Math = System.Math;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
    public class AnimationLib
    {
        private static int numFrames = 12; //static necessary for creation of framedPoints list
        List<PointF>[] framedPoints = new List<PointF>[numFrames];
        List<PointF>[] wideFramedPoints = new List<PointF>[numFrames];
        Dictionary<PointF, List<Triad>> poTriDic = new Dictionary<PointF, List<Triad>>();
        private List<Triad> triangulatedPoints;

        internal void seperatePointsIntoRectangleFrames(List<DelaunayTriangulator.Vertex> points, int boundsWidth, int boundsHeight, int angle)
        {
            var overlays = createRectangleOverlays(boundsWidth, boundsHeight, angle);
            framedPoints = new List<PointF>[numFrames];
            wideFramedPoints = new List<PointF>[numFrames];


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

                for (int i = 0; i < overlays[1].Length; i++)
                {
                    //if the rectangle overlay contains a point
                    if (overlays[0][i].Contains(newPoint))
                    {
                        //if the point has not already been added to the overlay's point list
                        if (!framedPoints[i].Contains(newPoint))
                            //add it
                            framedPoints[i].Add(newPoint);
                    }
                    //if overlays[i] does not contain the point, but wideOverlays does, add it. (The point lies outside the visible area and still needs to be maintained).
                    else if (overlays[1][i].Contains(newPoint))
                    {
                        //if the point has not already been added to the overlay's point list
                        if (!wideFramedPoints[i].Contains(newPoint))
                            //add it
                            wideFramedPoints[i].Add(newPoint);
                    }

                }
            }
        }

        internal List<PointF>[] makeSweepPointsFrame(int frameNum, int direction, List<PointF>[] oldFramelist = null)
        {
            //temporary copy of the frame's points. This copy will serve as a 'frame' in the animationFrames array
            //var tempPoList = new List<ceometric.DelaunayTriangulator.Point>(_points);

            ////get list of points contained in a specified frame
            ////this frame checking handles adding all the points that are close to a working point
            //List<PointF> pointList = new List<PointF>();
            //if (frameNum == 0)
            //{
            //    pointList = framedPoints[frameNum];
            //    pointList.AddRange(framedPoints[frameNum + 1]);                                              possible this chunck of code could be used elsewhere later
            //}
            //else if (frameNum == totalFrames - 1)
            //{
            //    pointList = framedPoints[frameNum];
            //    pointList.AddRange(framedPoints[frameNum - 1]);
            //}
            //else
            //{
            //    pointList = framedPoints[frameNum];
            //}
            var framePoints = new List<PointF>();
            //all the points will move within 15 degrees of the same general direction
            direction = getAngleInRange(direction, 15);
            //workingFrameList is set either to the initial List<PointF>[] of points, or the one provided by calling this method
            List<PointF>[] workingFrameList = new List<PointF>[framedPoints.Length];
            if (oldFramelist == null)
            //workingFrameList = framedPoints;
            //think a direct assignment was causing issues with extra points getting added to framedPoints
            {
                for (int i = 0; i < framedPoints.Length; i++)
                {
                    workingFrameList[i] = new List<PointF>();
                    workingFrameList[i].AddRange(framedPoints[i]);
                }

            }
            else
                workingFrameList = oldFramelist;

            foreach (var point in workingFrameList[frameNum])
            {
                //created bc cant modify point
                var wPoint = new PointF(point.X, point.Y);
                //get list of tris at given workingPoint in given frame
                //var tris = tempPoTriDic[workingPoint];


                var distCanMove = shortestDistanceFromPoints(wPoint, workingFrameList[frameNum], direction);
                var xComponent = getXComponent(direction, distCanMove);
                var yComponent = getYComponent(direction, distCanMove);

                wPoint.X += (float)xComponent;
                wPoint.Y += (float)yComponent;
                framePoints.Add(wPoint);
            }
            workingFrameList[frameNum] = framePoints;

            for (int i = 0; i < numFrames; i++)
            {
                workingFrameList[i].AddRange(wideFramedPoints[i]);
            }

            return workingFrameList;
        }

        private List<DelaunayTriangulator.Vertex> makeTrisFrame(int frameNum, List<DelaunayTriangulator.Vertex> points)
        {
            //temporary copy of the frame's points. This copy will serve as a 'frame' in the animationFrames array
            var tempPoList = new List<DelaunayTriangulator.Vertex>(points);
            //get array of points contained in a specified frame
            var pointList = framedPoints[frameNum];

            var direction = get360Direction();

            foreach (var workingPoint in pointList)
            {
                //get list of tris at given workingPoint in given frame
                var tris = new List<Triad>(poTriDic[workingPoint]);

                var distCanMove = shortestDistanceFromTris(workingPoint, tris, direction, points);
                var xComponent = getXComponent(direction, distCanMove);
                var yComponent = getYComponent(direction, distCanMove);
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

        private double frameLocation(int frame, int totalFrames, Double distanceToCcover)
        {
            //method to be used when a final destination is known, and you want to get a proportional distance to have moved up to that point at this point in time
            var ratioToFinalMovement = frame / (Double)totalFrames;
            var thisCoord = ratioToFinalMovement * distanceToCcover;
            return thisCoord;
        }

        private double getXComponent(int angle, double length)
        {
            return length * Math.Cos(degreesToRadians(angle));
        }

        private double getYComponent(int angle, double length)
        {
            return length * Math.Sin(degreesToRadians(angle));
        }

        internal int get360Direction()
        {
            var rand = new System.Random();
            //return a int from 0 to 359 that represents the direction a point will move
            return rand.Next(360);
        }

        internal int getAngleInRange(int angle, int range)
        {
            var rand = new System.Random();
            var range_lower = angle - range;
            var range_upper = angle + range;
            //return a int from range_lower to range_upper that represents the direction a point will move
            //this is done to add an amount of 'variability'. Each point will travel in the same general direction, but with a little bit of 'wiggle room'
            return rand.Next(range_lower,range_upper);
        }

        private List<PointF> quadListFromPoints(List<PointF> points, int degree, PointF workingPoint)
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

            foreach (var point in points)
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

        private List<Triad> quadListFromTris(List<Triad> tris, int degree, PointF workingPoint, List<DelaunayTriangulator.Vertex> points)
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

            var polyLib = new LowPolyLib();

            foreach (var tri in tris)
            {
                //var angle = getAngle(workingPoint, centroid(tri));
                var triCenter = polyLib.centroid(tri, points);
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

        private double shortestDistanceFromPoints(PointF workingPoint, List<PointF> points, int degree)
        {
            //this list consists of all the points in the same directional quardant as the working point.
            var quadPoints = quadListFromPoints(points, degree, workingPoint);//just changed to quad points

            //shortest distance between a workingPoint and all points of a given list
            double shortest = -1;
            foreach (var point in quadPoints)
            {
                //get distances between a workingPoint and the point
                var vertDistance = dist(workingPoint, point);

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

        private double shortestDistanceFromTris(PointF workingPoint, List<Triad> tris, int degree, List<DelaunayTriangulator.Vertex> points)
        {
            var quadTris = quadListFromTris(tris, degree, workingPoint, points);

            //shortest distance between a workingPoint and all points of a tri
            double shortest = -1;
            foreach (var tri in quadTris)
            {
                //get distances between a workingPoint and each vertex of a tri
                var vert1Distance = dist(workingPoint, points[tri.a]);
                var vert2Distance = dist(workingPoint, points[tri.a]);
                var vert3Distance = dist(workingPoint, points[tri.a]);

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

        private double dist(PointF workingPoint, DelaunayTriangulator.Vertex vertex)
        {
            var xSquare = (workingPoint.X - vertex.x) * (workingPoint.X - vertex.x);
            var ySquare = (workingPoint.Y - vertex.y) * (workingPoint.Y - vertex.y);
            return Math.Sqrt(xSquare + ySquare);
        }

        internal double dist(PointF workingPoint, PointF vertex)
        {
            var xSquare = (workingPoint.X - vertex.X) * (workingPoint.X - vertex.X);
            var ySquare = (workingPoint.Y - vertex.Y) * (workingPoint.Y - vertex.Y);
            return Math.Sqrt(xSquare + ySquare);
        }

        private void divyTris(System.Drawing.PointF point, RectangleF[] overlays, int arrayLoc)
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

		private PointF getIntersection(float slope, int point2Offset, PointF perpendicularLinePoint, PointF linePoint)
        {
            var linePoint2 = new PointF();
            linePoint2.X = linePoint.X + point2Offset;
            linePoint2.Y = (slope * linePoint2.X);

            var k = ((linePoint2.Y - linePoint.Y) * (perpendicularLinePoint.X - linePoint.X) - (linePoint2.X - linePoint.X) * (perpendicularLinePoint.Y - linePoint.Y)) / ((linePoint2.Y - linePoint.Y) * (linePoint2.Y - linePoint.Y) + (linePoint2.X - linePoint.X) * (linePoint2.X - linePoint.X));
            var x4 = perpendicularLinePoint.X - k * (linePoint2.Y - linePoint.Y);
            var y4 = perpendicularLinePoint.Y + k * (linePoint2.X - linePoint.X);

            return new PointF(x4, y4);
        }

        private PointF walkLine(int angle, float distance, PointF startingPoint)
		{
			var endPoint = new PointF(startingPoint.X, startingPoint.Y);
			var y = distance * ((float)Math.Sin(degreesToRadians(angle)));
			var x = distance * ((float)Math.Cos(degreesToRadians(angle)));
			endPoint.X += x;
			endPoint.Y += y;
			return endPoint;
		}

        private List<cRectangleF[]> createRectangleOverlays(int boundsWidth, int boundsHeight, int angle)
        {
            //array size numFrames of rectangles. each array entry serves as a rotated cRectangleF
            cRectangleF[] frames = new cRectangleF[numFrames];

            var tempHeight = boundsHeight/2f;
            var x2Offset = boundsWidth;

            
			//normal visible overlay
			//var overlay = new cRectangleF
			//{
			//    A = new PointF(currentX, 0),
			//    B = new PointF(currentX + frameWidth, 0),
			//    C = new PointF(currentX + frameWidth, boundsHeight),
			//    D = new PointF(currentX, boundsHeight)
			//};
                
            //slope of the given angle
			var slope = (float)Math.Tan(degreesToRadians(angle));
            var reflectYAngle = 359-angle;

			PointF firstIntersection;
			PointF secondIntersection;
			var drawingAreaA = new PointF(0, 0);
			var drawingAreaB = new PointF(boundsWidth, 0);
			var drawingAreaC = new PointF(boundsWidth, boundsHeight);
			var drawingAreaD = new PointF(0, boundsHeight);

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
                x2Offset = -x2Offset;
            }
            else if (angle < 180)
            {
                //quad2
                cornerA = drawingAreaB;
                cornerB = drawingAreaC;
                cornerC = drawingAreaD;
                cornerD = drawingAreaA;
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
                cornerA = drawingAreaD;
                cornerB = drawingAreaA;
                cornerC = drawingAreaB;
                cornerD = drawingAreaC;
                x2Offset = -x2Offset;
            }

            firstIntersection = getIntersection(slope,x2Offset, cornerA, cornerD);
            secondIntersection = getIntersection(slope,x2Offset, cornerC, cornerD);
            var frameWidth = (float)dist(cornerD, cornerB)/numFrames;
            var wideOverlays = createWideRectangleOverlays(frameWidth, firstIntersection, secondIntersection, angle,boundsWidth, boundsHeight);

            var walkedB = walkLine(reflectYAngle, frameWidth, firstIntersection);
            var walkedC = walkLine(reflectYAngle, frameWidth, secondIntersection);
            frames[0] = new cRectangleF
            {
                A = new PointF(firstIntersection.X, firstIntersection.Y),
                B = new PointF(walkedB.X, walkedB.Y),
                C = new PointF(walkedC.X, walkedC.Y),
                D = new PointF(secondIntersection.X, secondIntersection.Y)
            };

            //starts from second array entry because first entry is assigned above
            for (int i = 1; i < numFrames; i++)
            {
                var overlay = new cRectangleF();
                overlay.A = frames[i - 1].B;
                overlay.D = frames[i - 1].C;
                overlay.B = walkLine(reflectYAngle, frameWidth, overlay.A);
                overlay.C = walkLine(reflectYAngle, frameWidth, overlay.D);

                //overlay.A = frames[i - 1].D;
                //overlay.B = frames[i - 1].C;
                //overlay.C = walkLine(recipAngle, frameWidth, overlay.B);
                //overlay.D = walkLine(recipAngle, frameWidth, overlay.A);
                frames[i] = overlay;
            }
            var returnList = new List<cRectangleF[]>();
            returnList.Add(frames);
            returnList.Add(wideOverlays);
            return returnList;
        }

        private cRectangleF[] createWideRectangleOverlays(float frameWidth, PointF D, PointF A, int angle, int boundsWidth, int boundsHeight)
        {
            //first and last rectangles need to be wider to cover points that are outside to the left and right of the pic bounds
            //all rectangles need to be higher and lower than the pic bounds to cover points above and below the pic bounds

            //array size numFrames of rectangles. each array entry serves as a rectangle(i) starting from the left
            cRectangleF[] frames = new cRectangleF[numFrames];

            var reflectYAngle = 359 - angle;

            //represents the corner A of the regular overlays
            var overlayA = new PointF(A.X, A.Y);
            var overlayD = new PointF(D.X, D.Y);

            var tempWidth = boundsWidth / 2;
            var tempHeight = boundsHeight / 2;

            frames[0] = new cRectangleF();
            frames[0].A = walkLine(reflectYAngle + 90, tempHeight, overlayA);
            frames[0].B = walkLine(reflectYAngle, frameWidth, frames[0].A);
            frames[0].A = walkLine(reflectYAngle + 180, tempWidth, frames[0].A);
            frames[0].D = walkLine(reflectYAngle + 270, tempHeight, overlayD);
            frames[0].C = walkLine(reflectYAngle, frameWidth, frames[0].D);
            frames[0].D = walkLine(reflectYAngle + 180, tempWidth, frames[0].D);


            //this logic is for grabbing all points (even those outside the visible drawing area)
            //starts at 1 cause first array spot handled above
            for (int i = 1; i < numFrames; i++)
            {
                cRectangleF overlay = new cRectangleF();
                //if the first rectangle
                //if (i == 0)
                //    overlay = new cRectangleF
                //    {
                //        A = new PointF(currentX - tempWidth, 0 - tempHeight),
                //        B = new PointF(currentX + frameWidth, 0 - tempHeight),
                //        C = new PointF(currentX + frameWidth, 0 + boundsHeight + tempHeight),
                //        D = new PointF(currentX - tempWidth, 0 + boundsHeight + tempHeight)
                //    };
                //if the last rectangle
                //if (i == numFrames - 1)
                //{
                //    overlay.A = new PointF(currentX, 0 - tempHeight),
                //    overlay.B = new PointF(currentX + frameWidth + tempWidth, 0 - tempHeight),
                //    overlay.C = new PointF(currentX + frameWidth + tempWidth, 0 + boundsHeight + tempHeight),
                //    overlay.D = new PointF(currentX, 0 + boundsHeight + tempHeight)
                //}
                if (i == numFrames - 1)
                {
                    overlay.A = new PointF(frames[i - 1].B.X, frames[i - 1].B.Y);
                    overlay.D = new PointF(frames[i - 1].C.X, frames[i - 1].C.Y);
                    overlay.B = walkLine(reflectYAngle, frameWidth + tempWidth, overlay.A);
                    overlay.C = walkLine(reflectYAngle, frameWidth + tempWidth, overlay.A);
                }
                else
                //overlay = new cRectangleF
                //   {
                //       A = new PointF(currentX, 0 - tempHeight),
                //       B = new PointF(currentX + frameWidth, 0 - tempHeight),
                //       C = new PointF(currentX + frameWidth, 0 + boundsHeight + tempHeight),
                //       D = new PointF(currentX, 0 + boundsHeight + tempHeight)
                //   };

                {
                    overlay.A = new PointF(frames[i - 1].B.X, frames[i - 1].B.Y);
                    overlay.D = new PointF(frames[i - 1].C.X, frames[i - 1].C.Y);
                    overlay.B = walkLine(reflectYAngle, frameWidth, overlay.A);
                    overlay.C = walkLine(reflectYAngle, frameWidth, overlay.D);
                }
                frames[i] = overlay;
            }

            return frames;
        }

        private double degreesToRadians(int angle)
        {
            var toRad = Math.PI/180;
            return angle*toRad;
        }
    }
}
