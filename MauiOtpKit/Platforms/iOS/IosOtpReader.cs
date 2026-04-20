#if IOS

using MauiOtpKit.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace MauiOtpKit.Platforms.iOS;

/// <summary>
/// iOS OTP Reader
/// 
/// NOTE: iOS does not provide direct SMS access due to privacy restrictions.
/// Instead, OTP codes are delivered via:
/// 1. UIPasteboard (user pastes from SMS notification)
/// 2. One Time Password (OTP) autofill support
/// 3. User manual entry
/// 
/// This reader provides a base for iOS OTP handling and can be extended
/// to support autofill UI integration via UITextContentType.OneTimeCode
/// </summary>
public class IosOtpReader : IOtpReader
{
    private readonly ILogger<IosOtpReader>? _logger;
    private bool _isListening;

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;

    public IosOtpReader(ILogger<IosOtpReader>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts listening for OTP (iOS uses autofill or manual entry)
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isListening)
            return Task.CompletedTask;

        try
        {
            _logger?.LogInformation("Starting iOS OTP Reader");
            _logger?.LogInformation("iOS: Configure UITextContentType.OneTimeCode on your entry field for autofill support");
            
            _isListening = true;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting iOS OTP reader");
            ErrorOccurred?.Invoke(this, $"Failed to start OTP reader: {ex.Message}");
            return Task.FromException(ex);
        }
    }

    /// <summary>
    /// Stops listening for OTP
    /// </summary>
    public Task StopAsync()
    {
        if (!_isListening)
            return Task.CompletedTask;

        try
        {
            _logger?.LogInformation("Stopping iOS OTP Reader");
            _isListening = false;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error stopping iOS OTP reader");
            return Task.FromException(ex);
        }
    }

    /// <summary>
    /// Helper method to trigger OTP detection from autofill or clipboard
    /// </summary>
    public void DetectOtpFromPasteboard(string? otpCode)
    {
        if (string.IsNullOrEmpty(otpCode))
        {
            ErrorOccurred?.Invoke(this, "No OTP found in pasteboard");
            return;
        }

        _logger?.LogInformation("OTP detected from pasteboard");
        OtpReceived?.Invoke(this, otpCode);
    }
}

#endif
