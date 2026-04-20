#if ANDROID

using Android.Content;

namespace MauiOtpKit.Platforms.Android;

/// <summary>
/// Helper class to generate app hash for SMS Retriever API
/// 
/// The app hash is a 11-character hash that uniquely identifies your app.
/// Google Play Services includes this hash in incoming SMS messages,
/// ensuring that only your app receives the OTP.
/// 
/// Usage:
///   string appHash = AppHashHelper.GetAppHash(context);
///   
/// The generated hash should match the one you provide to your backend service.
/// </summary>
internal static class AppHashHelper
{
    /// <summary>
    /// Generates the app hash for SMS Retriever API
    /// </summary>
    /// <param name="context">Android context</param>
    /// <returns>11-character app hash</returns>
    public static string GetAppHash(Context context)
    {
        try
        {
            var packageName = context.PackageName
                ?? throw new InvalidOperationException("Package name is unavailable");
            var certificateHash = GetSignatureHash(context, packageName);
            
            // Combine package name and certificate hash
            var hashInput = $"{packageName} {certificateHash}";
            var hash = ComputeHash(hashInput);
            
            // Return first 11 characters of Base64 encoded hash
            return Convert.ToBase64String(hash).Substring(0, 11);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to compute app hash", ex);
        }
    }

    private static byte[] GetSignatureHash(Context context, string packageName)
    {
        var packageManager = context.PackageManager;
        
#pragma warning disable CS0618 // Type or member is obsolete
        var packageInfo = packageManager?.GetPackageInfo(packageName,
            global::Android.Content.PM.PackageInfoFlags.Signatures);
#pragma warning restore CS0618

        if (packageInfo?.Signatures == null || packageInfo.Signatures.Count == 0)
            throw new InvalidOperationException("No signatures found");

        // Use the first signature
        var signature = packageInfo.Signatures[0];
        var publicKey = signature.ToByteArray()
            ?? throw new InvalidOperationException("Signature public key is unavailable");

        return ComputeHash(publicKey);
    }

    private static byte[] ComputeHash(string input)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        }
    }

    private static byte[] ComputeHash(byte[] input)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            return sha256.ComputeHash(input);
        }
    }
}

#endif
