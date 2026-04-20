# iOS Setup Guide

## Overview

iOS does not provide direct SMS access due to privacy restrictions. Instead, MauiOtpKit leverages iOS's native **OTP autofill** support, which provides a secure and user-friendly experience.

---

## Prerequisites

✅ iOS 14.2+
✅ MAUI configured for iOS
✅ SMS sender uses standard OTP format

---

## How iOS OTP Autofill Works

iOS automatically detects OTP codes in incoming SMS messages and offers to autofill them in text input fields marked with `UITextContentType.OneTimeCode`.

**Flow**:
```
SMS arrives: "Your OTP is 123456"
           ↓
iOS system detects OTP pattern
           ↓
User sees autofill suggestion in notification
           ↓
User taps "Use OTP" or "Allow"
           ↓
Entry field auto-populates with "123456"
           ↓
Your app processes the OTP
```

---

## Step 1: Configure Entry Field

### Mark Entry Field for OTP Autofill

In your XAML, the `Entry` control should accept OTP codes:

```xml
<Entry
    x:Name="OtpEntry"
    Placeholder="000000"
    PlaceholderColor="LightGray"
    FontSize="24"
    FontAttributes="Bold"
    HorizontalTextAlignment="Center"
    Keyboard="Numeric"
    MaxLength="6"
    CharacterSpacing="5" />
```

MauiOtpKit automatically configures the iOS equivalent of `UITextContentType.OneTimeCode` internally when the `IosOtpReader` is initialized.

### Best Practice: Clear Visual Feedback

```xml
<VerticalStackLayout Spacing="15">
    <Label
        FontSize="14"
        FontAttributes="Bold"
        Text="Enter OTP Code" />

    <Entry
        x:Name="OtpEntry"
        Placeholder="000000"
        PlaceholderColor="LightGray"
        FontSize="24"
        FontAttributes="Bold"
        HorizontalTextAlignment="Center"
        Keyboard="Numeric"
        MaxLength="6"
        CharacterSpacing="5" />

    <Label
        FontSize="12"
        Text="Autofill is enabled - approve from notification"
        TextColor="Gray"
        FontAttributes="Italic" />
</VerticalStackLayout>
```

---

## Step 2: Initialize in Your App

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register MauiOtpKit
        builder.Services.AddMauiOtp(options =>
        {
            options.Length = 6;
            options.ExpirySeconds = 120;
            options.MaxAttempts = 3;
        });

        builder.Logging.AddDebug();

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
        
        // Start listening (passively on iOS)
        await _otpService.StartAsync();
        StatusLabel.Text = "Ready to receive OTP";
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
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
        else
        {
            await DisplayAlert("Error", result.Message, "OK");
        }
    }
}
```

---

## Step 3: Configure Backend SMS

### SMS Format

iOS expects standard OTP format in SMS:

```
Your OTP is 123456
```

**Why?**  
iOS's native OTP detection uses regex to find digit sequences in SMS messages.

### Backend Examples

#### C# / .NET
```csharp
public class SmsService
{
    public async Task SendOtpAsync(string phoneNumber, string otpCode)
    {
        var smsBody = $"Your OTP is {otpCode}";
        
        // Using Twilio
        var message = await MessageResource.CreateAsync(
            body: smsBody,
            from: new PhoneNumber("+1234567890"),
            to: new PhoneNumber(phoneNumber)
        );
    }
}
```

#### Node.js
```javascript
async function sendOtpSms(phoneNumber, otpCode) {
    const smsBody = `Your OTP is ${otpCode}`;
    
    const message = await twilio.messages.create({
        body: smsBody,
        from: '+1234567890',
        to: phoneNumber
    });
}
```

#### Python
```python
def send_otp_sms(phone_number, otp_code):
    sms_body = f"Your OTP is {otp_code}"
    
    message = client.messages.create(
        body=sms_body,
        from_="+1234567890",
        to=phone_number
    )
```

---

## Step 4: Info.plist Configuration

Fortunately, **iOS handles OTP autofill automatically**. No Info.plist modifications needed!

However, for completeness, here's what iOS does internally:

```xml
<!-- This is handled by iOS automatically -->
<!-- Your app inherits OTP autofill capability from Entry field -->
```

---

## Step 5: Testing

### On Simulator

**Limitation**: SMS Retriever doesn't work on iOS Simulator.

**Workaround**: Use manual entry or clipboard simulation:

```csharp
#if IOS
var reader = serviceProvider.GetRequiredService<IOtpReader>();

if (reader is Platforms.iOS.IosOtpReader iosReader)
{
    // Manually trigger for testing
    iosReader.DetectOtpFromPasteboard("123456");
}
#endif
```

### On Real Device

1. **Deploy to iOS device**
   ```bash
   dotnet publish -f net8.0-ios -c Release
   ```

2. **Receive SMS** containing OTP code
   - Notification appears: "Your app sent you a verification code"

3. **User action**: Tap notification or entry field
   - Autofill suggestion appears
   - User taps "Use..." button
   - OTP populates entry field

4. **App processes OTP**
   - Validation happens automatically or on user tap

---

## Advanced: Manual Pasteboard Detection

If you need to handle clipboard OTP manually (optional):

```csharp
#if IOS
using MauiOtpKit.Platforms.iOS;

private async void OnAppearing()
{
    // Check pasteboard (optional alternative)
    var pasteboard = UIPasteboard.General;
    if (!string.IsNullOrEmpty(pasteboard?.String))
    {
        var otpContent = pasteboard.String;
        
        // If it looks like OTP, trigger detection
        if (Regex.IsMatch(otpContent, @"^\d{4,6}$"))
        {
            var reader = (IosOtpReader)_otpService.GetType()
                .GetField("_reader", BindingFlags.NonPublic)
                ?.GetValue(_otpService);
            
            reader?.DetectOtpFromPasteboard(otpContent);
        }
    }
}
#endif
```

---

## User Experience Flow

### Happy Path (iOS Autofill)

```
1. User navigates to login page
   ↓
2. Page displays OTP entry field
   ↓
3. User initiates login
   ↓
4. Backend sends SMS with OTP
   ↓
5. Notification appears:
   "App sent you a verification code"
   ↓
6. User taps notification
   ↓
7. Autofill suggestion appears
   (with OTP code pre-filled)
   ↓
8. User taps "Use..." or field populates
   ↓
9. Entry field shows: "123456"
   ↓
10. App validates automatically or user taps "Verify"
    ↓
11. Success!
```

---

## Troubleshooting

### Issue: Autofill not appearing

**Solution**:
1. Verify entry field has `Keyboard="Numeric"`
2. Ensure SMS contains valid OTP format
3. Check iOS version is 14.2+
4. Grant notification permission for SMS alerts
5. Device must be unlocked to see autofill

### Issue: OTP not detected in SMS

**Solution**:
1. SMS must contain 4-6 digit sequence
2. Verify SMS isn't filtered as spam
3. Check notification settings for SMS app
4. Test with different OTP format variations

### Issue: Manual entry not validated

**Solution**:
1. Verify entry field type is Numeric keyboard
2. Check OTP length matches configuration
3. Ensure MaxLength attribute is set to 6
4. Verify validation logic on backend

### Issue: User dismisses notification

**Normal behavior**: User must re-initiate login to receive new OTP

---

## iOS-Specific Best Practices

✅ **Standard SMS Format**: Keep SMS simple and clear
   ```
   Your OTP is 123456
   ```

✅ **Sender Name**: Use recognizable sender ID
   - Two-letter country code (e.g., "US")
   - Short alphanumeric code

✅ **Reasonable Expiry**: 2-3 minutes
   ```csharp
   options.ExpirySeconds = 120; // 2 minutes
   ```

✅ **Clear UI**: Show instructions
   ```xml
   <Label Text="Check your notification for autofill" />
   ```

✅ **Fallback Entry**: Allow manual entry if autofill fails
   ```xml
   <Label FontSize="12" Text="Or enter code manually" />
   <Entry x:Name="OtpEntry" Placeholder="000000" />
   ```

✅ **Test on Real Device**: Simulator doesn't support SMS

---

## Production Checklist - iOS

- [ ] App built for iOS 14.2+
- [ ] Entry field configured with Numeric keyboard
- [ ] SMS format validated (contains OTP digits)
- [ ] OTP length matches configuration (default: 6)
- [ ] MaxLength attribute set correctly
- [ ] Notification permissions tested
- [ ] Real device testing completed
- [ ] Error handling for expired OTP
- [ ] Fallback manual entry implemented
- [ ] User instructions clear and visible

---

## Comparison: Android vs iOS

| Feature | Android | iOS |
|---------|---------|-----|
| **Method** | SMS Retriever API | Autofill API |
| **Permission** | None required | None required |
| **User Action** | Automatic | Tap notification |
| **Implementation** | Complex (BroadcastReceiver) | Simple (native autofill) |
| **Security** | High (app hash verification) | High (system-level) |
| **Format** | Requires app hash | Standard OTP format |

---

**iOS OTP autofill provides the best user experience with minimal implementation complexity.**
