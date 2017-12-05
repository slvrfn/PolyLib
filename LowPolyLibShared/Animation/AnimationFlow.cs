﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
  using Gridsum.DataflowEx;
  using Java.Lang;
  using LowPolyLibrary.Threading;
using LowPolyLibrary.Animation;
using SkiaSharp;
  using Exception = System.Exception;

namespace LowPolyLibrary.Animation
{
	public class AnimationFlow : Dataflow<AnimationBase>
	{
		private readonly CurrentAnimationsBlock _animations; 
		private readonly TransformBlock<AnimationBase[], RenderedFrame> _renderFrame;
		private readonly FrameQueueBlock<AnimationBase[]> _frameQueue;
		private RandomAnimationBlock _randomAnim;
		private readonly ActionBlock<RenderedFrame> _signalFrameRendered;

        public AnimationFlow(Action<RenderedFrame> notifyFrameReady, TaskScheduler uiScheduler) : base(DataflowOptions.Default)
        {
            
            _animations = new CurrentAnimationsBlock();
            _frameQueue = new FrameQueueBlock<AnimationBase[]>(new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });

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
                
                //allows any derived draw function
                //oldest animation determines how all current animations will be drawn
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

            //limit parallelism so that only one redraw update can occur
            _signalFrameRendered = new ActionBlock<RenderedFrame>(notifyFrameReady, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 1, TaskScheduler = uiScheduler });

            _animations.LinkTo(_frameQueue);
            _frameQueue.LinkTo(_renderFrame);
            _renderFrame.LinkTo(_signalFrameRendered);

            RegisterChild(_animations);
            RegisterChild(_frameQueue);
            RegisterChild(_renderFrame);
            RegisterChild(_signalFrameRendered);
		}

	    public void StartRandomAnimationsLoop(int msBetweenRandomAnim)
	    {
	        _randomAnim = new RandomAnimationBlock(_animations, msBetweenRandomAnim);
	        _randomAnim.LinkTo(_animations, new DataflowLinkOptions());
	        RegisterChild(_randomAnim);
        }

	    public void StopRandomAnimationsLoop()
	    {
            //force completion of the flow, which will make the flow get recreated
	        //engine determines if a new animation should be added
            _animations.Complete();
	    }

	    public override ITargetBlock<AnimationBase> InputBlock
	    {
	        get { return _animations; }
	    }
	}
}
