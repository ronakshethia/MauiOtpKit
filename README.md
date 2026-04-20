# MauiOtpKit 🔐

**Production-grade, cross-platform OTP solution for .NET MAUI**

![License](https://img.shields.io/badge/license-MIT-green)
![NuGet](https://img.shields.io/badge/NuGet-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)

## 🎯 Overview

**MauiOtpKit** provides a scalable, reusable OTP (One-Time Password) module for .NET MAUI applications. It handles:

- ✅ **Android SMS auto-read** via Google Play Services SMS Retriever API
- ✅ **iOS OTP autofill** support
- ✅ **Robust OTP parsing** with regex-based extraction
- ✅ **Validation logic** with expiry and retry limits
- ✅ **Clean architecture** with full DI support
- ✅ **Multi-targeting** for .NET 8, 9, and 10
- ✅ **Thread-safe** operations with proper async/await
- ✅ **Extensible design** for custom providers (Email, WhatsApp, etc.)

## 📦 Features

### Core Features

| Feature | Details |
|---------|---------|
| **OTP Parsing** | Regex-based extraction of 4-6 digit codes from any text |
| **Validation** | Match against expected code, enforce expiry and max attempts |
| **Timer** | Countdown timer with observable events for UI updates |
| **State Management** | Track OTP lifecycle (Idle → Listening → Received → Validated) |
| **Events** | OTP detected, validation errors, expiry, max attempts exceeded |
| **Logging** | Full logging abstraction via Microsoft.Extensions.Logging |

### Platform Support

#### Android
- **SMS Retriever API** (Google Play Services)
- **Zero permissions required** - No READ_SMS permission
- **App-specific delivery** - App hash ensures secure delivery
- **Automatic detection** - Works in background

#### iOS
- **OTP Autofill** - UITextContentType.OneTimeCode support
- **Privacy-first** - No SMS access (by design)
- **Clipboard detection** - Optional pasteboard integration
- **User-driven** - Notification or manual entry

## 🚀 Quick Start

### Installation

Install from NuGet:

```bash
dotnet add package MauiOtpKit
dotnet add package MauiOtpKit.Core
```

### Configuration

In your `MauiProgram.cs`:

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

        // Add MauiOtpKit
        builder.Services.AddMauiOtp(options =>
        {
            options.Length = 6;                  // OTP length
            options.ExpirySeconds = 120;         // 2 minutes
            options.MaxAttempts = 3;             // Max validation attempts
            options.AutoStart = true;            // Start on initialization
        });

        return builder.Build();
    }
}
```

### Basic Usage

```csharp
// In your page/view model
private readonly IOtpService _otpService;

public YourPage()
{
    _otpService = IPlatformApplication.Current?.Services
        .GetRequiredService<IOtpService>();
    
    // Subscribe to events
    _otpService.OtpDetected += OnOtpDetected;
    _otpService.ValidationError += OnValidationError;
}

protected override async void OnAppearing()
{
    // Start OTP listening
    await _otpService.StartAsync();
}

private void OnOtpDetected(object? sender, string otp)
{
    // OTP automatically detected
    OtpEntry.Text = otp;
}

private async void ValidateOtp()
{
    var result = await _otpService.ValidateAsync(userInput);
    
    if (result.IsSuccess)
    {
        await DisplayAlert("Success", "OTP validated!", "OK");
    }
    else
    {
        await DisplayAlert("Error", result.Message, "OK");
    }
}
```

---

## � Documentation & Guides

For detailed setup and implementation guides, see:

- **[QUICKSTART.md](QUICKSTART.md)** - Get running in 5 minutes
- **[docs/ANDROID_SETUP.md](docs/ANDROID_SETUP.md)** - Android SMS Retriever configuration (app hash, backend setup, testing)
- **[docs/iOS_SETUP.md](docs/iOS_SETUP.md)** - iOS OTP autofill setup
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** - Technical architecture & design patterns- **[docs/NUGET_PUBLISHING.md](docs/NUGET_PUBLISHING.md)** - Publish packages to NuGet.org (GitHub Actions workflow)- **[CHANGELOG.md](CHANGELOG.md)** - Feature list & version history

---

## �🔧 Android Setup

### Step 1: Enable Google Play Services

Your app must be published on Google Play Store or use Firebase console for testing.

### Step 2: Get App Hash

MauiOtpKit provides a helper to generate your app hash:

```csharp
#if ANDROID
using MauiOtpKit.Platforms.Android;

var context = Android.App.Application.Context;
string appHash = AppHashHelper.GetAppHash(context);
Debug.WriteLine($"App Hash: {appHash}");
#endif
```

**What is App Hash?**
- 11-character unique identifier for your app
- Generated from app's package name + signing certificate
- Included in SMS messages received by SMS Retriever API
- Ensures **only your app** receives the OTP

### Step 3: Configure Backend

Provide the app hash to your backend service. Backend should send SMS in format:

```
<#> Your OTP is 123456 ABC+D/EF+GHI
```

Where:
- `<#>` - Required prefix
- `123456` - Your OTP code
- `ABC+D/EF+GHI` - Your app hash (11 chars, Base64 URL-safe)

### Step 4: AndroidManifest.xml

No special permissions required! SMS Retriever API handles everything.

---

## 🍎 iOS Setup

### Step 1: Enable OTP Autofill

In your XAML Entry field:

```xml
<Entry
    x:Name="OtpEntry"
    Placeholder="000000"
    Keyboard="Numeric"
    MaxLength="6" />
```

MauiOtpKit automatically configures `UITextContentType.OneTimeCode` (equivalent in MAUI).

### Step 2: Info.plist Configuration

No additional configuration needed. iOS handles OTP autofill natively.

### Step 3: SMS Format

iOS expects SMS in standard format:

```
Your OTP is 123456
```

iOS will automatically detect and offer to autofill.

---

## 📚 API Reference

### IOtpService

Main service orchestrating OTP flow:

```csharp
public interface IOtpService
{
    // Start/stop OTP listening
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();

    // Validate user input
    Task<OtpResult> ValidateAsync(string userInput);

    // Reset state
    Task ResetAsync();

    // Get current state
    OtpState GetState();

    // Events
    event EventHandler<string>? OtpDetected;
    event EventHandler<string>? ValidationError;
    event EventHandler? OtpExpired;
    event EventHandler? MaxAttemptsExceeded;
}
```

### OtpResult

Validation result:

```csharp
public class OtpResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public int AttemptCount { get; set; }
    public int RemainingSeconds { get; set; }
}
```

### OtpOptions

Configuration:

```csharp
public class OtpOptions
{
    public int Length { get; set; } = 6;
    public int ExpirySeconds { get; set; } = 120;
    public int MaxAttempts { get; set; } = 3;
    public bool AutoStart { get; set; } = true;
    public int TimeoutMilliseconds { get; set; } = 300000;
}
```

### OtpState

```csharp
public enum OtpState
{
    Idle,           // Not listening
    Listening,      // Waiting for OTP
    Received,       // OTP received but not validated
    Validated,      // Successfully validated
    Expired,        // OTP expired
    Failed          // Validation failed
}
```

---

## 🏗️ Architecture

### Project Structure

```
MauiOtpKit/
├── MauiOtpKit.Core/          # Platform-agnostic business logic
│   ├── Contracts/            # Interfaces
│   ├── Models/               # OtpOptions, OtpResult
│   └── Implementations/      # Parser, Validator, Service, Timer
│
├── MauiOtpKit/               # MAUI platform implementations
│   ├── Extensions/           # DI setup
│   ├── Platforms/
│   │   ├── Android/          # SMS Retriever API
│   │   ├── iOS/              # OTP Autofill
│   │   └── NullOtpReader.cs # Fallback
│   └── MauiOtpKit.csproj    # Multi-target project
│
└── samples/
    └── MauiOtpKit.Sample/    # Example MAUI app
```

### Core Components

#### 1. **IOtpReader** (Platform-specific)
- `AndroidSmsReader` - SMS Retriever API integration
- `IosOtpReader` - Autofill support
- `NullOtpReader` - Fallback

#### 2. **IOtpParser**
- Regex-based extraction
- Supports various SMS formats
- Configurable OTP length

#### 3. **IOtpValidator**
- Expiry checking
- Attempt limiting
- Code matching

#### 4. **IOtpTimer**
- Countdown timer
- Per-second updates
- Expiry notifications

#### 5. **IOtpService**
- Orchestrates full flow
- Thread-safe state management
- Event-driven architecture

---

## 💡 Usage Examples

### Example 1: Basic OTP Validation

```csharp
// Subscribe to auto-detection
_otpService.OtpDetected += (sender, otp) =>
{
    OtpEntry.Text = otp;
};

// Start listening
await _otpService.StartAsync();

// User enters OTP (or it's auto-filled)
var result = await _otpService.ValidateAsync(userInput);
if (result.IsSuccess)
{
    // Success!
}
```

### Example 2: Retry Logic

```csharp
int maxRetries = 3;
int retryCount = 0;

while (retryCount < maxRetries)
{
    var result = await _otpService.ValidateAsync(userInput);
    
    if (result.IsSuccess)
        break;
    
    retryCount++;
    if (retryCount < maxRetries)
        await _otpService.ResetAsync();
}
```

### Example 3: Monitoring Expiry

```csharp
_otpService.OtpExpired += async (sender, e) =>
{
    await DisplayAlert("OTP Expired", 
        "Please request a new OTP code", "OK");
    
    // Request new OTP from backend
    await RequestNewOtp();
};
```

### Example 4: Custom Configuration

```csharp
builder.Services.AddMauiOtp(options =>
{
    options.Length = 4;              // Short OTP for tests
    options.ExpirySeconds = 300;     // 5 minutes
    options.MaxAttempts = 5;         // More attempts
    options.AutoStart = false;       // Manual control
});
```

---

## 🔒 Security Considerations

### Android
- ✅ **No READ_SMS permission** required
- ✅ **App hash verification** ensures app-specific delivery
- ✅ **Google Play Services** handles encryption
- ✅ **Automatic cleanup** after retrieval

### iOS
- ✅ **No SMS access** (by iOS design)
- ✅ **User-controlled** via autofill
- ✅ **Secure pasteboard** handling
- ✅ **Notification-based** delivery

### General
- ✅ **Expiry enforcement** prevents replay attacks
- ✅ **Attempt limiting** prevents brute force
- ✅ **State validation** ensures correct flow
- ✅ **Thread-safe** operations

---

## 🧪 Testing

### Unit Testing

```csharp
[TestClass]
public class OtpParserTests
{
    private IOtpParser _parser;

    [TestInitialize]
    public void Setup()
    {
        _parser = new OtpParser(6);
    }

    [TestMethod]
    public void Parse_ValidSms_ReturnsOtp()
    {
        var sms = "Your OTP is 123456";
        var result = _parser.Parse(sms);
        Assert.AreEqual("123456", result);
    }
}
```

### Mock Platform Readers

```csharp
public class MockOtpReader : IOtpReader
{
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(500);
        OtpReceived?.Invoke(this, "123456");
    }

    public Task StopAsync() => Task.CompletedTask;

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;
}
```

---

## 📋 Best Practices

### 1. **Always Start/Stop**
```csharp
protected override async void OnAppearing()
{
    await _otpService.StartAsync();
}

protected override async void OnDisappearing()
{
    await _otpService.StopAsync();
}
```

### 2. **Handle All Events**
```csharp
_otpService.OtpDetected += OnOtpDetected;
_otpService.ValidationError += OnValidationError;
_otpService.OtpExpired += OnOtpExpired;
_otpService.MaxAttemptsExceeded += OnMaxAttemptsExceeded;
```

### 3. **Configure Appropriately**
```csharp
builder.Services.AddMauiOtp(options =>
{
    options.ExpirySeconds = 120;    // Don't make too long
    options.MaxAttempts = 3;        // Prevent brute force
    options.Length = 6;             // Standard length
});
```

### 4. **Log Errors**
```csharp
builder.Logging.AddDebug();
// MauiOtpKit logs all operations
```

### 5. **Reset on Failure**
```csharp
if (!result.IsSuccess)
{
    await _otpService.ResetAsync();
    // Optionally request new OTP
}
```

---

## ❓ FAQ

### Q: Why no READ_SMS permission on Android?
**A:** SMS Retriever API uses Google Play Services which handles SMS securely without exposing permission-requiring access. Google Play handles permission delegation internally.

### Q: Can I use this for WhatsApp OTP?
**A:** Yes! Create a custom `IOtpReader` implementation for WhatsApp notification parsing.

### Q: How do I get the app hash?
**A:** Use the provided `AppHashHelper.GetAppHash(context)` method. See Android Setup section.

### Q: Does iOS allow SMS reading?
**A:** No. iOS restricts SMS access by design. Use OTP autofill (recommended) or ask users to paste from notification.

### Q: Can I customize OTP parsing?
**A:** Yes! Implement `IOtpParser` with your own regex pattern.

### Q: Is it thread-safe?
**A:** Yes! All operations use proper locking and are fully thread-safe.

### Q: What's the minimum .NET version?
**A:** .NET 8.0 (Core) and .NET 8.0-android/ios (MAUI)

### Q: How do I extend it?
**A:** Create custom implementations of `IOtpReader`, `IOtpParser`, `IOtpValidator`, or register different ones in DI.

---

## � Documentation

Complete documentation is available in the docs folder:

| Document | Purpose |
|----------|---------|
| [QUICKSTART.md](QUICKSTART.md) | Get started in 5 minutes |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Technical deep dive into design & components |
| [docs/ANDROID_SETUP.md](docs/ANDROID_SETUP.md) | Android SMS Retriever API setup guide |
| [docs/iOS_SETUP.md](docs/iOS_SETUP.md) | iOS OTP autofill configuration guide |
| [CHANGELOG.md](CHANGELOG.md) | Version history and feature list |
| [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) | Complete implementation details |

**Recommended Reading Order**:
1. Start with [QUICKSTART.md](QUICKSTART.md) for basic setup
2. Read platform-specific guides:
   - [docs/ANDROID_SETUP.md](docs/ANDROID_SETUP.md) for Android implementation
   - [docs/iOS_SETUP.md](docs/iOS_SETUP.md) for iOS implementation
3. Review [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for technical details
4. Check [CHANGELOG.md](CHANGELOG.md) for version information

---

## �📄 License

MIT License - See LICENSE file for details

---

--

## 🎓 Advanced Topics

### Custom OTP Provider

```csharp
public class EmailOtpReader : IOtpReader
{
    private readonly IEmailService _emailService;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var otp = await _emailService.GetOtpFromInboxAsync();
        OtpReceived?.Invoke(this, otp);
    }

    public event EventHandler<string>? OtpReceived;
    public event EventHandler<string>? ErrorOccurred;
}

// Register
services.AddSingleton<IOtpReader, EmailOtpReader>();
```

### Conditional DI Registration

```csharp
services.AddMauiOtp(options =>
{
    #if DEBUG
    options.ExpirySeconds = 3600; // 1 hour for testing
    #else
    options.ExpirySeconds = 120;  // 2 minutes for production
    #endif
});
```

### Retry with Exponential Backoff

```csharp
var retryPolicy = Policy
    .Handle<InvalidOperationException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var result = await retryPolicy.ExecuteAsync(() =>
    _otpService.ValidateAsync(userInput));
```

---