using MauiOtpKit.Core.Contracts;

namespace MauiOtpKit.Sample;

public partial class MainPage : ContentPage
{
    private readonly IOtpService _otpService;
    private int _otpLength = 6;

    public MainPage()
    {
        InitializeComponent();
        
        // Get OTP service from DI
        _otpService = IPlatformApplication.Current?.Services.GetService<IOtpService>()
            ?? throw new InvalidOperationException("OTP Service not found");

        // Subscribe to OTP events
        _otpService.OtpDetected += OnOtpDetected;
        _otpService.ValidationError += OnValidationError;
        _otpService.OtpExpired += OnOtpExpired;
        _otpService.MaxAttemptsExceeded += OnMaxAttemptsExceeded;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Start OTP service
        await _otpService.StartAsync();
        StatusLabel.Text = "Listening for OTP...";
        StatusLabel.TextColor = Colors.Blue;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Stop OTP service
        await _otpService.StopAsync();
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        var otpInput = OtpEntry.Text?.Trim();

        if (string.IsNullOrEmpty(otpInput))
        {
            await DisplayAlert("Error", "Please enter OTP code", "OK");
            return;
        }

        var result = await _otpService.ValidateAsync(otpInput);

        if (result.IsSuccess)
        {
            StatusLabel.Text = "✓ OTP Validated Successfully!";
            StatusLabel.TextColor = Colors.Green;
            OtpEntry.Text = string.Empty;
            await DisplayAlert("Success", $"OTP validated!\nCode: {result.Code}", "OK");
        }
        else
        {
            StatusLabel.Text = $"✗ Validation failed: {result.Message}";
            StatusLabel.TextColor = Colors.Red;
            ErrorLabel.Text = $"Attempt {result.AttemptCount}";
        }
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        await _otpService.ResetAsync();
        OtpEntry.Text = string.Empty;
        StatusLabel.Text = "Reset - listening for new OTP...";
        StatusLabel.TextColor = Colors.Blue;
        ErrorLabel.Text = string.Empty;
    }

    private void OnOtpDetected(object? sender, string otp)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "✓ OTP Detected!";
            StatusLabel.TextColor = Colors.Green;
            OtpEntry.Text = otp;
            
            // Auto-focus for user to confirm
            OtpEntry.Focus();
        });
    }

    private void OnValidationError(object? sender, string error)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ErrorLabel.Text = error;
            StatusLabel.TextColor = Colors.Red;
        });
    }

    private void OnOtpExpired(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "✗ OTP Expired - Request new OTP";
            StatusLabel.TextColor = Colors.Red;
            OtpEntry.Text = string.Empty;
        });
    }

    private void OnMaxAttemptsExceeded(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "✗ Max attempts exceeded - Request new OTP";
            StatusLabel.TextColor = Colors.Red;
        });
    }
}
