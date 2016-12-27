using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace LowPolyLibrary.Threading
{
	public class AnimationRendering
	{
		private readonly CurrentAnimationsBlock _animations;
		private readonly TransformBlock<CustomAnimtion[], int> _renderFrame;
		private readonly FrameQueueBlock<int> _frameQueue;
		private readonly RandomAnimationBlock _randomAnim;
		private readonly ActionBlock<int> _writeImage;

		private Stopwatch watch;

		public AnimationRendering()
		{
			_animations = new CurrentAnimationsBlock();

			_frameQueue = new FrameQueueBlock<int>();

			_randomAnim = new RandomAnimationBlock(_animations, 5000);

			_renderFrame = new TransformBlock<CustomAnimtion[], int>(animations =>
			{
				//Console.WriteLine("---------");
				var frameChange = 0;
				for (int i = 0; i < animations.Length; i++)
				{
					//Thread.Sleep(animations[i].FrameDuration);
					var thisChange = animations[i].frames[animations[i].CurrentFrame];
					frameChange += thisChange;
					//Console.WriteLine("Animation: " + animations[i].Type);
					//Console.WriteLine("Frame Changes: {0}", animations[i].frames.ToString());
					//Console.WriteLine("Current frame: " + (animations[i].CurrentFrame + 1) + "/" + animations[i].TotalFrames);
					//Console.WriteLine("This Frame change: {0}", thisChange);
				}
				_animations.FrameRendered();
				return frameChange;
			});

			_writeImage = new ActionBlock<int>(frame =>
			{
				watch.Stop();
				//Console.WriteLine("Time since last frame display: {0}", watch.Elapsed);
				//Console.ForegroundColor = ConsoleColor.DarkYellow;
				//Console.WriteLine("Current Frame: {0}", frame);
				//Console.ResetColor();
				watch.Reset();
				watch.Start();
			});

			_animations.LinkTo(_renderFrame);
			_renderFrame.LinkTo(_frameQueue);
			_frameQueue.LinkTo(_writeImage);
			_randomAnim.LinkTo(_animations, new DataflowLinkOptions());

			GenerateImage();

			watch = new Stopwatch();
			watch.Start();
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
