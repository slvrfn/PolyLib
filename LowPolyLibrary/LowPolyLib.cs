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
	public class LowPolyLib
	{
		private Triangulator angulator = new Triangulator();
		private List<DelaunayTriangulator.Vertex> _points;
	    private List<Triad> triangulatedPoints;
        public int boundsWidth;
		public int boundsHeight;
		public double cell_size = 75;
		public double setVariance = .75;
		private double calcVariance, cells_x, cells_y;
		private double bleed_x, bleed_y;
		private static int numFrames = 12; //static necessary for creation of framedPoints list
		List<System.Drawing.PointF>[] framedPoints = new List<System.Drawing.PointF>[numFrames];
        List<System.Drawing.PointF>[] wideFramedPoints = new List<System.Drawing.PointF>[numFrames];

        Bitmap gradient;

        Dictionary<System.Drawing.PointF, List<Triad>> poTriDic = new Dictionary<System.Drawing.PointF, List<Triad>>();

		System.Random rand = new System.Random();

		public Bitmap GenerateNew()
		{
			UpdateVars();
			_points = GeneratePoints();
			return createBitmap();
		}

		private void UpdateVars()
		{
			calcVariance = cell_size * setVariance / 2;
			cells_x = Math.Floor((boundsWidth + 4 * cell_size) / cell_size);
			cells_y = Math.Floor((boundsHeight + 4 * cell_size) / cell_size);
			bleed_x = ((cells_x * cell_size) - boundsWidth) / 2;
			bleed_y = ((cells_y * cell_size) - boundsHeight) / 2;
		}

		private Bitmap createBitmap()
		{
			Bitmap drawingCanvas = Bitmap.CreateBitmap (boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
			Canvas canvas = new Canvas (drawingCanvas);

			Paint paint = new Paint();

			paint.StrokeWidth = .5f;
			paint.SetStyle (Paint.Style.FillAndStroke);
			paint.AntiAlias = true;
            
      //      var overlays = createVisibleOverlays();
      //      for (int i = 0; i < framedPoints.Length; i++)
		    //{
		    //    framedPoints[i] = new List<System.Drawing.PointF>();
		    //}

			gradient = getGradient();
			triangulatedPoints = angulator.Triangulation(_points);
			seperatePointsIntoFrames(_points);
			//generating a new base triangulation. if an old one exists get rid of it
			if (poTriDic != null)
				poTriDic = new Dictionary<System.Drawing.PointF, List<Triad>>();

			for (int i = 0; i < triangulatedPoints.Count; i++)
			{
				var a = new PointF((float)_points[triangulatedPoints[i].a].x, (float)_points[triangulatedPoints[i].a].y);
				var b = new PointF((float)_points[triangulatedPoints[i].b].x, (float)_points[triangulatedPoints[i].b].y);
				var c = new PointF((float)_points[triangulatedPoints[i].c].x, (float)_points[triangulatedPoints[i].c].y);
                
			    Path trianglePath = drawTrianglePath(a, b, c);

				var center = centroid(triangulatedPoints[i], _points);

                //animation logic
                //divyTris(a, overlays, i);
                //divyTris(b, overlays, i);
                //divyTris(c,overlays, i);

                paint.Color = getTriangleColor (gradient, center);

				canvas.DrawPath (trianglePath, paint);
			}
			return drawingCanvas;
		}

		private Bitmap drawTriFrame(Dictionary<System.Drawing.PointF, List<Triad>> frameDic)
		{
			Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
			Canvas canvas = new Canvas(drawingCanvas);

			Paint paint = new Paint();
			paint.StrokeWidth = .5f;
			paint.SetStyle(Paint.Style.FillAndStroke);
			paint.AntiAlias = true;

			foreach (KeyValuePair<System.Drawing.PointF, List<Triad>> entry in frameDic)
			{
				// do something with entry.Value or entry.Key
				var frameTriList = entry.Value;
				foreach (var tri in frameTriList)
				{
					var a = new PointF(_points[tri.a].x, _points[tri.a].y);
					var b = new PointF(_points[tri.b].x, _points[tri.b].y);
					var c = new PointF(_points[tri.c].x, _points[tri.c].y);

					Path trianglePath = drawTrianglePath(a, b, c);

					var center = centroid(tri, _points);

					paint.Color = getTriangleColor(gradient, center);

					canvas.DrawPath(trianglePath, paint);
				}
			}
			return drawingCanvas;
		}

        private Bitmap drawPointFrame(List<PointF>[] frameList)
        {
            Bitmap drawingCanvas = Bitmap.CreateBitmap(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
            Canvas canvas = new Canvas(drawingCanvas);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.FillAndStroke);
            paint.AntiAlias = true;

            var convertedPoints = new List<DelaunayTriangulator.Vertex>();
            //can we just stay in PointF's?
            foreach (var frame in frameList)
            {
                foreach (var point in frame)
                {
                    convertedPoints.Add(new DelaunayTriangulator.Vertex(point.X, point.Y));
                }
            }

            var newTriangulatedPoints = angulator.Triangulation(convertedPoints);
            for (int i = 0; i < newTriangulatedPoints.Count; i++)
            {
                var a = new PointF((float)convertedPoints[newTriangulatedPoints[i].a].x, (float)convertedPoints[newTriangulatedPoints[i].a].y);
				var b = new PointF((float)convertedPoints[newTriangulatedPoints[i].b].x, (float)convertedPoints[newTriangulatedPoints[i].b].y);
				var c = new PointF((float)convertedPoints[newTriangulatedPoints[i].c].x, (float)convertedPoints[newTriangulatedPoints[i].c].y);

                Path trianglePath = drawTrianglePath(a, b, c);

                var center = centroid(newTriangulatedPoints[i], convertedPoints);

                paint.Color = getTriangleColor(gradient, center);

                canvas.DrawPath(trianglePath, paint);
            }
            return drawingCanvas;
        }

        public Bitmap createAnimBitmap(int frame)
		{
			var frameList = makePointsFrame(frame, 24);
			var frameBitmap = drawPointFrame(frameList);
			return frameBitmap;
		}

        public AnimationDrawable makeAnimation(int numFrames2)
        {
            AnimationDrawable animation = new AnimationDrawable();
            animation.OneShot = true;
            var duration = 42*2;//roughly how many milliseconds each frame will be for 24fps
            
            for (int i = 0; i < numFrames2; i++)
            {
                var frameBitmap = createAnimBitmap(i);
                BitmapDrawable frame = new BitmapDrawable(frameBitmap);
                animation.AddFrame(frame,duration);
            }
            return animation;
        }

        private void seperatePointsIntoFrames(List<DelaunayTriangulator.Vertex> points)
		{
			var overlays = createVisibleOverlays();
		    var wideOverlays = createWideOverlays();
			framedPoints = new List<PointF>[numFrames];
            wideFramedPoints = new List<PointF>[numFrames];


            for (int i = 0; i < framedPoints.Length; i++)
			{
				framedPoints[i] = new List<System.Drawing.PointF>();
                wideFramedPoints[i] = new List<System.Drawing.PointF>();
            }

			foreach (var point in points)
			{
				var newPoint = new PointF();
				newPoint.X = point.x;
				newPoint.Y = point.y;

				for (int i = 0; i < overlays.Length; i++)
				{
					//if the rectangle overlay contains a point
					if (overlays[i].Contains(newPoint))
					{
						//if the point has not already been added to the overlay's point list
						if (!framedPoints[i].Contains(newPoint))
							//add it
							framedPoints[i].Add(newPoint);
					}
                    //if overlays[i] does not contain the point, but wideOverlays does, add it. (The point lies outside the visible area and still needs to be maintained).
                    else if (wideOverlays[i].Contains(newPoint))
                    {
                        //if the point has not already been added to the overlay's point list
                        if (!wideFramedPoints[i].Contains(newPoint))
                            //add it
                            wideFramedPoints[i].Add(newPoint);
                    }

				}
			}
		}

		private List<PointF>[] makePointsFrame(int frameNum, int totalFrames, List<PointF>[] oldFramelist = null)
	    {
			//temporary copy of the frame's points. This copy will serve as a 'frame' in the animationFrames array
			//var tempPoList = new List<ceometric.DelaunayTriangulator.Point>(_points);
			
            //get array of points contained in a specified frame
		    var framePoints = new List<PointF>();

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

            var direction = get360Direction();
            //workingFrameList is set either to the initial List<PointF>[] of points, or the one provided by calling this method
            List<PointF>[] workingFrameList = new List<PointF>[framedPoints.Length];
		    if (oldFramelist == null)
                //workingFrameList = framedPoints;
                //think a direct assignment was causing issues with extra points getting added to framedPoints
                framedPoints.CopyTo(workingFrameList,0);
		    else
		        workingFrameList = oldFramelist;

            foreach (var point in workingFrameList[frameNum])
			{
                //created bc cant modify point
                var wPoint = new PointF(point.X, point.Y);
                //get list of tris at given workingPoint in given frame
	            //var tris = tempPoTriDic[workingPoint];

                
				var distCanMove = shortestDistanceFromPoints(wPoint, framedPoints[frameNum], direction);
				var xComponent = getXComponent(direction, distCanMove);
				var yComponent = getYComponent(direction, distCanMove);

                wPoint.X += (float)xComponent;
                wPoint.Y += (float)yComponent;
			    framePoints.Add(wPoint);
			}
            //created so we dont have to modify framedPoints
            //var temp = new List<PointF>[framedPoints.Length];
            //framedPoints.CopyTo(temp,0);
            //temp[frameNum] = framePoints;
            //for (int i = 0; i < numFrames; i++)
            //{
            //    temp[i].AddRange(wideFramedPoints[i]);
            //}
            workingFrameList[frameNum] = framePoints;


            for (int i = 0; i < numFrames; i++)
		    {
                workingFrameList[i].AddRange(wideFramedPoints[i]);
		    }

	        return workingFrameList;
	    }

        //private Dictionary<System.Drawing.PointF, List<Triangle>> makeTrisFrame(int frameNum, int totalFrames)
        //{
        //    //temporary copy of the frame's points. This copy will serve as a 'frame' in the animationFrames array
        //    var tempPoList = new List<ceometric.DelaunayTriangulator.Point>(_points);
        //    //get array of points contained in a specified frame
        //    var pointList = framedPoints[frameNum];

        //    var direction = get360Direction();

        //    foreach (var workingPoint in pointList)
        //    {
        //        //get list of tris at given workingPoint in given frame
        //        //var tris = tempPoTriDic[workingPoint];

        //        var distCanMove = shortestDistanceFromTris(workingPoint, pointList, direction);
        //        var xComponent = getXComponent(direction, distCanMove);
        //        var yComponent = getYComponent(direction, distCanMove);
        //        foreach (var triangle in tris)
        //        {
        //            //animate each triangle
        //            //triangle.animate();
        //            if (triangle.Vertex1.X.CompareTo(workingPoint.X) == 0 && triangle.Vertex1.Y.CompareTo(workingPoint.Y) == 0)
        //            {
        //                triangle.Vertex1.X += xComponent;//frameLocation(frameNum, totalFrames, xComponent);
        //                triangle.Vertex1.Y += yComponent;//frameLocation(frameNum, totalFrames, yComponent);
        //            }
        //            else if (triangle.Vertex2.X.CompareTo(workingPoint.X) == 0 && triangle.Vertex2.Y.CompareTo(workingPoint.Y) == 0)
        //            {
        //                triangle.Vertex2.X += xComponent;//frameLocation(frameNum, totalFrames, xComponent);
        //                triangle.Vertex2.Y += yComponent;//frameLocation(frameNum, totalFrames, yComponent);
        //            }
        //            else if (triangle.Vertex3.X.CompareTo(workingPoint.X) == 0 && triangle.Vertex3.Y.CompareTo(workingPoint.Y) == 0)
        //            {
        //                triangle.Vertex3.X += xComponent;//frameLocation(frameNum, totalFrames, xComponent);
        //                triangle.Vertex3.Y += yComponent;//frameLocation(frameNum, totalFrames, yComponent);
        //            }
        //        }
        //    }
        //    return tempPoTriDic;
        //}

        private double frameLocation(int frame, int totalFrames, Double distanceToCcover)
		{
			var ratioToFinalMovement = frame / (Double)totalFrames;
			var thisCoord = ratioToFinalMovement * distanceToCcover;
			return thisCoord;
		}

		private double getXComponent(int angle, double length)
		{
			return length * Math.Cos(angle);
		}

		private double getYComponent(int angle, double length)
		{
			return length * Math.Sin(angle);
		}

		private int get360Direction()
		{
			//return a int from 0 to 359 that represents the direction a point will move
			return rand.Next(360);
		}

		private List<PointF> quadListFromPoints(List<PointF> points, int degree, PointF workingPoint)
		{
			var direction = "empty";

			if (degree > 270)
				direction = "quad4";
			else if (degree > 180)
				direction = "quad3";
			else if (degree > 90)
				direction = "quad2";
			else
				direction = "quad1";

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
				else if(point.X < workingPoint.X && point.Y < workingPoint.Y)
					quad3.Add(point);
			}
			switch (direction)
			{
				case "quad1":
					return quad1;
				case "quad2":
					return quad2;
				case "quad3":
					return quad3;
				case "quad4":
					return quad4;
				default:
					return quad1;
			}

		}

		private List<Triad> quadListFromTris(List<Triad> tris, int degree, PointF workingPoint, List<DelaunayTriangulator.Vertex> points)
		{
			var direction = "empty";

			if (degree > 270)
				direction = "quad4";
			else if (degree > 180)
				direction = "quad3";
			else if (degree > 90)
				direction = "quad2";
			else
				direction = "quad1";

			var quad1 = new List<Triad>();
			var quad2 = new List<Triad>();
			var quad3 = new List<Triad>();
			var quad4 = new List<Triad>();

			foreach (var tri in tris)
			{
				//var angle = getAngle(workingPoint, centroid(tri));
				var triCenter = centroid(tri, points);
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
				case "quad1":
					return quad1;
				case "quad2":
					return quad2;
				case "quad3":
					return quad3;
				case "quad4":
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
                    if (vertDistance < shortest && vertDistance.CompareTo(0)!=0)//if the vertDistance < current shortest distance and not equal to 0
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
                var vert1Distance = dist(workingPoint, _points[tri.a]);
                var vert2Distance = dist(workingPoint, _points[tri.a]);
                var vert3Distance = dist(workingPoint, _points[tri.a]);

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
                    if (tempShortest<shortest)
                        shortest = tempShortest;

            }
            return shortest;
        }

	    private double dist(PointF workingPoint, DelaunayTriangulator.Vertex vertex)
	    {
            var xSquare = (workingPoint.X + vertex.x) * (workingPoint.X + vertex.x);
            var ySquare = (workingPoint.Y + vertex.y) * (workingPoint.Y + vertex.y);
            return Math.Sqrt(xSquare + ySquare);
        }

        private double dist(PointF workingPoint, PointF vertex)
        {
            var xSquare = (workingPoint.X + vertex.X) * (workingPoint.X + vertex.X);
            var ySquare = (workingPoint.Y + vertex.Y) * (workingPoint.Y + vertex.Y);
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
            //for (int j = 0; j < overlays.Length; j++)
            //{
            //    //if the rectangle overlay contains a point
            //    if (overlays[j].Contains(point))
            //    {
            //        //if the point has not already been added to the overlay's point list
            //        if(!framedPoints[j].Contains(point))
            //            //add it
            //            framedPoints[j].Add(point);
            //    }
            //}
	    }

	    private RectangleF[] createVisibleOverlays()
	    {
            //get width of frame when there are numFrames rectangles on screen
            var frameWidth = boundsWidth / numFrames;
            //represents the left edge of the rectangles
            var currentX = 0;
			//array size numFrames of rectangles. each array entry serves as a rectangle(i) starting from the left
            RectangleF[] frames = new RectangleF[numFrames];

            //logic for grabbing points only in visible drawing area
            for (int i = 0; i < numFrames; i++)
            {
                RectangleF overlay = new RectangleF(currentX, 0, frameWidth, boundsHeight);

                frames[i] = overlay;
                currentX += frameWidth;
            }

            return frames;
	    }

        private RectangleF[] createWideOverlays()
        {
            //first and last rectangles need to be wider to cover points that are outside to the left and right of the pic bounds
            //all rectangles need to be higher and lower than the pic bounds to cover points above and below the pic bounds

            //get width of frame when there are numFrames rectangles on screen
            var frameWidth = boundsWidth / numFrames;
            //represents the left edge of the rectangles
            var currentX = 0;
            //array size numFrames of rectangles. each array entry serves as a rectangle(i) starting from the left
            RectangleF[] frames = new RectangleF[numFrames];

            //this logic is for grabbing all points (even those outside the visible drawing area)
            var tempWidth = boundsWidth / 2;
            var tempHeight = boundsHeight / 2;
            for (int i = 0; i < numFrames; i++)
            {
                System.Drawing.RectangleF overlay;
                //if the first rectangle
                if (i == 0)
                    overlay = new RectangleF(currentX - tempWidth, 0 - tempHeight, frameWidth + tempWidth, boundsHeight + (tempHeight * 2));
                //if the last rectangle
                else if (i == numFrames - 1)
                    overlay = new RectangleF(currentX, 0 - tempHeight, frameWidth + tempWidth, boundsHeight + (tempHeight * 2));
                else
                    overlay = new RectangleF(currentX, 0 - tempHeight, frameWidth, boundsHeight + (tempHeight * 2));

                frames[i] = overlay;
                currentX += frameWidth;
            }

            return frames;
        }

        private Path drawTrianglePath(System.Drawing.PointF a, System.Drawing.PointF b, System.Drawing.PointF c)
	    {
            Path path = new Path();
            path.SetFillType(Path.FillType.EvenOdd);
            path.MoveTo(b.X, b.Y);
            path.LineTo(c.X, c.Y);
            path.LineTo(a.X, a.Y);
            path.Close();
            return path;
        }

		private Android.Graphics.Color getTriangleColor(Bitmap gradient, System.Drawing.Point center)
		{
		    center = keepInPicBounds(center);

			System.Drawing.Color colorFromRGB;
			try
			{
				colorFromRGB = System.Drawing.Color.FromArgb(gradient.GetPixel(center.X, center.Y));
			}
			catch
			{
				colorFromRGB = System.Drawing.Color.Cyan;
			}

			Android.Graphics.Color triColor = Android.Graphics.Color.Rgb (colorFromRGB.R, colorFromRGB.G, colorFromRGB.B);
			return triColor;
		}

	    private System.Drawing.Point keepInPicBounds(System.Drawing.Point center)
	    {
            if (center.X < 0)
                center.X += (int)bleed_x;
            else if (center.X > boundsWidth)
                center.X -= (int)bleed_x;
            else if (center.X == boundsWidth)
                center.X -= (int)bleed_x - 1;
            if (center.Y < 0)
                center.Y += (int)bleed_y;
            else if (center.Y > boundsHeight)
                center.Y -= (int)bleed_y + 1;
            else if (center.Y == boundsHeight)
                center.Y -= (int)bleed_y - 1;
	        return center;
	    }

		private	int[] getGradientColors()
		{
			//get all gradient codes
			var values = Enum.GetValues(typeof(ColorBru.Code));
			ColorBru.Code randomCode = (ColorBru.Code)values.GetValue(rand.Next(values.Length));
			//gets specified colors in gradient length: #
			var brewColors = ColorBru.GetHtmlCodes (randomCode, 6);
			//array of ints converted from brewColors
			var colorArray = new int[brewColors.Length];
			for (int i = 0; i < brewColors.Length; i++) {
				colorArray [i] = Android.Graphics.Color.ParseColor (brewColors [i]);
			}
			return colorArray;
		}

		private Bitmap getGradient()
		{
			var colorArray = getGradientColors ();

			Shader gradientShader;

			switch (rand.Next(3)) {
			case 0:
				gradientShader = new LinearGradient (
					                      0,
					                      0,
					                      boundsWidth,
					                      boundsHeight,
					                      colorArray,
					                      null,
					                      Shader.TileMode.Repeat
				                      );
				break;
			case 1:
				gradientShader = new SweepGradient (
					((float)boundsWidth / 2),
					((float)boundsHeight / 2),
					colorArray,
					//new float[]{ }
					null
				);
				break;
			case 2:
				gradientShader = new RadialGradient (
					                        ((float)boundsWidth / 2),
					                        ((float)boundsHeight / 2),
					                        ((float)boundsWidth / 2),
					                        colorArray,
					                        null,
					                        Shader.TileMode.Clamp
				                        );
				break;
			default:
				gradientShader = new LinearGradient (
					0,
					0,
					boundsWidth,
					boundsHeight,
					colorArray,
					null,
					Shader.TileMode.Repeat
				);
				break;
			}

//			LinearGradientBrush brush = new LinearGradientBrush(
//				new System.Drawing.Point(0,0),
//				new System.Drawing.Point(width, height), 
//				Color.FromArgb(255,0,0,255),
//				Color.FromArgb(255,0,255,0));

//			Bitmap temp = new Bitmap(width, height);
			Bitmap bmp = Bitmap.CreateBitmap (boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
//			Graphics graphics = Graphics.FromImage(temp);
			Canvas canvas = new Canvas (bmp);
			Paint pnt = new Paint();
			pnt.SetStyle (Paint.Style.Fill);
			pnt.SetShader (gradientShader);
			canvas.DrawRect(0,0,boundsWidth,boundsHeight,pnt);
			return bmp;
		}

		public List<DelaunayTriangulator.Vertex> GeneratePoints()
		{
			var points = new List<DelaunayTriangulator.Vertex>();
			for (var i = - bleed_x; i < boundsWidth + bleed_x; i += cell_size) 
			{
				for (var j = - bleed_y; j < boundsHeight + bleed_y; j += cell_size) 
				{
					var x = i + cell_size/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					var y = j + cell_size/2 + _map(rand.NextDouble(),new int[] {0, 1},new double[] {-calcVariance, calcVariance});
					points.Add(new DelaunayTriangulator.Vertex((float)Math.Floor(x),(float)Math.Floor(y)));
				}
			}
			return points;
		}

		private double _map(double num, int[] in_range, double[] out_range)
		{
			return (num - in_range[0]) * (out_range[1] - out_range[0]) / (in_range[1] - in_range[0]) + out_range[0];
		}

		private System.Drawing.Point centroid(Triad triangle, List<DelaunayTriangulator.Vertex> points)
		{
			var x = (int)((points[triangle.a].x + points[triangle.b].x + points[triangle.c].x) / 3);
			var y = (int)((points[triangle.a].y + points[triangle.b].y + points[triangle.c].y) / 3);

			return new System.Drawing.Point(x,y);
		}
	}
}

