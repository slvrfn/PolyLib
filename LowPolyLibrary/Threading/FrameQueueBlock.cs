using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LowPolyLibrary.Threading
{
    // Propagates data in a sliding window fashion.
    public class FrameQueueBlock<T> : IPropagatorBlock<T, T>, IReceivableSourceBlock<T>
    {
        // The target part of the block.
        private readonly ITargetBlock<T> _mtarget;
        // The source part of the block.
        private readonly IReceivableSourceBlock<T> _msource;

        private readonly BroadcastBlock<T> _source;

        private readonly BufferBlock<T> _target;

        private Timer _tim;

        #region Constructors
        // Constructs a SlidingWindowBlock object.
        public FrameQueueBlock() : this(new DataflowBlockOptions(), new ExecutionDataflowBlockOptions()) { }

        public FrameQueueBlock(DataflowBlockOptions broadcastBlockOptions) : this(broadcastBlockOptions, new ExecutionDataflowBlockOptions()) { }

        public FrameQueueBlock(ExecutionDataflowBlockOptions bufferBlockOptions) : this(new DataflowBlockOptions(), bufferBlockOptions) { }

        public FrameQueueBlock(DataflowBlockOptions broadcastBlockOptions, ExecutionDataflowBlockOptions bufferBlockOptions)
        {
            // The source part of the propagator holds arrays of size windowSize
            // and propagates data out to any connected targets.
            _source = new BroadcastBlock<T>(f => f, broadcastBlockOptions);

            // The target part receives data and adds them to the queue.
            _target = new BufferBlock<T>(bufferBlockOptions);

            //	(item =>
            //{
            //	// Add the item to the queue.
            //	frameQueue.Enqueue(item);
            //}, actionBlockOptions);

            // When the target is set to the completed state, propagate out any
            // remaining data and set the source to the completed state.
            _target.Completion.ContinueWith(delegate
            {
                _source.Complete();
            });

            _mtarget = _target;
            _msource = _source;

            //estimated fps
            _tim = new Timer(42, DisplayFrame, false);
            _tim.Start();
        }
        #endregion

        public async Task<bool> DisplayFrame(object sender)
        {
            var t = await _target.ReceiveAsync();
            return _source.Post(t);
        }

        #region IReceivableSourceBlock<TOutput> members

        // Attempts to synchronously receive an item from the source.
        public bool TryReceive(Predicate<T> filter, out T item)
        {
            return _msource.TryReceive(filter, out item);
        }

        // Attempts to remove all available elements from the source into a new 
        // array that is returned.
        public bool TryReceiveAll(out IList<T> items)
        {
            return _msource.TryReceiveAll(out items);
        }

        #endregion

        #region ISourceBlock<TOutput> members

        // Links this dataflow block to the provided target.
        public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        {
            return _msource.LinkTo(target, linkOptions);
        }

        // Called by a target to reserve a message previously offered by a source 
        // but not yet consumed by this target.
        bool ISourceBlock<T>.ReserveMessage(DataflowMessageHeader messageHeader,
           ITargetBlock<T> target)
        {
            return _msource.ReserveMessage(messageHeader, target);
        }

        // Called by a target to consume a previously offered message from a source.
        T ISourceBlock<T>.ConsumeMessage(DataflowMessageHeader messageHeader,
           ITargetBlock<T> target, out bool messageConsumed)
        {
            return _msource.ConsumeMessage(messageHeader,
               target, out messageConsumed);
        }

        // Called by a target to release a previously reserved message from a source.
        void ISourceBlock<T>.ReleaseReservation(DataflowMessageHeader messageHeader,
           ITargetBlock<T> target)
        {
            _msource.ReleaseReservation(messageHeader, target);
        }

        #endregion

        #region ITargetBlock<TInput> members

        // Asynchronously passes a message to the target block, giving the target the 
        // opportunity to consume the message.
        DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader,
           T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {
            return _mtarget.OfferMessage(messageHeader,
               messageValue, source, consumeToAccept);
        }

        #endregion

        #region IDataflowBlock members

        // Gets a Task that represents the completion of this dataflow block.
        public Task Completion { get { return _msource.Completion; } }

        // Signals to this target block that it should not accept any more messages, 
        // nor consume postponed messages. 
        public void Complete()
        {
            _mtarget.Complete();
        }

        public void Fault(Exception error)
        {
            _mtarget.Fault(error);
        }

        #endregion
    }
}
