namespace MauiOtpKit.Core.Models;

/// <summary>
/// Result of OTP validation
/// </summary>
public class OtpResult
{
    /// <summary>
    /// Indicates if validation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// OTP code that was validated (if successful)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Number of attempts made
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Remaining time in seconds (if not expired)
    /// </summary>
    public int RemainingSeconds { get; set; }

    /// <summary>
    /// Static method to create success result
    /// </summary>
    public static OtpResult Success(string code, int remainingSeconds = 0) =>
        new()
        {
            IsSuccess = true,
            Code = code,
            RemainingSeconds = remainingSeconds,
            Message = "Validation successful"
        };

    /// <summary>
    /// Static method to create failure result
    /// </summary>
    public static OtpResult Failure(string message) =>
        new()
        {
            IsSuccess = false,
            Message = message
        };
}
