using MauiOtpKit.Core.Contracts;
using MauiOtpKit.Core.Models;

namespace MauiOtpKit.Core.Implementations;

/// <summary>
/// Validates OTP codes against configured rules
/// </summary>
public class OtpValidator : IOtpValidator
{
    private readonly OtpOptions _options;

    public OtpValidator(OtpOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Validates OTP code
    /// </summary>
    public OtpResult Validate(string input, string expectedCode, DateTime generatedAt, int attemptCount)
    {
        if (string.IsNullOrEmpty(input))
            return OtpResult.Failure("OTP cannot be empty");

        if (string.IsNullOrEmpty(expectedCode))
            return OtpResult.Failure("No OTP was generated");

        // Check expiry
        if (IsExpired(generatedAt))
            return OtpResult.Failure("OTP has expired");

        // Check max attempts
        if (IsMaxAttemptsExceeded(attemptCount))
            return OtpResult.Failure("Maximum validation attempts exceeded");

        // Validate OTP
        if (!input.Equals(expectedCode, StringComparison.Ordinal))
            return OtpResult.Failure("OTP is incorrect");

        var remainingSeconds = GetRemainingSeconds(generatedAt);
        return OtpResult.Success(input, remainingSeconds);
    }

    /// <summary>
    /// Checks if OTP has expired
    /// </summary>
    public bool IsExpired(DateTime generatedAt)
    {
        var elapsedSeconds = (DateTime.UtcNow - generatedAt).TotalSeconds;
        return elapsedSeconds >= _options.ExpirySeconds;
    }

    /// <summary>
    /// Checks if max attempts exceeded
    /// </summary>
    public bool IsMaxAttemptsExceeded(int attemptCount)
    {
        return attemptCount >= _options.MaxAttempts;
    }

    /// <summary>
    /// Gets remaining seconds until expiry
    /// </summary>
    public int GetRemainingSeconds(DateTime generatedAt)
    {
        var elapsedSeconds = (int)(DateTime.UtcNow - generatedAt).TotalSeconds;
        var remaining = _options.ExpirySeconds - elapsedSeconds;
        return Math.Max(0, remaining);
    }
}
