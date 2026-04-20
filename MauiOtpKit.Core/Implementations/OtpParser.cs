using System.Text.RegularExpressions;
using MauiOtpKit.Core.Contracts;

namespace MauiOtpKit.Core.Implementations;

/// <summary>
/// Regex-based OTP parser that extracts codes from text
/// </summary>
public class OtpParser : IOtpParser
{
    private readonly int _otpLength;

    public OtpParser(int otpLength = 6)
    {
        _otpLength = otpLength;
    }

    /// <summary>
    /// Extracts OTP from text using regex patterns
    /// </summary>
    public string? Parse(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        // Pattern: matches 4-6 digit sequences
        var pattern = @"\b\d{4,6}\b";
        var matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            if (match.Value.Length == _otpLength)
                return match.Value;
        }

        // If exact length not found, return first match of any 4-6 digit code
        if (matches.Count > 0)
            return matches[0].Value;

        return null;
    }

    /// <summary>
    /// Checks if text contains OTP pattern
    /// </summary>
    public bool Contains(string text)
    {
        return !string.IsNullOrEmpty(Parse(text));
    }
}
