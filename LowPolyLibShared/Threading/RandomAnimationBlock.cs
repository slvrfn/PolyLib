using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using LowPolyLibrary.Animation;

namespace LowPolyLibrary.Threading
{
	// Propagates data in a sliding window fashion.
	public class RandomAnimationBlock : ISourceBlock<AnimationBase>
	{
		// The source part of the block.
		private readonly IReceivableSourceBlock<AnimationBase> _msource;

		private readonly BroadcastBlock<AnimationBase> _source;

        private Triangulation tri;

        //TODO Make user specify what animations could randomly show up, and how long they occur for
	    private int numFrames = 12;

		Timer tim;

        #region Constructors
        // Constructs a SlidingWindowBlock object.
        public RandomAnimationBlock(CurrentAnimationsBlock animBlock, int MSdelayUntilAnimAdded)
		{
			_source = new BroadcastBlock<AnimationBase>(f => f);

            _msource = _source;

			//LinkTo(animBlock, new DataflowLinkOptions());


			animBlock.AnimationAdded += AnimBlock_AnimationAdded;
			animBlock.NoPendingAnimations += AnimBlock_NoPendingAnimations;

			//delay until random animation is added
			tim = new Timer(MSdelayUntilAnimAdded, AddRandomAnimation, true);
		}
		#endregion

		public async Task<bool> AddRandomAnimation(object sender)
		{
            var values = Enum.GetValues(typeof(AnimationTypes.Type));
            var t = values.GetValue(Random.Rand.Next(values.Length));
            ColorBru.Code randomAnimType = (ColorBru.Code)t;
            var conv = (AnimationTypes.Type)t;

            //var newAnim = new AnimationBase("custom", 6, 200);
            AnimationBase newAnim;

            switch (conv)
            {
                case AnimationTypes.Type.Sweep:
                    newAnim = new Sweep(tri, numFrames);
                    break;
                case AnimationTypes.Type.RandomTouch:
                    var x = Random.Rand.Next(0, tri.BoundsWidth);
                    var y = Random.Rand.Next(0, tri.BoundsHeight);
                    newAnim = new RandomTouch(tri, numFrames, x, y, 200);
                    break;
                case AnimationTypes.Type.PushTouch:
                    var xx = Random.Rand.Next(0, tri.BoundsWidth);
                    var yy = Random.Rand.Next(0, tri.BoundsHeight);
                    newAnim = new PushTouch(tri, numFrames, xx, yy, 200);
                    break;
                case AnimationTypes.Type.Grow:
                    newAnim = new Grow(tri, numFrames);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            tim.Start();
			return await _source.SendAsync(newAnim);
		}

        public void UpdateTriangulation(Triangulation _tri)
        {
            tri = _tri;
            tim.Start();
        }

		void AnimBlock_AnimationAdded(object sender, EventArgs e)
		{
			tim.Stop();
		}

		void AnimBlock_NoPendingAnimations(object sender, EventArgs e)
		{
			tim.Start();
		}

		#region ISourceBlock<TOutput> members

		// Links this dataflow block to the provided target.
		public IDisposable LinkTo(ITargetBlock<AnimationBase> target, DataflowLinkOptions linkOptions)
		{
			return _msource.LinkTo(target, linkOptions);
		}

		// Called by a target to reserve a message previously offered by a source 
		// but not yet consumed by this target.
		bool ISourceBlock<AnimationBase>.ReserveMessage(DataflowMessageHeader messageHeader,
		   ITargetBlock<AnimationBase> target)
		{
			return _msource.ReserveMessage(messageHeader, target);
		}

		// Called by a target to consume a previously offered message from a source.
		AnimationBase ISourceBlock<AnimationBase>.ConsumeMessage(DataflowMessageHeader messageHeader,
		   ITargetBlock<AnimationBase> target, out bool messageConsumed)
		{
			return _msource.ConsumeMessage(messageHeader,
			   target, out messageConsumed);
		}

		// Called by a target to release a previously reserved message from a source.
		void ISourceBlock<AnimationBase>.ReleaseReservation(DataflowMessageHeader messageHeader,
		   ITargetBlock<AnimationBase> target)
		{
			_msource.ReleaseReservation(messageHeader, target);
		}

		#endregion

		#region IDataflowBlock members

		// Gets a Task that represents the completion of this dataflow block.
		public Task Completion { get { return _msource.Completion; } }

		// Signals to this target block that it should not accept any more messages, 
		// nor consume postponed messages. 
		public void Complete()
		{
			_msource.Complete();
		}

		public void Fault(Exception error)
		{
			_msource.Fault(error);
		}

		#endregion
	}
}
