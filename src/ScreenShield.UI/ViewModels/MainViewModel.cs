using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenShield.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenShield.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IInputService _inputService;
    private readonly IMonitorService _monitorService;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleProtectionButtonText))]
    private bool _isActive;

    [ObservableProperty]
    private string _statusText;

    public string ToggleProtectionButtonText => IsActive ? "Stop Protection" : "Start Protection";

    public MainViewModel(IInputService inputService, IMonitorService monitorService, IWindowService windowService)
    {
        _inputService = inputService;
        _monitorService = monitorService;
        _windowService = windowService;

        StatusText = "Ready.";
        IsActive = false;
    }

    [RelayCommand]
    private async Task ToggleProtectionAsync()
    {
        IsActive = !IsActive;

        if (IsActive)
        {
            StatusText = "Starting protection...";
            _inputService.StartHook();
            var monitorResult = await _monitorService.GetMonitorsAsync();
            if (monitorResult.IsSuccess)
            {
                var secondaryMonitors = monitorResult.Value.Where(m => !m.IsPrimary).ToList();
                StatusText = $"Found {secondaryMonitors.Count} secondary monitor(s). Applying overlays...";
                foreach (var monitor in secondaryMonitors)
                {
                    await _windowService.ShowOverlayAsync(monitor);
                }
                StatusText = $"Protection enabled on {secondaryMonitors.Count} monitor(s).";
            }
            else
            {
                StatusText = $"Error: {monitorResult.Error}";
                IsActive = false; // Revert state on failure
            }
        }
        else
        {
            StatusText = "Stopping protection...";
            _inputService.StopHook();
            var monitorResult = await _monitorService.GetMonitorsAsync(); // Re-fetch to be safe
            if (monitorResult.IsSuccess)
            {
                foreach (var monitor in monitorResult.Value)
                {
                    await _windowService.HideOverlayAsync(monitor);
                }
            }
            StatusText = "Ready.";
        }
    }
}
