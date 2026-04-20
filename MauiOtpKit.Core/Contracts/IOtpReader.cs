namespace MauiOtpKit.Core.Contracts;

/// <summary>
/// Reads OTP from platform-specific sources (SMS, clipboard, etc.)
/// </summary>
public interface IOtpReader
{
    /// <summary>
    /// Starts listening for OTP
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops listening for OTP
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Event fired when OTP is received
    /// </summary>
    event EventHandler<string>? OtpReceived;

    /// <summary>
    /// Event fired when error occurs
    /// </summary>
    event EventHandler<string>? ErrorOccurred;
}
