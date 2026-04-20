# Changelog

All notable changes to MauiOtpKit will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- OTP resend mechanism with rate limiting
- Biometric confirmation for OTP validation
- Hardware security module (HSM) integration
- Adaptive OTP length based on security requirements

---

## [1.0.0] - 2026-04-20

### Added

#### Core Library (MauiOtpKit.Core)
- `IOtpService` - Main orchestration service
- `IOtpReader` - Platform abstraction for SMS/OTP retrieval
- `IOtpParser` - Regex-based OTP extraction
- `IOtpValidator` - OTP validation with expiry/attempt checking
- `IOtpTimer` - Countdown timer with event callbacks
- `OtpParser` - Production-grade parser supporting 4-6 digit codes
- `OtpValidator` - Validation logic with configurable rules
- `OtpTimer` - 1-second interval countdown timer
- `OtpService` - Thread-safe orchestration service
- `OtpOptions` - Configuration options
- `OtpResult` - Validation result with detailed feedback
- `OtpState` enum - Service state tracking
- Multi-targeting support: .NET 8.0, 9.0, 10.0

#### MAUI Platform (MauiOtpKit)
- `AndroidSmsReader` - SMS Retriever API integration
- `SmsRetrievalClient` - SMS retrieval task management
- `SmsBroadcastReceiver` - BroadcastReceiver for SMS events
- `SmsRetrievalTask` - Wrapper for SMS Retriever API task
- `AppHashHelper` - App hash generation for SMS Retriever
- `IosOtpReader` - iOS autofill support
- `NullOtpReader` - Fallback for unsupported platforms
- `MauiOtpServiceCollectionExtensions` - DI setup
- Conditional compilation for Android/iOS platform code
- Multi-targeting: net8.0-android, net8.0-ios, net9.0-android, net9.0-ios, net10.0-android, net10.0-ios

#### Features
- Automatic OTP detection on SMS receive (Android)
- OTP autofill support (iOS)
- Regex-based OTP parsing with configurable length
- OTP expiry enforcement with countdown timer
- Maximum attempt limiting to prevent brute force
- Thread-safe operations with lock-based synchronization
- Event-driven architecture (OtpDetected, ValidationError, OtpExpired, MaxAttemptsExceeded)
- Full ILogger integration for diagnostic logging
- Dependency Injection support via Microsoft.Extensions.DependencyInjection
- State management (Idle → Listening → Received → Validated)
- Configurable options (length, expiry, max attempts, timeout)

#### Sample Application (MauiOtpKit.Sample)
- Complete MAUI app demonstrating OTP functionality
- Cross-platform UI with XAML
- Event handling and state management
- Multi-targeting for Android and iOS

#### Documentation
- Comprehensive README with quick start guide
- Architecture guide with detailed component descriptions
- Android setup guide with app hash generation
- iOS setup guide with autofill configuration
- API reference documentation
- Usage examples and best practices
- Troubleshooting guides

### Security

#### Android
- No READ_SMS permission required
- App hash verification ensures app-specific SMS delivery
- Google Play Services encryption
- Automatic SMS cleanup after retrieval

#### iOS
- No direct SMS access (iOS privacy by design)
- User-controlled via autofill
- Secure notification handling

#### General
- Expiry enforcement to prevent replay attacks
- Attempt limiting to prevent brute force
- Thread-safe state management
- Proper error handling and logging

### Performance
- Minimal memory footprint (~2 KB per service instance)
- 1-second interval timer for expiry tracking
- Non-blocking async/await operations
- Lock-free reader/validator for minimal contention

### Testing & Quality
- Designed for unit testing with mock implementations
- Separated concerns for easy testing of individual components
- Comprehensive error messages for debugging
- Full logging via ILogger integration

---

## Installation

```bash
# Install core package
dotnet add package MauiOtpKit.Core

# Install MAUI platform package
dotnet add package MauiOtpKit
```

---

## Migration Guide

### From Version 0.x (if applicable)

N/A - First stable release

---

## Known Issues

- iOS Simulator doesn't support SMS retrieval (use real device for testing)
- SMS Retriever on Android requires app to be installed via Google Play Store
- Sideloaded APKs may not work with SMS Retriever API

---

## Future Roadmap

### 2.0.0 (Planned)
- [ ] OTP resend mechanism
- [ ] Rate limiting utilities
- [ ] Biometric confirmation
- [ ] Analytics/telemetry support
- [ ] Offline mode support

### 3.0.0 (Planned)
- [ ] Alternative OTP providers (Email, WhatsApp, Telegram)
- [ ] Hardware security module integration
- [ ] Advanced fraud detection
- [ ] Machine learning-based anomaly detection

---

## Contributors

- MauiOtpKit Team

---

## License

MIT License - See LICENSE file for details

---

## Support

- **Issues**: Report bugs on GitHub
- **Discussions**: Join community discussions
- **Docs**: See [documentation](./docs/)
- **Email**: support@mauiotpkit.dev

---

## Deprecations

None yet.

---

**For detailed information on releases, please visit the [GitHub Releases](https://github.com/yourusername/MauiOtpKit/releases) page.**
