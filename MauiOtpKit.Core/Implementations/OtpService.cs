using Microsoft.Extensions.Logging;
using MauiOtpKit.Core.Contracts;
using MauiOtpKit.Core.Models;

namespace MauiOtpKit.Core.Implementations;

/// <summary>
/// Main OTP service orchestrating the full OTP flow
/// </summary>
public class OtpService : IOtpService, IDisposable
{
    private readonly IOtpReader _reader;
    private readonly IOtpParser _parser;
    private readonly IOtpValidator _validator;
    private readonly IOtpTimer _timer;
    private readonly OtpOptions _options;
    private readonly ILogger<OtpService>? _logger;

    private string? _generatedCode;
    private DateTime _generatedAt;
    private int _attemptCount;
    private OtpState _state = OtpState.Idle;
    private readonly object _lockObject = new();

    public event EventHandler<string>? OtpDetected;
    public event EventHandler<string>? ValidationError;
    public event EventHandler? OtpExpired;
    public event EventHandler? MaxAttemptsExceeded;

    public OtpService(
        IOtpReader reader,
        IOtpParser parser,
        IOtpValidator validator,
        IOtpTimer timer,
        OtpOptions options,
        ILogger<OtpService>? logger = null)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        SubscribeToReaderEvents();
    }

    /// <summary>
    /// Starts OTP reading
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (_state == OtpState.Listening)
                return;

            _state = OtpState.Listening;
            _generatedCode = null;
            _attemptCount = 0;
        }

        _logger?.LogInformation("Starting OTP service");
        
        await _reader.StartAsync(cancellationToken);
        _timer.Start(_options.ExpirySeconds);
    }

    /// <summary>
    /// Stops OTP reading
    /// </summary>
    public async Task StopAsync()
    {
        lock (_lockObject)
        {
            _state = OtpState.Idle;
        }

        _logger?.LogInformation("Stopping OTP service");
        
        await _reader.StopAsync();
        _timer.Stop();
    }

    /// <summary>
    /// Validates user input
    /// </summary>
    public async Task<OtpResult> ValidateAsync(string userInput)
    {
        lock (_lockObject)
        {
            if (string.IsNullOrEmpty(_generatedCode))
            {
                var error = "No OTP was generated";
                _logger?.LogWarning(error);
                ValidationError?.Invoke(this, error);
                return OtpResult.Failure(error);
            }

            _attemptCount++;

            // Check if already exceeded
            if (_validator.IsMaxAttemptsExceeded(_attemptCount))
            {
                var error = "Maximum validation attempts exceeded";
                _logger?.LogWarning(error);
                ValidationError?.Invoke(this, error);
                MaxAttemptsExceeded?.Invoke(this, EventArgs.Empty);
                return OtpResult.Failure(error);
            }

            // Check if expired
            if (_validator.IsExpired(_generatedAt))
            {
                var error = "OTP has expired";
                _logger?.LogWarning(error);
                ValidationError?.Invoke(this, error);
                OtpExpired?.Invoke(this, EventArgs.Empty);
                _state = OtpState.Expired;
                return OtpResult.Failure(error);
            }

            var result = _validator.Validate(userInput, _generatedCode, _generatedAt, _attemptCount);

            if (result.IsSuccess)
            {
                _state = OtpState.Validated;
                _logger?.LogInformation("OTP validation successful");
            }
            else
            {
                _logger?.LogWarning($"OTP validation failed: {result.Message}");
            }

            result.AttemptCount = _attemptCount;
            return result;
        }
    }

    /// <summary>
    /// Resets OTP state
    /// </summary>
    public Task ResetAsync()
    {
        lock (_lockObject)
        {
            _generatedCode = null;
            _generatedAt = DateTime.MinValue;
            _attemptCount = 0;
            _state = OtpState.Idle;
        }

        _logger?.LogInformation("OTP service reset");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets current state
    /// </summary>
    public OtpState GetState()
    {
        lock (_lockObject)
        {
            return _state;
        }
    }

    private void SubscribeToReaderEvents()
    {
        _reader.OtpReceived += async (sender, otp) =>
        {
            _logger?.LogInformation("OTP received from reader");
            
            lock (_lockObject)
            {
                if (_state != OtpState.Listening)
                    return;

                _generatedCode = otp;
                _generatedAt = DateTime.UtcNow;
                _state = OtpState.Received;
            }

            OtpDetected?.Invoke(this, otp);
            await Task.CompletedTask;
        };

        _reader.ErrorOccurred += (sender, error) =>
        {
            _logger?.LogError($"OTP reader error: {error}");
            ValidationError?.Invoke(this, error);
        };

        _timer.TimerExpired += (sender, e) =>
        {
            lock (_lockObject)
            {
                if (_state == OtpState.Listening || _state == OtpState.Received)
                {
                    _state = OtpState.Expired;
                }
            }

            _logger?.LogInformation("OTP expired");
            OtpExpired?.Invoke(this, EventArgs.Empty);
        };
    }

    public void Dispose()
    {
        _timer?.Stop();
        (_timer as IDisposable)?.Dispose();
    }
}
