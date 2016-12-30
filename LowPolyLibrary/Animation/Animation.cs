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

		public Animation(ActionBlock<Android.Graphics.Bitmap> writeImage)
		{
			_animations = new CurrentAnimationsBlock();
			//_randomAnim = new RandomAnimationBlock(_animations, 5000);
			_frameQueue = new FrameQueueBlock<Android.Graphics.Bitmap>();

			_renderFrame = new TransformBlock<AnimationBase[], AnimationBase>((arg) => 
			{
				var temp = new List<List<AnimatedPoint>>();
				foreach (var a in arg)
				{
					if (a.AnimationType == AnimationTypes.Type.Touch)
					{
						var t = (Touch)a;
						var x = t.RenderFrame();
						temp.Add(x);
					}
					else if (a.AnimationType == AnimationTypes.Type.Sweep)
					{
						var t = (Sweep)a;
						var x = t.RenderFrame();
						temp.Add(x);
					}
					else if (a.AnimationType == AnimationTypes.Type.Grow)
					{
						var t = (Grow)a;
						var x = t.RenderFrame();
						temp.Add(x);
					}
				}
				var dict = new Dictionary<System.Drawing.PointF, AnimatedPoint>();
				//for each animation render for this frame
				foreach (var frame in temp)
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

				_animations.FrameRendered();

				//animation to be used for rendering
				//can be any derived class of animationbase
				//chosen anim affects drawing of the frame
				//if grow animation is included for the frame, the frame needs to be drawn a special way
				if (arg.Any((t) => t.AnimationType == AnimationTypes.Type.Grow))
				{
					return arg[0] as Grow;
				}
				return arg[0] as Sweep;
			});

			_drawFrame = new TransformBlock<AnimationBase, Android.Graphics.Bitmap>((arg) => 
			{
				Android.Graphics.Bitmap bitmap = null;

				var growAnim = arg as Grow;
				if (growAnim != null)
				{
					bitmap = growAnim.DrawPointFrame(growAnim.AnimatedPoints);
				}
				else
				{
					bitmap = arg.DrawPointFrame(arg.AnimatedPoints);
				}
				return bitmap;
			});

			_writeImage = writeImage;

			_animations.LinkTo(_renderFrame);
			//_randomAnim.LinkTo(_animations, new DataflowLinkOptions());
			_renderFrame.LinkTo(_drawFrame);
			_drawFrame.LinkTo(_frameQueue);
			_frameQueue.LinkTo(_writeImage);

			//GenerateImage();
		}

		public void AddEvent(Triangulation tri, AnimationTypes.Type animName, int totalFrames)
		{
			AnimationBase temp = null;
			switch (animName)
			{
				case AnimationTypes.Type.Grow:
					temp = new Grow(tri);
					break;
				case AnimationTypes.Type.Touch:
					temp = new Touch(tri, 0, 0, 0);
					break;
				case AnimationTypes.Type.Sweep:
					temp = new Sweep(tri);
					break;
			}

			_animations.Post(temp);
		}
	}
}
