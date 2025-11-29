using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenShield.Core.Interfaces;
using ScreenShield.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenShield.UI.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IInputService _inputService;
    private readonly IMonitorService _monitorService;
    private readonly IWindowService _windowService;
    private readonly IdleDetector _idleDetector;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleProtectionButtonText))]
    private bool _isActive;

    [ObservableProperty]
    private string _statusText;

    public string ToggleProtectionButtonText => IsActive ? "Stop Protection" : "Start Protection";

    public MainViewModel(IInputService inputService, IMonitorService monitorService, IWindowService windowService, IdleDetector idleDetector)
    {
        _inputService = inputService;
        _monitorService = monitorService;
        _windowService = windowService;
        _idleDetector = idleDetector;

        _idleDetector.IdleDetected += OnIdleDetected;
        _idleDetector.ActivityDetected += OnActivityDetected;

        StatusText = "Start Protection";
        IsActive = false;
    }

    private void OnActivityDetected()
    {
        Application.Current.Dispatcher.Invoke(async () =>
        {
            if (IsActive)
            {
                await HideOverlaysAsync();
            }
        });
    }

    private void OnIdleDetected()
    {
        Application.Current.Dispatcher.Invoke(async () =>
        {
            if (IsActive)
            {
                await ShowOverlaysAsync();
            }
        });
    }

    [RelayCommand]
    private async Task ToggleProtectionAsync()
    {
        IsActive = !IsActive;

        if (IsActive)
        {
            StatusText = "Protection Active (Waiting for Idle)";

            // 1. Reset the timer so it counts from 0 right now.
            _idleDetector.ResetTimer();

            // 2. Start tracking the mouse.
            _inputService.StartHook();

            // REMOVED: await ShowOverlaysAsync(); 
            // Why? Because we are active right now! We let the IdleDetected event 
            // handle the showing later.
        }
        else
        {
            StatusText = "Protection Paused";

            // 1. Stop tracking to save CPU/Stability.
            _inputService.StopHook();

            // 2. Force hide the overlays immediately so you can work.
            await HideOverlaysAsync();
        }
    }

    private async Task ShowOverlaysAsync()
    {
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

    private async Task HideOverlaysAsync()
    {
        var monitorResult = await _monitorService.GetMonitorsAsync(); // Re-fetch to be safe
        if (monitorResult.IsSuccess)
        {
            foreach (var monitor in monitorResult.Value)
            {
                await _windowService.HideOverlayAsync(monitor);
            }
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    private void ShowWindow()
    {
        Application.Current.MainWindow.Visibility = Visibility.Visible;
        Application.Current.MainWindow.Activate();
    }

    [RelayCommand]
    private void HideWindow()
    {
        Application.Current.MainWindow.Visibility = Visibility.Collapsed;
    }

    public void Dispose()
    {
        _idleDetector.IdleDetected -= OnIdleDetected;
        _idleDetector.ActivityDetected -= OnActivityDetected;
        _idleDetector.Dispose();
        GC.SuppressFinalize(this);
    }
}
