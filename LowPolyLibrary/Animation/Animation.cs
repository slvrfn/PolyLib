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
		private readonly TransformBlock<CustomAnimtion[], CustomAnimtion> _renderFrame;
		private readonly TransformBlock<CustomAnimtion, int> _drawFrame;
		private readonly FrameQueueBlock<int> _frameQueue;
		private readonly RandomAnimationBlock _randomAnim;
		private readonly ActionBlock<int> _writeImage;

		public Animation()
		{
			_animations = new CurrentAnimationsBlock();
			_randomAnim = new RandomAnimationBlock(_animations, 5000);
			_frameQueue = new FrameQueueBlock<int>();

			_renderFrame = new TransformBlock<CustomAnimtion[], CustomAnimtion>((arg) => 
			{
				return new CustomAnimtion(AnimationTypes.Type.Grow, -1);
			});

			_drawFrame = new TransformBlock<CustomAnimtion, int>((arg) => 
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

		public void AddEvent(string animName, int totalFrames, int duration)
		{
			var anim = new CustomAnimtion(animName, totalFrames, duration);

			_animations.Post(anim);
		}
	}
}
