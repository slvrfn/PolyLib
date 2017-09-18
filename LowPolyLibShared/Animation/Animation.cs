﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using LowPolyLibrary.Threading;
using LowPolyLibrary.Animation;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
	public class Animation
	{
		private readonly CurrentAnimationsBlock _animations; 
		private readonly TransformBlock<AnimationBase[], RenderedFrame> _renderFrame;
		private readonly FrameQueueBlock<RenderedFrame> _frameQueue;
		private readonly RandomAnimationBlock _randomAnim;
		private readonly ActionBlock<RenderedFrame> _signalFrameRendered;

	    private CustomCanvasView _currentDisplay;

	    private RenderedFrame currentRenderedFrame;

	    public bool HasFrameToDraw
        {
	        get { return currentRenderedFrame != null; }
	    }

        public Animation(CustomCanvasView currentDisplay)
        {
            _currentDisplay = currentDisplay;
            _animations = new CurrentAnimationsBlock();
            _randomAnim = new RandomAnimationBlock(_animations, 5000);
            _frameQueue = new FrameQueueBlock<RenderedFrame>(new ExecutionDataflowBlockOptions { BoundedCapacity = 5, MaxDegreeOfParallelism = Environment.ProcessorCount });

            _renderFrame = new TransformBlock<AnimationBase[], RenderedFrame>((arg) =>
            {

                var animFrame = new List<List<AnimatedPoint>>();
                foreach (var anim in arg)
                {
                        if (!anim.IsSetup)
                        {
                            anim.SetupAnimation();
                        }

                    var x = anim.RenderFrame();
                    animFrame.Add(x);
                }
                
                //storage to be more generic in future, allows any derived draw function
                var rend = new RenderedFrame(arg[0].DrawPointFrame);
            
                    //no use in "combining" animations unless there is more than 1 anim for this frame
                if (animFrame.Count > 1)
                {
                    var dict = new Dictionary<SKPoint, AnimatedPoint>();
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
                        rend.FramePoints = dict.Values.ToList();
                }
                else
                {
                    rend.FramePoints = animFrame[0].ToList();
                }

                _animations.FrameRendered();

                return rend;
            }, new ExecutionDataflowBlockOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount});

            _signalFrameRendered = new ActionBlock<RenderedFrame>((arg) =>
            {
                currentRenderedFrame = arg;
                _currentDisplay.Invalidate();
            }, new ExecutionDataflowBlockOptions { TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext() });

			_animations.LinkTo(_renderFrame);
			//_randomAnim.LinkTo(_animations, new DataflowLinkOptions());

            _renderFrame.LinkTo(_frameQueue);
			_frameQueue.LinkTo(_signalFrameRendered);
		}

		public void AddEvent(Triangulation tri, AnimationTypes.Type animName, float x = 0f, float y = 0f, int radius = 0)
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

	    public void DrawOnMe(SKSurface surface)
	    {
	        if (currentRenderedFrame!= null)
	        {
	            currentRenderedFrame.DrawFunction(surface, currentRenderedFrame.FramePoints);
	            currentRenderedFrame = null;
	        }
	    }

        public void UpdateTriangulationForRandom(Triangulation tri)
        {
            _randomAnim.UpdateTriangulation(tri);
        }
	}
}
