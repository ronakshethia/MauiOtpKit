namespace MauiOtpKit.Core.Models;

/// <summary>
/// Configuration options for OTP service
/// </summary>
public class OtpOptions
{
    /// <summary>
    /// Length of OTP code (default: 6)
    /// </summary>
    public int Length { get; set; } = 6;

    /// <summary>
    /// OTP expiry time in seconds (default: 120)
    /// </summary>
    public int ExpirySeconds { get; set; } = 120;

    /// <summary>
    /// Maximum validation attempts allowed (default: 3)
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Enable auto-start of OTP reader (default: true)
    /// </summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Time in milliseconds to wait for OTP before timeout (default: 300000 = 5 minutes)
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 300000;
}
