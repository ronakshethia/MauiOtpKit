# Android Setup Guide

## Overview

MauiOtpKit uses **Google Play Services SMS Retriever API** for Android OTP extraction. This approach requires **no READ_SMS permission** and provides secure, automatic OTP detection.

---

## Prerequisites

✅ Android API 21+ (Android 5.0 Lollipop)
✅ Google Play Services installed on device
✅ App published on Google Play Store (or Firebase Console for testing)
✅ Signing certificate configured

---

## Step 1: Generate Your App Hash

The **app hash** is crucial for security. It ensures only your app receives the SMS containing the OTP.

### Method 1: Using AppHashHelper (Recommended)

```csharp
#if ANDROID
using Android.App;
using MauiOtpKit.Platforms.Android;

// In your MainActivity or during app initialization
protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    
    var context = Android.App.Application.Context;
    string appHash = AppHashHelper.GetAppHash(context);
    
    System.Diagnostics.Debug.WriteLine($"App Hash: {appHash}");
    // Result: ABC+D/EF+GHI (11 characters)
}
#endif
```

### Method 2: Manual Generation (Debugging)

```csharp
// Get your app's signing certificate hash
// 1. Open Android Studio
// 2. Build → Analyze APK
// 3. Find "SHA-1" under signing certificate
// 4. Copy the SHA-1 string

// Then compute the app hash:
// echo "package_name SHA-1_HASH" | openssl sha256 -binary | base64 | cut -c 1-11
```

### What is App Hash?

The app hash is an **11-character Base64 string** derived from:
1. Your app's package name
2. Your signing certificate's SHA-1 hash

**Why it matters**:
- Google Play Services includes this hash in incoming SMS messages
- Your backend service verifies the hash
- Only your app (with matching package + certificate) can read the OTP
- Prevents other apps from intercepting SMS

---

## Step 2: Configure Your Backend

### SMS Format Requirements

Your backend service MUST send SMS in this format:

```
<#> Your OTP is 123456 XXXXXXXXXXXXXXXX

```

Where:
- `<#>` - **Required prefix** (special Unicode characters)
- `Your OTP is 123456` - Your message
- `XXXXXXXXXXXXXXXX` - **Your app hash (11 characters)**

**Example SMS**:
```
<#> Your OTP is 654321 ABC+D/EF+GHI
```

### Backend Integration (Node.js Example)

```javascript
const APP_HASH = "ABC+D/EF+GHI";  // Get from step 1

async function sendOtpSms(phoneNumber, otpCode) {
    const smsBody = `<#> Your OTP is ${otpCode} ${APP_HASH}`;
    
    // Using Twilio
    const message = await twilio.messages.create({
        body: smsBody,
        from: '+1234567890',
        to: phoneNumber
    });
    
    return message.sid;
}
```

### Backend Integration (C# Example)

```csharp
public class SmsService
{
    private const string APP_HASH = "ABC+D/EF+GHI";
    
    public async Task SendOtpAsync(string phoneNumber, string otpCode)
    {
        var smsBody = $"<#> Your OTP is {otpCode} {APP_HASH}";
        
        // Using Twilio SDK
        var message = await MessageResource.CreateAsync(
            body: smsBody,
            from: new PhoneNumber("+1234567890"),
            to: new PhoneNumber(phoneNumber)
        );
    }
}
```

### Backend Integration (Python Example)

```python
from twilio.rest import Client

APP_HASH = "ABC+D/EF+GHI"

def send_otp_sms(phone_number, otp_code):
    sms_body = f"<#> Your OTP is {otp_code} {APP_HASH}"
    
    client = Client(account_sid, auth_token)
    message = client.messages.create(
        body=sms_body,
        from_="+1234567890",
        to=phone_number
    )
    
    return message.sid
```

---

## Step 3: Update AndroidManifest.xml

**Good news**: You need NO special permissions!

SMS Retriever API handles SMS access internally without exposing `READ_SMS` permission.

However, register the broadcast receiver if needed:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">

    <!-- No READ_SMS permission needed! -->
    
    <application>
        <!-- Your app's activities here -->
        
        <!-- SMS Retriever Broadcast Receiver (MAUI handles this) -->
        <!-- You typically don't need to add this - MauiOtpKit handles it -->
        
    </application>

</manifest>
```

---

## Step 4: Initialize in Your App

### In MauiProgram.cs

```csharp
using MauiOtpKit.Extensions;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register MauiOtpKit with Android support
        builder.Services.AddMauiOtp(options =>
        {
            options.Length = 6;                  // OTP length
            options.ExpirySeconds = 120;         // 2 minutes validity
            options.MaxAttempts = 3;             // Max validation attempts
        });

        builder.Logging.AddDebug();  // Enable debug logging

        return builder.Build();
    }
}
```

### In Your Page/ViewModel

```csharp
using MauiOtpKit.Core.Contracts;

public partial class LoginPage : ContentPage
{
    private readonly IOtpService _otpService;

    public LoginPage()
    {
        InitializeComponent();
        
        // Get OTP service from DI
        _otpService = IPlatformApplication.Current?.Services
            .GetRequiredService<IOtpService>()
            ?? throw new InvalidOperationException("OTP Service not found");

        // Subscribe to events
        _otpService.OtpDetected += OnOtpDetected;
        _otpService.ValidationError += OnValidationError;
        _otpService.OtpExpired += OnOtpExpired;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Start listening for SMS
        await _otpService.StartAsync();
        StatusLabel.Text = "Listening for OTP...";
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Stop listening
        await _otpService.StopAsync();
    }

    private void OnOtpDetected(object? sender, string otp)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OtpEntry.Text = otp;
            StatusLabel.Text = "✓ OTP Detected!";
        });
    }

    private void OnValidationError(object? sender, string error)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ErrorLabel.Text = error;
        });
    }

    private void OnOtpExpired(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "OTP Expired";
        });
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        var result = await _otpService.ValidateAsync(OtpEntry.Text);
        
        if (result.IsSuccess)
        {
            await DisplayAlert("Success", "OTP Validated!", "OK");
        }
    }
}
```

---

## Step 5: Testing

### Test on Real Device

1. **Build and deploy** to Android device
   ```bash
   dotnet build -f net8.0-android -c Debug
   dotnet publish -f net8.0-android -c Release
   ```

2. **Verify app installation**
   ```bash
   adb devices
   adb shell pm list packages | grep mauiotpkit
   ```

3. **Get your app hash**
   - Run the app
   - Check Debug output for: `App Hash: ABC+D/EF+GHI`
   - Or use: `adb logcat | grep "App Hash"`

4. **Send test SMS** (using backend)
   ```csharp
   var smsBody = "<#> Your OTP is 123456 ABC+D/EF+GHI";
   // Send via SMS API
   ```

5. **Observe automatic detection**
   - SMS arrives on device
   - App automatically extracts OTP
   - UI updates with OTP code

### Debugging Tips

```csharp
// Enable verbose logging
builder.Logging.AddDebug();

// Check logcat for SMS events
// adb logcat | grep "SMS_RETRIEVED"
// adb logcat | grep "MauiOtpKit"
```

---

## Troubleshooting

### Issue: "OTP not detected" / "No SMS received"

**Solution**:
1. Verify app hash matches backend configuration
2. Check SMS format includes `<#>` prefix
3. Ensure Google Play Services is installed on device
4. Device must be on Android 5.0+ (API 21+)

### Issue: "SMS Retriever task failed"

**Solution**:
1. App must be installed via Google Play or Firebase
2. Sideloaded/debug APKs may not work with SMS Retriever
3. Check network connectivity
4. Verify Google Play Services is updated

### Issue: "Permission denied" errors

**Solution**:
1. MauiOtpKit requires zero permissions
2. If you see permission errors, check for conflicting code
3. Ensure no custom manifest additions request READ_SMS

### Issue: "BroadcastReceiver not receiving SMS"

**Solution**:
1. Verify `<#>` prefix is present in SMS
2. Verify app hash is exactly 11 characters
3. Check app is running/not force-stopped
4. Verify SMS format: `<#> message ${HASH}`

---

## Security Best Practices

✅ **Always use HTTPS** for backend communication
✅ **Regenerate** app hash if signing certificate changes
✅ **Update** backend hash if app certificate is renewed
✅ **Validate** OTP on backend before processing
✅ **Log** OTP verification attempts for security audits
✅ **Rate-limit** OTP requests to prevent abuse
✅ **Set reasonable expiry** (120-300 seconds)
✅ **Enforce max attempts** (3-5) to prevent brute force

---

## Advanced Configuration

### Custom OTP Length

```csharp
// For 4-digit OTP
builder.Services.AddMauiOtp(options =>
{
    options.Length = 4;  // Changed from 6
});
```

### Custom Expiry

```csharp
// 5 minute validity
builder.Services.AddMauiOtp(options =>
{
    options.ExpirySeconds = 300;  // 5 minutes
});
```

### Custom Attempt Limits

```csharp
// 5 validation attempts
builder.Services.AddMauiOtp(options =>
{
    options.MaxAttempts = 5;
});
```

---

## Production Checklist

- [ ] App published on Google Play Store
- [ ] App hash generated from production signing certificate
- [ ] Backend configured with correct app hash
- [ ] SMS format includes `<#>` prefix and 11-char hash
- [ ] Logging enabled for monitoring
- [ ] Rate limiting implemented on backend
- [ ] OTP validation tested on real Android device
- [ ] Error handling implemented for all scenarios
- [ ] Security review completed
- [ ] User communication prepared

---

**Android SMS Retriever API provides the most secure and user-friendly OTP experience on Android.**
