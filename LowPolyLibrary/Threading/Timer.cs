using System;
using System.Threading.Tasks;

namespace LowPolyLibrary.Threading
{
    public delegate Task<bool> TimerCallback(object state);

    //http://stackoverflow.com/a/33683963/3344317
    public class Timer
    {
        private bool timerRunning;
        private int interval;
        private TimerCallback tick;
        private bool runOnce;

        public Timer(int interval, TimerCallback tick, bool runOnce = false)
        {
            this.interval = interval;
            this.tick = tick;
            this.runOnce = runOnce;
        }

        public Timer Start()
        {
            if (!timerRunning)
            {
                timerRunning = true;
                RunTimer();
            }

            return this;
        }

        public void Stop()
        {
            timerRunning = false;
        }

        private async Task RunTimer()
        {
            while (timerRunning)
            {
                await Task.Delay(interval);

                if (timerRunning)
                {
                    //tick(this);
                    //intention is to complete whatever frame before the next
                    await tick(this);

                    if (runOnce)
                    {
                        Stop();
                    }
                }
            }
        }
    }
}
