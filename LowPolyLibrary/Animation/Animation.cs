using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using LowPolyLibrary.Threading;
using LowPolyLibrary.Animation;

namespace LowPolyLibrary.Animation
{
	public class Animation
	{
		private readonly CurrentAnimationsBlock _animations; 
		private readonly TransformBlock<AnimationBase[], AnimationBase> _renderFrame;
		private readonly TransformBlock<AnimationBase, Android.Graphics.Bitmap> _drawFrame;
		private readonly FrameQueueBlock<Android.Graphics.Bitmap> _frameQueue;
		//private readonly RandomAnimationBlock _randomAnim;
		private readonly ActionBlock<Android.Graphics.Bitmap> _writeImage;

        public Animation(Action<Android.Graphics.Bitmap> writeImage)
        {
            _animations = new CurrentAnimationsBlock();
            //_randomAnim = new RandomAnimationBlock(_animations, 5000);
            _frameQueue = new FrameQueueBlock<Android.Graphics.Bitmap>(new ExecutionDataflowBlockOptions { BoundedCapacity = 5, MaxDegreeOfParallelism = Environment.ProcessorCount });

            _renderFrame = new TransformBlock<AnimationBase[], AnimationBase>((arg) =>
            {
                var animFrame = new List<List<AnimatedPoint>>();
                foreach (var anim in arg)
                {
                    var x = anim.RenderFrame();
                    animFrame.Add(x);
                }

                //no use in "combining" animations unless there is more than 1 anim for this frame
                if (animFrame.Count > 1)
                {
                    var dict = new Dictionary<System.Drawing.PointF, AnimatedPoint>();
                    //for each animation render for this frame
                    foreach (var frame in animFrame)
                    {
                        //for each point changed in the rendered animation
                        foreach (var pointChange in frame)
                        {
                            //if point has been previously animated, update it
                            if (dict.ContainsKey(pointChange.Point))
                            {
                                dict[pointChange.Point].XDisplacement += pointChange.XDisplacement;
                                dict[pointChange.Point].YDisplacement += pointChange.YDisplacement;
                            }
                            //or add it
                            else
                            {
                                dict[pointChange.Point] = pointChange;
                            }
                        }
                    }
                    arg[0].AnimatedPoints = dict.Values.ToList();
                }
                else
                {
                    arg[0].AnimatedPoints = animFrame[0];
                }

                _animations.FrameRendered();
                AnimationBase a;
                switch (arg[0].AnimationType)
                {
                    case AnimationTypes.Type.Sweep:
                        a = new Sweep(arg[0].triangulation, arg[0].AnimatedPoints.ToList());
                        break;
                    case AnimationTypes.Type.Touch:
                        a = new Touch(arg[0].triangulation, arg[0].AnimatedPoints.ToList());
                        break;
                    case AnimationTypes.Type.Grow:
                        a = new Grow(arg[0].triangulation, arg[0].AnimatedPoints.ToList());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return a;
            }, new ExecutionDataflowBlockOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount});

            _drawFrame = new TransformBlock<AnimationBase, Android.Graphics.Bitmap>((arg) =>
            {
                Android.Graphics.Bitmap bitmap = null;
                try
                {
                    bitmap = arg.DrawPointFrame(arg.AnimatedPoints);
                    return bitmap;
                }
                catch (Exception ex)
                {
                    return bitmap;
                }

            }, new ExecutionDataflowBlockOptions { BoundedCapacity = 5, MaxDegreeOfParallelism = Environment.ProcessorCount });

            _writeImage = new ActionBlock<Android.Graphics.Bitmap>(writeImage, new ExecutionDataflowBlockOptions { TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext() });

			_animations.LinkTo(_renderFrame);
			//_randomAnim.LinkTo(_animations, new DataflowLinkOptions());
			_renderFrame.LinkTo(_drawFrame);
			_drawFrame.LinkTo(_frameQueue);
			_frameQueue.LinkTo(_writeImage);

			//GenerateImage();
		}

		public void AddEvent(Triangulation tri, AnimationTypes.Type animName, int totalFrames, float x = 0f, float y = 0f, int radius = 0)
		{
			AnimationBase temp = null;
			switch (animName)
			{
				case AnimationTypes.Type.Grow:
					temp = new Grow(tri);
					break;
				case AnimationTypes.Type.Touch:
			        temp = new Touch(tri, x, y, radius);
					break;
				case AnimationTypes.Type.Sweep:
					temp = new Sweep(tri);
					break;
			}

			_animations.Post(temp);
		}
	}
}
