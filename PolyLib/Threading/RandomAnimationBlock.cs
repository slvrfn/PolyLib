using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PolyLib.Animation;

namespace PolyLib.Threading
{
    // Propagates data in a sliding window fashion.
    public class RandomAnimationBlock : ISourceBlock<AnimationBase>
    {
        // The source part of the block.
        private readonly IReceivableSourceBlock<AnimationBase> _msource;

        private readonly BroadcastBlock<AnimationBase> _source;

        private List<Triangulation> _tris;

        //functions to be provided by user that specify how an animation should be created
        //ex: a function which randomly assigns touch locations couild be created
        private List<Func<Triangulation, AnimationBase>> _animCreators;

        private Timer _tim;

        #region Constructors
        // Constructs a SlidingWindowBlock object.
        public RandomAnimationBlock(int MSdelayUntilAnimAdded)
        {
            _source = new BroadcastBlock<AnimationBase>(f => f);

            _msource = _source;

            //delay until random animation is added
            _tim = new Timer(MSdelayUntilAnimAdded, AddRandomAnimation);
            _tim.Start();

            _animCreators = new List<Func<Triangulation, AnimationBase>>();
            _tris = new List<Triangulation>();
        }
        #endregion

        public void SetAnimationCreators(List<Func<Triangulation, AnimationBase>> animCreators)
        {
            _animCreators = animCreators;
        }

        private async Task<bool> AddRandomAnimation(object sender)
        {
            if (_animCreators.Count > 0 && _tris.Count>0)
            {
                var index = Random.Rand.Next(_animCreators.Count);
                var indexforTriangulation = Random.Rand.Next(_tris.Count);

                var randomAnim = _animCreators[index](_tris[indexforTriangulation]);
                return await _source.SendAsync(randomAnim);

            }
            return false;
        }

        public void UpdateSourceTriangulations(List<Triangulation> tris)
        {
            if (_tris.Equals(tris))
            {
                return;
            }

            _tris = tris;
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
