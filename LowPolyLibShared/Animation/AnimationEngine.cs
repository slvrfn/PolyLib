using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    class AnimationEngine
    {
        private CustomCanvasView _currentDisplay;
        private RenderedFrame currentRenderedFrame;

        //for now keep look alive as long as the animation engine exists
        private AnimationFlow animationFlow;

        //used to run final action of notifying canvas of a frame being availbe for drawing
        readonly TaskScheduler uiTaskScheduler;

        private bool KeepLoopAlive = true;

        //used to track if a random animation loop should be added to the animaiton flow when it is started or recreated
        private bool ShouldStartRandomAnim = false;
        private int RandomAnimationTime = 5000;

        public bool HasFrameToDraw
        {
            get { return currentRenderedFrame != null; }
        }

        public AnimationEngine(CustomCanvasView display)
        {
            //start the thread that will keep the animation flow alive
            Task.Run(RestartActionBlock);
            
            uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            _currentDisplay = display;
        }

        ~AnimationEngine()
        {
            KeepLoopAlive = false;
            _currentDisplay = null;
        }

        private async Task RestartActionBlock()
        {
            while (KeepLoopAlive)
            {
                animationFlow = new AnimationFlow((arg) =>
                {
                    currentRenderedFrame = arg;
                    _currentDisplay.Invalidate();
                }, uiTaskScheduler);
                
                if (ShouldStartRandomAnim)
                {
                    animationFlow.StartRandomAnimationsLoop(RandomAnimationTime);
                }

                var completionTask = await Task.WhenAny(animationFlow.CompletionTask);

                var message = "Animation Loop Has Closed: it ";
                switch (completionTask.Status)
                {
                    case TaskStatus.RanToCompletion:
                        message += "ran to completion";
                        break;
                    case TaskStatus.Canceled:
                        message += "was canceled";
                        break;
                    case TaskStatus.Faulted:
                        message += "has faulted";
                        Debug.WriteLine(completionTask.Exception);
                        break;
                }
                Debug.WriteLine("\n" + message);
            }
        }

        public void AddAnimation(AnimationBase anim)
        {
            animationFlow.InputBlock.Post(anim);
        }

        public void StartRandomAnimationsLoop(int msBetweenRandomAnim)
        {
            RandomAnimationTime = msBetweenRandomAnim;
            ShouldStartRandomAnim = true;

            animationFlow.StartRandomAnimationsLoop(RandomAnimationTime);
        }

        public void StopRandomAnimationsLoop()
        {
            ShouldStartRandomAnim = false;
            animationFlow.StopRandomAnimationsLoop();
        }

        public void DrawOnMe(SKSurface surface)
        {
            if (currentRenderedFrame != null)
            {
                if (currentRenderedFrame.PreviousFramePoints != null)
                {
                    currentRenderedFrame.DrawFunction(surface, currentRenderedFrame.PreviousFramePoints, true);
                }
                
                currentRenderedFrame.DrawFunction(surface, currentRenderedFrame.CurrentFramePoints, false);
                currentRenderedFrame = null;
            }
        }

        //public void UpdateTriangulationForRandom(Triangulation tri)
        //{
        //    _randomAnim.UpdateTriangulation(tri);
        //}
    }
}
