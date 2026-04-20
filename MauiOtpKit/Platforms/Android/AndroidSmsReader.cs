#if ANDROID

using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.Phone;
using Android.OS;
using MauiOtpKit.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace MauiOtpKit.Platforms.Android;

/// <summary>
/// Android SMS Retriever implementation using Google Play Services
/// </summary>
public class AndroidSmsReader : IOtpReader
{
    private SmsRetrievalClient? _smsClient;
    private readonly ILogger<AndroidSmsReader>? _logger;
    private bool _isListening;

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;

    public AndroidSmsReader(ILogger<AndroidSmsReader>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts listening for SMS messages
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isListening)
            return Task.CompletedTask;

        try
        {
            _logger?.LogInformation("Starting Android SMS Reader");
            
            _smsClient = new SmsRetrievalClient();
            _smsClient.OtpReceived += (sender, otp) => OtpReceived?.Invoke(this, otp);
            _smsClient.ErrorOccurred += (sender, error) => ErrorOccurred?.Invoke(this, error);
            
            _smsClient.Start();
            _isListening = true;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting SMS reader");
            ErrorOccurred?.Invoke(this, $"Failed to start SMS reader: {ex.Message}");
            return Task.FromException(ex);
        }
    }

    /// <summary>
    /// Stops listening for SMS messages
    /// </summary>
    public Task StopAsync()
    {
        if (!_isListening)
            return Task.CompletedTask;

        try
        {
            _logger?.LogInformation("Stopping Android SMS Reader");
            _smsClient?.Stop();
            _isListening = false;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error stopping SMS reader");
            return Task.FromException(ex);
        }
    }
}

#endif
