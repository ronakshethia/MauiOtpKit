using MauiOtpKit.Extensions;
using Microsoft.Extensions.Logging;

namespace MauiOtpKit.Sample;

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

        // Register MauiOtpKit services
        builder.Services.AddMauiOtp(options =>
        {
            options.Length = 6;
            options.ExpirySeconds = 120;
            options.MaxAttempts = 3;
            options.AutoStart = true;
        });

        // Add logging
        builder.Logging.AddDebug();

        return builder.Build();
    }
}
