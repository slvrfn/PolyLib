using System;
using System.Collections.Generic;
namespace LowPolyLibrary.Animation
{
    //created bc there has been an issue with frames being skipped (generating frames faster than they can be drawn)
    public class FrameDrawManager
    {
        private Queue<RenderedFrame> frameQueue; 

        public FrameDrawManager()
        {
            frameQueue = new Queue<RenderedFrame>();
        }

        public void MarkFrameDrawn(int frameIdentifier){
            var frame = frameQueue.Peek();

            //this should always be true when this function runs
            if (frame.FrameIdentifier == frameIdentifier)
            {
                frameQueue.Dequeue();
                Console.WriteLine($"Frame drawn {frame.currFrame}/{frame.totalFrame}");
            }
            else{
                throw new Exception("Expected frame drawn to be the latest planned frame");
            }
        }

        public Func<RenderedFrame, bool> BuildDrawFrameAction(Action<RenderedFrame> action){
            return new Func<RenderedFrame, bool>((frameToConsider) => {
                //nulls allowed to allow the re-signal that a frame still needs to be drawn
                if(frameToConsider != null){
                    //should always be anle to enqueue new frames, they should never be lost
                    frameQueue.Enqueue(frameToConsider);
                }

                //grabs the frame that is planned to be drawn
                //this frame is removed when it has actually been drawn
                action(frameQueue.Peek());

                //if true, another redraw signal will be fired
                return frameQueue.Count > 0;
            });
        }
    }
}
