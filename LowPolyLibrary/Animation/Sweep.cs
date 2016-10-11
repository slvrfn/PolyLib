using System;
using System.Collections.Generic;
using System.Text;
using Android.Graphics;
using DelaunayTriangulator;
using PointF = System.Drawing.PointF;

namespace LowPolyLibrary
{
    class Sweep : Animation
    {
        public Bitmap createSweepAnimBitmap(int frame, int direction)
        {
            var frameList = makeSweepPointsFrame(frame, direction);
            var frameBitmap = drawPointFrame(frameList);
            return frameBitmap;
        }

        internal List<PointF>[] makeSweepPointsFrame(int frameNum, int direction)
        {
            var framePoints = new List<PointF>();
            //all the points will move within 15 degrees of the same general direction
            direction = Geometry.getAngleInRange(direction, 15);
            //workingFrameList is set either to the initial List<PointF>[] of points, or the one passed into this method
            List<PointF>[] workingFrameList = new List<PointF>[framedPoints.Length];
            for (int i = 0; i < framedPoints.Length; i++)
            {
                workingFrameList[i] = new List<PointF>();
                workingFrameList[i].AddRange(framedPoints[i]);
            }

            foreach (var point in workingFrameList[frameNum])
            {
                //created bc cant modify point
                var wPoint = new PointF(point.X, point.Y);

                var distCanMove = shortestDistanceFromPoints(wPoint, workingFrameList, direction, frameNum);
                var xComponent = Geometry.getXComponent(direction, distCanMove);
                var yComponent = Geometry.getYComponent(direction, distCanMove);

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
    }
}
