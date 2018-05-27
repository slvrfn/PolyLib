using System;
using System.Threading.Tasks;

namespace PolyLib.Threading
{
    public delegate Task<bool> TimerCallback(object state);

    //http://stackoverflow.com/a/33683963/3344317
    public class Timer
    {
        private bool _timerRunning;
        private int _interval;
        private TimerCallback _tick;
        private bool _runOnce;

        public Timer(int interval, TimerCallback tick, bool runOnce = false)
        {
            this._interval = interval;
            this._tick = tick;
            this._runOnce = runOnce;
        }

        public Timer Start()
        {
            if (!_timerRunning)
            {
                _timerRunning = true;
                RunTimer();
            }

            return this;
        }

        public void Stop()
        {
            _timerRunning = false;
        }

        public void UpdateInterval(int interval)
        {
            _interval = interval;
        }

        private async Task RunTimer()
        {
            while (_timerRunning)
            {
                await Task.Delay(_interval);

                if (_timerRunning)
                {
                    //tick(this);
                    //intention is to complete whatever frame before the next
                    await _tick(this);

                    if (_runOnce)
                    {
                        Stop();
                    }
                }
            }
        }
    }
}
