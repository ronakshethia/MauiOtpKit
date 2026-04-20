using MauiOtpKit.Core.Models;

namespace MauiOtpKit.Core.Contracts;

/// <summary>
/// Validates OTP codes against configured rules
/// </summary>
public interface IOtpValidator
{
    /// <summary>
    /// Validates OTP code against expected code, expiry, and attempts
    /// </summary>
    OtpResult Validate(string input, string expectedCode, DateTime generatedAt, int attemptCount);

    /// <summary>
    /// Checks if OTP has expired
    /// </summary>
    bool IsExpired(DateTime generatedAt);

    /// <summary>
    /// Checks if max attempts exceeded
    /// </summary>
    bool IsMaxAttemptsExceeded(int attemptCount);

    /// <summary>
    /// Gets remaining seconds until expiry
    /// </summary>
    int GetRemainingSeconds(DateTime generatedAt);
}
