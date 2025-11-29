using System;
using ScreenShield.Core.Interfaces;

namespace ScreenShield.Core.Services
{
    public class IdleDetector : IDisposable
    {
        private readonly IInputService _inputService;

        private readonly System.Timers.Timer _timer;
        private bool _isIdle;

        public event Action IdleDetected;
        public event Action ActivityDetected;

        public IdleDetector(IInputService inputService)
        {
            _inputService = inputService;

            // Set up the timer for 5 seconds (5000ms)
            _timer = new System.Timers.Timer(5000);
            _timer.AutoReset = false; // Run once, then wait for reset
            _timer.Elapsed += OnTimerElapsed;

            // Subscribe to mouse movement
            _inputService.MouseMoved += OnMouseMoved;
        }

        private void OnMouseMoved(object sender, System.Drawing.Point e)
        {
            // Reset the timer whenever the mouse moves
            _timer.Stop();
            _timer.Start();

            if (_isIdle)
            {
                _isIdle = false;
                ActivityDetected?.Invoke();
            }
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isIdle)
            {
                _isIdle = true;
                IdleDetected?.Invoke();
            }
        }

        // Add this new method to IdleDetector.cs
        public void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
            _isIdle = false;
            ActivityDetected?.Invoke(); // Tell the UI we are active again
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
            _inputService.MouseMoved -= OnMouseMoved;
        }
    }
}