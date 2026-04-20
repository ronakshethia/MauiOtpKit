using MauiOtpKit.Core.Models;

namespace MauiOtpKit.Core.Contracts;

/// <summary>
/// Main OTP service that orchestrates the full OTP flow
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Starts OTP reading and timer
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops OTP reading and timer
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Validates user input against expected OTP
    /// </summary>
    Task<OtpResult> ValidateAsync(string userInput);

    /// <summary>
    /// Resets OTP state
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Gets current OTP state
    /// </summary>
    OtpState GetState();

    /// <summary>
    /// Event fired when OTP is auto-detected
    /// </summary>
    event EventHandler<string>? OtpDetected;

    /// <summary>
    /// Event fired on validation error
    /// </summary>
    event EventHandler<string>? ValidationError;

    /// <summary>
    /// Event fired on OTP expiry
    /// </summary>
    event EventHandler? OtpExpired;

    /// <summary>
    /// Event fired when max attempts exceeded
    /// </summary>
    event EventHandler? MaxAttemptsExceeded;
}

/// <summary>
/// Current state of OTP service
/// </summary>
public enum OtpState
{
    Idle,
    Listening,
    Received,
    Validated,
    Expired,
    Failed
}
