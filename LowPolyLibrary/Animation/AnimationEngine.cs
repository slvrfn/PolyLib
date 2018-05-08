using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SkiaSharp;

namespace LowPolyLibrary.Animation
{
    public class AnimationEngine
    {
        private IAnimationUpdateView _currentDisplay;
        private RenderedFrame _currentRenderedFrame;

        //for now keep look alive as long as the animation engine exists
        private AnimationFlow _animationFlow;

        //used to run final action of notifying canvas of a frame being availbe for drawing
        private readonly TaskScheduler _uiTaskScheduler;

        private bool _keepLoopAlive = true;

        //used to track if a random animation loop should be added to the animaiton flow when it is started or recreated
        private bool _shouldStartRandomAnim = false;
        private int _randomAnimationTime = 5000;

        public AnimationEngine(IAnimationUpdateView display)
        {
            //start the thread that will keep the animation flow alive
            Task.Run(RestartActionBlock);

            _uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            _currentDisplay = display;
        }

        ~AnimationEngine()
        {
            _keepLoopAlive = false;
            _currentDisplay = null;
        }

        private async Task RestartActionBlock()
        {
            while (_keepLoopAlive)
            {
                _animationFlow = new AnimationFlow((arg) =>
                {
                    _currentRenderedFrame = arg;
                    _currentDisplay.SignalRedraw();
                }, _uiTaskScheduler);

                if (_shouldStartRandomAnim)
                {
                    _animationFlow.StartRandomAnimationsLoop(_randomAnimationTime);
                }

                var completionTask = await Task.WhenAny(_animationFlow.CompletionTask);

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
                        Debug.WriteLine(completionTask.Exception.InnerException.InnerException.InnerException);
                        break;
                }
                Debug.WriteLine("\n" + message);
            }
        }

        public void AddAnimation(AnimationBase anim)
        {
            _animationFlow.InputBlock.Post(anim);
        }

        public void StartRandomAnimationsLoop(int msBetweenRandomAnim)
        {
            _randomAnimationTime = msBetweenRandomAnim;
            _shouldStartRandomAnim = true;

            _animationFlow.StartRandomAnimationsLoop(_randomAnimationTime);
        }

        public void StopRandomAnimationsLoop()
        {
            _shouldStartRandomAnim = false;
            _animationFlow.StopRandomAnimationsLoop();
        }

        public void DrawOnMe(SKSurface surface)
        {
            if (_currentRenderedFrame != null)
            {
                _currentRenderedFrame.DrawFunction(surface, _currentRenderedFrame.FramePoints);
                _currentRenderedFrame = null;
            }
            else
            {
                using (var c = surface.Canvas)
                {
                    c.Clear();
                }
            }
        }

        //public void UpdateTriangulationForRandom(Triangulation tri)
        //{
        //    _randomAnim.UpdateTriangulation(tri);
        //}
    }
}
