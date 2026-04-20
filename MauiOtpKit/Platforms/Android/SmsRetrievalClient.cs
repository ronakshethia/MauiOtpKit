#if ANDROID

using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.Phone;
using Android.OS;
using Microsoft.Extensions.Logging;

namespace MauiOtpKit.Platforms.Android;

/// <summary>
/// Manages SMS retrieval client and handles SMS broadcast messages
/// </summary>
internal class SmsRetrievalClient
{
    private SmsRetrievalTask? _smsRetrievalTask;
    private BroadcastReceiver? _broadcastReceiver;
    private readonly ILogger? _logger;
    private bool _isRunning;

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;

    public SmsRetrievalClient(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts the SMS retrieval task
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        try
        {
            _logger?.LogInformation("Starting SMS Retrieval Task");
            
            _smsRetrievalTask = new SmsRetrievalTask();
            _broadcastReceiver = new SmsBroadcastReceiver(_logger);
            
            // Subscribe to OTP received event
            ((SmsBroadcastReceiver)_broadcastReceiver).OtpReceived += (sender, otp) =>
                OtpReceived?.Invoke(this, otp);
            
            ((SmsBroadcastReceiver)_broadcastReceiver).ErrorOccurred += (sender, error) =>
                ErrorOccurred?.Invoke(this, error);

            _smsRetrievalTask.StartSmsRetrieval();
            _isRunning = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting SMS retrieval");
            ErrorOccurred?.Invoke(this, $"SMS retrieval error: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops the SMS retrieval task
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        try
        {
            _logger?.LogInformation("Stopping SMS Retrieval Task");
            
            if (_broadcastReceiver != null)
            {
                try
                {
                    Application.Context.UnregisterReceiver(_broadcastReceiver);
                }
                catch { /* Receiver already unregistered */ }
            }

            _smsRetrievalTask?.Stop();
            _isRunning = false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error stopping SMS retrieval");
        }
    }
}

/// <summary>
/// Broadcast receiver for SMS messages from Google Play Services
/// </summary>
[BroadcastReceiver]
[IntentFilter(new[] { "com.google.android.gms.auth.api.phone.SMS_RETRIEVED" })]
internal class SmsBroadcastReceiver : BroadcastReceiver
{
    private readonly ILogger? _logger;

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;

    public SmsBroadcastReceiver(ILogger? logger = null)
    {
        _logger = logger;
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent == null)
            return;

        try
        {
            if (intent.Action == "com.google.android.gms.auth.api.phone.SMS_RETRIEVED")
            {
                var extras = intent.Extras;
                if (extras == null)
                {
                    ErrorOccurred?.Invoke(this, "No data in SMS intent");
                    return;
                }

                // Get the SMS message
                var smsMessage = extras.GetString("com.google.android.gms.auth.api.phone.SMS_MESSAGE");
                
                if (string.IsNullOrEmpty(smsMessage))
                {
                    ErrorOccurred?.Invoke(this, "SMS message is empty");
                    return;
                }

                _logger?.LogInformation($"SMS received: {smsMessage}");
                OtpReceived?.Invoke(this, smsMessage);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing SMS");
            ErrorOccurred?.Invoke(this, $"Error processing SMS: {ex.Message}");
        }
    }
}

/// <summary>
/// Wrapper for SMS Retrieval API task
/// </summary>
internal class SmsRetrievalTask : IDisposable
{
    private Android.Gms.Tasks.Task? _task;
    private readonly ILogger? _logger;

    public SmsRetrievalTask(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts the SMS retrieval process
    /// </summary>
    public void StartSmsRetrieval()
    {
        try
        {
            var client = SmsRetriever.GetClient(Application.Context);
            
            _task = client.StartSmsUserConsent(null); // null = listen to all senders
            
            _logger?.LogInformation("SMS Retrieval started with SMS Retriever API");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting SMS retrieval with API");
            throw;
        }
    }

    /// <summary>
    /// Stops the SMS retrieval process
    /// </summary>
    public void Stop()
    {
        _task = null;
    }

    public void Dispose()
    {
        Stop();
    }
}

#endif
