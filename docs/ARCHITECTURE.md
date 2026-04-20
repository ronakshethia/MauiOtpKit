# Architecture Guide - MauiOtpKit

## Overview

MauiOtpKit is built on a **layered architecture** with clean separation of concerns:

```
┌─────────────────────────────────────────┐
│        User Application Layer           │
│    (MAUI Pages, ViewModels, UI)         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Dependency Injection Layer         │
│   (Service Registration & Resolution)   │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│        Business Logic Layer             │
│    (IOtpService - Orchestration)        │
└────────────┬─────────────┬──────────────┘
             │             │
    ┌────────▼──┐  ┌──────▼────────┐
    │  Parser   │  │   Validator   │
    │  Timer    │  │   Reader      │
    └───────────┘  └───────────────┘
             │             │
┌────────────▼─────────────▼──────────────┐
│     Platform Abstraction Layer          │
│  (Interfaces: IOtpReader, IOtpParser)   │
└────────────┬─────────────┬──────────────┘
             │             │
    ┌────────▼──┐  ┌──────▼────────┐
    │  Android  │  │      iOS      │
    │ SMS API   │  │   Autofill    │
    └───────────┘  └───────────────┘
             │             │
    ┌────────▼─────────────▼────────┐
    │   Platform-Specific Code      │
    │ (Native Android/iOS bindings) │
    └───────────────────────────────┘
```

---

## Core Components

### 1. **IOtpService** (Orchestrator)

**Responsibility**: Coordinates the entire OTP flow

**Workflow**:
```
User initiates validation
        ↓
OtpService.StartAsync()
        ↓
Reader.StartAsync() + Timer.Start()
        ↓
[WAITING FOR SMS/AUTOFILL]
        ↓
Reader.OtpReceived event fires
        ↓
OtpService receives raw SMS
        ↓
Parser.Parse(sms) extracts code
        ↓
Service stores code + timestamp
        ↓
User enters/receives OTP
        ↓
OtpService.ValidateAsync(input)
        ↓
Validator checks: expiry, attempts, match
        ↓
Returns OtpResult (success/failure)
```

**Key Features**:
- Thread-safe using locks
- State management (Idle → Listening → Received → Validated)
- Event-driven architecture
- Full logging integration

### 2. **IOtpParser** (Text Extraction)

**Responsibility**: Extract OTP codes from unstructured text

**Algorithm**:
```
Input: "Your OTP is 123456 ABC+DEF+GHI"
            ↓
Regex Pattern: \b\d{4,6}\b
            ↓
Find all 4-6 digit sequences
            ↓
Match against configured length (default: 6)
            ↓
Return: "123456"
```

**Supported Patterns**:
- `Your OTP is 123456` ✓
- `123456 is your code` ✓
- `Code: 1234` ✓
- `OTP-123456` ✓

### 3. **IOtpValidator** (Verification)

**Responsibility**: Validate OTP against rules

**Checks**:
```
Input: "123456"
Expected: "123456"
Generated: DateTime.UtcNow - 30 seconds

┌─ Not Empty? ────────────────────────────┐
│  ✓ "123456" is not empty                 │
└──────────────────────────────────────────┘
         ↓
┌─ Code Generated? ──────────────────────┐
│  ✓ Code exists in service               │
└──────────────────────────────────────────┘
         ↓
┌─ Not Expired? (ExpirySeconds = 120) ──┐
│  Elapsed: 30 seconds                    │
│  Remaining: 90 seconds                  │
│  ✓ 30 < 120                             │
└──────────────────────────────────────────┘
         ↓
┌─ Not Max Attempts? (MaxAttempts = 3) ─┐
│  Attempts: 1                            │
│  ✓ 1 < 3                                │
└──────────────────────────────────────────┘
         ↓
┌─ Code Matches? ────────────────────────┐
│  Input: "123456"                        │
│  Expected: "123456"                     │
│  ✓ StringComparison.Ordinal             │
└──────────────────────────────────────────┘
         ↓
Result: SUCCESS ✓
```

### 4. **IOtpReader** (Platform-Specific)

**Android Implementation**:
```
AndroidSmsReader
    ↓
Uses: SmsRetriever.GetClient()
    ↓
Starts: StartSmsUserConsent()
    ↓
Listens: BroadcastReceiver (intent action)
    ↓
Receives: "com.google.android.gms.auth.api.phone.SMS_RETRIEVED"
    ↓
Extracts: SMS message from intent extras
    ↓
Fires: OtpReceived event with raw SMS
```

**iOS Implementation**:
```
IosOtpReader
    ↓
Passive: Awaits UITextContentType.OneTimeCode
    ↓
Alternative: DetectOtpFromPasteboard(otp)
    ↓
User action: Confirms autofill in notification
    ↓
Fires: OtpReceived event
```

### 5. **IOtpTimer** (Countdown)

**Responsibility**: Track OTP validity duration

**Implementation**:
```
Start(durationSeconds: 120)
    ↓
Initialize: _remainingSeconds = 120
    ↓
Timer fires every 1000ms
    ↓
Decrement: _remainingSeconds--
    ↓
Fire: TimerElapsed event (120, 119, 118, ...)
    ↓
When _remainingSeconds = 0:
    ├─ Stop timer
    └─ Fire: TimerExpired event
    
OtpService listens → Sets state to Expired
```

---

## Data Flow

### Happy Path: Android SMS Auto-Read

```
1. User opens login page
   ├─ MauiProgram registers MauiOtpKit
   ├─ DI injects AndroidSmsReader + OtpService
   └─ Page subscribes to OtpService events

2. Page appears
   └─ OnAppearing() → otpService.StartAsync()
       ├─ reader.StartAsync()
       │   └─ SmsRetrievalClient.Start()
       │       └─ SmsRetriever.GetClient().StartSmsUserConsent()
       └─ timer.Start(120)
           └─ 1-second interval timer begins

3. SMS arrives from server
   └─ SMS Retriever broadcasts SMS_RETRIEVED intent
       └─ SmsBroadcastReceiver.OnReceive()
           └─ Extracts "Your OTP is 123456"
               └─ FireEvent: OtpReceived("Your OTP is 123456")

4. OtpService processes
   └─ _reader.OtpReceived event handler
       ├─ parser.Parse(sms) → "123456"
       ├─ Store: _generatedCode = "123456"
       ├─ Store: _generatedAt = DateTime.UtcNow
       └─ Fire: OtpDetected("123456")

5. UI updates
   └─ MainPage.OnOtpDetected()
       ├─ OtpEntry.Text = "123456"
       └─ Display: "✓ OTP Detected!"

6. User taps Validate (or auto-submits)
   └─ otpService.ValidateAsync("123456")
       ├─ validator.Validate()
       │   ├─ Check: Not empty ✓
       │   ├─ Check: Not expired ✓
       │   ├─ Check: Not max attempts ✓
       │   ├─ Check: Code matches ✓
       │   └─ Return: OtpResult.Success()
       └─ Fire: ResultSuccess event

7. Cleanup
   └─ Page disappears
       └─ OnDisappearing() → otpService.StopAsync()
           ├─ reader.StopAsync()
           │   └─ UnregisterReceiver()
           └─ timer.Stop()
```

---

## Thread Safety

MauiOtpKit uses **lock-based synchronization** for thread safety:

```csharp
// OtpService internal state protected by lock
private readonly object _lockObject = new();

public async Task<OtpResult> ValidateAsync(string userInput)
{
    lock (_lockObject)  // ← Enter critical section
    {
        // All state mutations here are atomic
        if (string.IsNullOrEmpty(_generatedCode))
            return OtpResult.Failure("No OTP");
        
        _attemptCount++;
        
        if (_validator.IsMaxAttemptsExceeded(_attemptCount))
            return OtpResult.Failure("Max attempts");
        
        var result = _validator.Validate(userInput, _generatedCode, _generatedAt, _attemptCount);
        
        if (result.IsSuccess)
            _state = OtpState.Validated;
        
        return result;
    }  // ← Exit critical section
}
```

**Protected Operations**:
- `_generatedCode` assignment
- `_attemptCount` increment
- `_state` transitions
- Reading state for validation

---

## Dependency Injection Workflow

```csharp
// MauiProgram.cs
builder.Services.AddMauiOtp(options => { ... });

// Internally, extension registers:
services.AddSingleton(otpOptions);
services.AddSingleton<IOtpParser>(sp => new OtpParser(options.Length));
services.AddSingleton<IOtpValidator>(sp => new OtpValidator(options));
services.AddSingleton<IOtpTimer, OtpTimer>();
services.AddSingleton<IOtpReader>(CreatePlatformOtpReader);  // ← Platform-specific!
services.AddSingleton<IOtpService, OtpService>();

// CreatePlatformOtpReader()
#if ANDROID
return new AndroidSmsReader(...);
#elif IOS
return new IosOtpReader(...);
#else
return new NullOtpReader(...);
#endif

// Usage in page
public YourPage()
{
    var otpService = IPlatformApplication.Current?.Services
        .GetRequiredService<IOtpService>();
}
```

**Benefits**:
- ✓ Single responsibility for each component
- ✓ Easy to test with mocks
- ✓ Platform code isolated via conditional compilation
- ✓ Extensible for custom implementations

---

## Event Architecture

MauiOtpKit uses a **pub-sub event model**:

```
┌─────────────────────────────────────┐
│     OtpService (Publisher)          │
├─────────────────────────────────────┤
│ Events:                             │
│  • OtpDetected(string otp)          │
│  • ValidationError(string message)  │
│  • OtpExpired()                     │
│  • MaxAttemptsExceeded()            │
└──────┬────────────────────────────┬─┘
       │                            │
       │                            │
┌──────▼────────────┐      ┌────────▼─────────────┐
│   MainPage        │      │   ResultViewModel    │
├───────────────────┤      ├─────────────────────┤
│ OnOtpDetected()   │      │ UpdateUI()          │
│ OnValidationError()       │ HandleError()       │
│ OnOtpExpired()    │      │ CheckExpiry()       │
└───────────────────┘      └─────────────────────┘
```

---

## Performance Considerations

### Memory
- **OtpTimer**: ~1 KB per instance (1 System.Timers.Timer object)
- **OtpService**: ~2 KB per instance (string storage, DateTime)
- **Overall**: Minimal footprint suitable for all devices

### CPU
- **Timer firing**: 1000ms interval (configurable)
- **Event handling**: Synchronous callbacks
- **Lock contention**: Negligible (operations are fast)

### Network
- **Zero network overhead** - All operations local
- **SMS retrieval**: Handled by Google Play Services (Android)

---

## Extensibility Points

### Custom Parser
```csharp
public class CustomOtpParser : IOtpParser
{
    public string? Parse(string text) { /* ... */ }
    public bool Contains(string text) { /* ... */ }
}

services.AddSingleton<IOtpParser, CustomOtpParser>();
```

### Custom Reader
```csharp
public class EmailOtpReader : IOtpReader
{
    public async Task StartAsync(CancellationToken ct) { /* ... */ }
    public async Task StopAsync() { /* ... */ }
}

services.AddSingleton<IOtpReader, EmailOtpReader>();
```

### Custom Validator
```csharp
public class StrictOtpValidator : IOtpValidator
{
    // Custom validation logic
}

services.AddSingleton<IOtpValidator, StrictOtpValidator>();
```

---

## Error Handling Strategy

```
OtpService Error Handler
    │
    ├─ Reader Error
    │  └─ Log + Fire ValidationError event
    │
    ├─ Parser Error (N/A - no exceptions)
    │  └─ Silent (returns null)
    │
    ├─ Validator Error (N/A - no exceptions)
    │  └─ Returns OtpResult.Failure()
    │
    ├─ Timer Error
    │  └─ Log + Fire TimerExpired
    │
    └─ State Error
       └─ Return appropriate OtpResult
```

All errors are logged via `ILogger<T>`.

---

## Testing Strategy

### Unit Testing
```csharp
// Test parser
[TestMethod]
public void OtpParser_Extract6DigitCode() { }

// Test validator
[TestMethod]
public void OtpValidator_CheckExpiry() { }

// Test service logic
[TestMethod]
public void OtpService_ValidateValidCode() { }
```

### Integration Testing
```csharp
// With mock reader
[TestMethod]
public async Task OtpService_WithMockReader_FiresEvents() { }
```

### Platform Testing
```csharp
// Android: Test SMS broadcast
[TestMethod]
public void SmsBroadcastReceiver_ReceiveSMS() { }

// iOS: Test autofill integration
[TestMethod]
public void IosOtpReader_DetectPasteboard() { }
```

---

**This architecture ensures MauiOtpKit is robust, testable, and production-ready.**
