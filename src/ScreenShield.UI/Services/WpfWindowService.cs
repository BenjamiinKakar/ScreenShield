using ScreenShield.Core.Common;
using ScreenShield.Core.Interfaces;
using ScreenShield.Core.Models;
using ScreenShield.UI.Views;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenShield.UI.Services;

public class WpfWindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, Window> _activeOverlays = new();

    public WpfWindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<Result<bool>> ShowOverlayAsync(MonitorInfo monitor)
    {
        try
        {
            if (_activeOverlays.ContainsKey(monitor.DeviceName))
            {
                return Task.FromResult(Result<bool>.Success(true));
            }

            var result = Application.Current.Dispatcher.Invoke(() =>
            {
                var overlay = (_serviceProvider.GetService(typeof(OverlayWindow)) as OverlayWindow)!;
                overlay.PositionOnMonitor(monitor);
                overlay.Show();
                _activeOverlays.TryAdd(monitor.DeviceName, overlay);
                return Result<bool>.Success(true);
            });

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool>.Failure($"Failed to show overlay: {ex.Message}"));
        }
    }

    public Task<Result<bool>> HideOverlayAsync(MonitorInfo monitor)
    {
        try
        {
            if (_activeOverlays.TryRemove(monitor.DeviceName, out var overlay))
            {
                Application.Current.Dispatcher.Invoke(overlay.Close);
            }
            return Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool>.Failure($"Failed to hide overlay: {ex.Message}"));
        }
    }
}
