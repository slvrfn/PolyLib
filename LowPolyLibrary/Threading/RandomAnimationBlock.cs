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

			tim.Start();
		}
		#endregion

		public void AddRandomAnimation(object sender)
		{
			var rand = new System.Random();
			var values = Enum.GetValues(typeof(AnimationTypes.Type));
			ColorBru.Code randomAnimType = (ColorBru.Code)values.GetValue(rand.Next(values.Length));

			var newAnim = new AnimationBase("custom", 6, 200);
			_source.Post(newAnim);
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
