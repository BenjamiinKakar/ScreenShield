# Error Handling & Logging Standards

## 1. The "Result" Pattern (Infrastructure & Core)
Do not throw exceptions for expected failures (e.g., "Hook failed to set", "Monitor detached"). 
Use a `Result<T>` wrapper class instead.

**Rule:**
- If a native call fails, return `Result.Failure("Error Message")`.
- If it succeeds, return `Result.Success(data)`.

**Example:**
```csharp
public Result<bool> ActivateHook() {
    try {
        // ... native code ...
        return Result<bool>.Success(true);
    } catch (Exception ex) {
        _logger.LogError(ex, "Failed to activate hook");
        return Result<bool>.Failure("Could not lock mouse input.");
    }
}