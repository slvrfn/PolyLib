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
		private readonly TransformBlock<AnimationBase, int> _drawFrame;
		private readonly FrameQueueBlock<int> _frameQueue;
		private readonly RandomAnimationBlock _randomAnim;
		private readonly ActionBlock<int> _writeImage;

		public Animation()
		{
			_animations = new CurrentAnimationsBlock();
			_randomAnim = new RandomAnimationBlock(_animations, 5000);
			_frameQueue = new FrameQueueBlock<int>();

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



				return arg[0];
			});

			_drawFrame = new TransformBlock<AnimationBase, int>((arg) => 
			{
				return -1;
			});

			_writeImage = new ActionBlock<int>((int arg) =>
			{

			});

			_animations.LinkTo(_renderFrame);
			_randomAnim.LinkTo(_animations, new DataflowLinkOptions());
			_renderFrame.LinkTo(_drawFrame);
			_drawFrame.LinkTo(_frameQueue);
			_frameQueue.LinkTo(_writeImage);

			//GenerateImage();
		}

		private void GenerateImage()
		{
			_writeImage.Post(0);
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
