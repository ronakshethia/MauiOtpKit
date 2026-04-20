# MauiOtpKit - Quick Start

Get up and running with MauiOtpKit in under 5 minutes.

## 1. Install NuGet Packages

```bash
dotnet add package MauiOtpKit
dotnet add package MauiOtpKit.Core
```

## 2. Register in MauiProgram

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
            options.Length = 6;
            options.ExpirySeconds = 120;
            options.MaxAttempts = 3;
        });

        return builder.Build();
    }
}
```

## 3. Create UI

```xml
<Entry
    x:Name="OtpEntry"
    Placeholder="000000"
    Keyboard="Numeric"
    MaxLength="6" />

<Button Text="Validate" Clicked="OnValidateClicked" />
```

## 4. Use in Code

```csharp
private readonly IOtpService _otpService;

public YourPage()
{
    InitializeComponent();
    
    _otpService = IPlatformApplication.Current?.Services
        .GetRequiredService<IOtpService>();
    
    _otpService.OtpDetected += (s, otp) => OtpEntry.Text = otp;
}

protected override async void OnAppearing()
{
    await _otpService.StartAsync();
}

private async void OnValidateClicked(object sender, EventArgs e)
{
    var result = await _otpService.ValidateAsync(OtpEntry.Text);
    if (result.IsSuccess)
        await DisplayAlert("Success", "OTP Validated!", "OK");
}
```

## 5. Platform Setup

### Android
- Get app hash: `AppHashHelper.GetAppHash(context)`
- Send SMS with format: `<#> Your OTP is 123456 {APP_HASH}`

### iOS
- Standard SMS format: `Your OTP is 123456`
- Autofill handles the rest!

## Next Steps

- See [README.md](./README.md) for full documentation
- Check [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) for design details
- Review [docs/ANDROID_SETUP.md](./docs/ANDROID_SETUP.md) for Android configuration
- Review [docs/iOS_SETUP.md](./docs/iOS_SETUP.md) for iOS configuration
- Explore [samples/MauiOtpKit.Sample](./samples/MauiOtpKit.Sample) for complete example

## Common Issues

**iOS Simulator doesn't work?** → Use real device for testing

**Android SMS not detected?** → Verify app hash in backend SMS

**OTP validation failing?** → Check expiry time and code matching

---

**Ready to secure your app? Start with the quick start above!**
