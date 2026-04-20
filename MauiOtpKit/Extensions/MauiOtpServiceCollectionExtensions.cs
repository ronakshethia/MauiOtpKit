using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MauiOtpKit.Core.Contracts;
using MauiOtpKit.Core.Implementations;
using MauiOtpKit.Core.Models;
using MauiOtpKit.Platforms;

#if ANDROID
using MauiOtpKit.Platforms.Android;
#endif

#if IOS
using MauiOtpKit.Platforms.iOS;
#endif

namespace MauiOtpKit.Extensions;

/// <summary>
/// Dependency Injection extension for MauiOtpKit
/// </summary>
public static class MauiOtpServiceCollectionExtensions
{
    /// <summary>
    /// Adds MauiOtpKit services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMauiOtp(
        this IServiceCollection services,
        Action<OtpOptions>? configure = null)
    {
        // Create and configure OtpOptions
        var options = new OtpOptions();
        configure?.Invoke(options);

        // Register options as singleton
        services.AddSingleton(options);

        // Register core implementations
        services.AddSingleton<IOtpParser>(sp => new OtpParser(options.Length));
        services.AddSingleton<IOtpValidator>(sp => new OtpValidator(options));
        services.AddSingleton<IOtpTimer, OtpTimer>();

        // Register platform-specific reader
        services.AddSingleton<IOtpReader>(CreatePlatformOtpReader);

        // Register main OTP service
        services.AddSingleton<IOtpService, OtpService>();

        return services;
    }

    /// <summary>
    /// Creates the platform-specific OTP reader
    /// </summary>
    private static IOtpReader CreatePlatformOtpReader(IServiceProvider sp)
    {
        var loggerFactory = sp.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("MauiOtpKit");

#if ANDROID
        logger?.LogInformation("Initializing Android SMS Retriever");
        return new AndroidSmsReader(
            loggerFactory?.CreateLogger<AndroidSmsReader>()
        );

#elif IOS
        logger?.LogInformation("Initializing iOS OTP Reader");
        return new IosOtpReader(
            loggerFactory?.CreateLogger<IosOtpReader>()
        );

#else
        logger?.LogWarning("Platform not supported, using null reader");
        return new NullOtpReader(logger);
#endif
    }
}
