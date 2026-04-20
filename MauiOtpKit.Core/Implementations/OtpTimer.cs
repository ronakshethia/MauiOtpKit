using MauiOtpKit.Core.Contracts;

namespace MauiOtpKit.Core.Implementations;

/// <summary>
/// Countdown timer for OTP expiry
/// </summary>
public class OtpTimer : IOtpTimer, IDisposable
{
    private System.Timers.Timer? _timer;
    private int _totalSeconds;
    private int _remainingSeconds;

    public int RemainingSeconds => _remainingSeconds;

    public bool IsRunning => _timer?.Enabled ?? false;

    public event EventHandler<int>? TimerElapsed;
    public event EventHandler? TimerExpired;

    /// <summary>
    /// Starts the countdown timer
    /// </summary>
    public void Start(int durationSeconds)
    {
        _totalSeconds = durationSeconds;
        _remainingSeconds = durationSeconds;

        _timer = new System.Timers.Timer(1000) // 1 second interval
        {
            AutoReset = true,
            Enabled = true
        };

        _timer.Elapsed += (s, e) =>
        {
            _remainingSeconds--;
            TimerElapsed?.Invoke(this, _remainingSeconds);

            if (_remainingSeconds <= 0)
            {
                Stop();
                TimerExpired?.Invoke(this, EventArgs.Empty);
            }
        };
    }

    /// <summary>
    /// Stops the countdown timer
    /// </summary>
    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        _remainingSeconds = 0;
    }

    public void Dispose()
    {
        Stop();
    }
}
