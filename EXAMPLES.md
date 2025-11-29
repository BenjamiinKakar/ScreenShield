# Code Patterns & Examples

## 1. The "Service" Pattern (Infrastructure Layer)
When implementing a native wrapper, always use a `SafeHandle` and Interface.

```csharp
// Interface (Core)
public interface IWindowService {
    void SetWindowBlur(IntPtr handle);
}

// Implementation (Infra)
public class WindowService : IWindowService {
    public void SetWindowBlur(IntPtr handle) {
        try {
            // Validate handle
            if (handle == IntPtr.Zero) throw new ArgumentException("Invalid Handle");
            
            // Native Call
            var data = new WindowCompositionAttributeData { ... };
            User32.SetWindowCompositionAttribute(handle, ref data);
        }
        catch (Exception ex) {
            // Log error, do not crash app
            Debug.WriteLine($"Failed to blur: {ex.Message}");
        }
    }
}

//2. The ViewModel Pattern (UI Layer)
//Use CommunityToolkit.Mvvm. Do not implement INotifyPropertyChanged manually.

public partial class MainWindowViewModel : ObservableObject {
    private readonly IWindowService _windowService;

    [ObservableProperty]
    private bool _isPrivacyActive;

    public MainWindowViewModel(IWindowService windowService) {
        _windowService = windowService;
    }

    [RelayCommand]
    private void TogglePrivacy() {
        IsPrivacyActive = !IsPrivacyActive;
        // Logic calls _windowService here...
    }
}