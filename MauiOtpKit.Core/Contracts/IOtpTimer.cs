namespace MauiOtpKit.Core.Contracts;

/// <summary>
/// Manages OTP countdown timer
/// </summary>
public interface IOtpTimer
{
    /// <summary>
    /// Starts a countdown timer
    /// </summary>
    void Start(int durationSeconds);

    /// <summary>
    /// Stops the countdown timer
    /// </summary>
    void Stop();

    /// <summary>
    /// Event fired on each second elapsed
    /// </summary>
    event EventHandler<int>? TimerElapsed; // remaining seconds

    /// <summary>
    /// Event fired when timer expires
    /// </summary>
    event EventHandler? TimerExpired;

    /// <summary>
    /// Current remaining time in seconds
    /// </summary>
    int RemainingSeconds { get; }

    /// <summary>
    /// Indicates if timer is running
    /// </summary>
    bool IsRunning { get; }
}
