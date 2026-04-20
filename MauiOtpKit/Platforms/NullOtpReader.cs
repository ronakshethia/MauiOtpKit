using MauiOtpKit.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace MauiOtpKit.Platforms;

/// <summary>
/// Fallback OTP reader for unsupported platforms
/// </summary>
internal class NullOtpReader : IOtpReader
{
    private readonly ILogger? _logger;

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;

    public NullOtpReader(ILogger? logger = null)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogWarning("OTP Reader not supported on this platform");
        ErrorOccurred?.Invoke(this, "OTP Reader is not supported on this platform");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
