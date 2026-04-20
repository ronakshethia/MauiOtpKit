namespace MauiOtpKit.Core.Contracts;

/// <summary>
/// Parses OTP codes from text messages
/// </summary>
public interface IOtpParser
{
    /// <summary>
    /// Extracts OTP code from text
    /// </summary>
    string? Parse(string text);

    /// <summary>
    /// Checks if text contains a valid OTP pattern
    /// </summary>
    bool Contains(string text);
}
