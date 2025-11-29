using ScreenShield.Core.Common;
using ScreenShield.Core.Interfaces;
using ScreenShield.Core.Models;
using ScreenShield.Infrastructure.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScreenShield.Infrastructure.Services;

public class WindowsMonitorService : IMonitorService
{
    public Task<Result<List<MonitorInfo>>> GetMonitorsAsync()
    {
        try
        {
            var monitors = new List<MonitorInfo>();
            
            NativeMethods.MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
            {
                var mi = new NativeMethods.MONITORINFOEX();
                mi.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));

                if (NativeMethods.GetMonitorInfo(hMonitor, ref mi))
                {
                    var bounds = new MonitorBounds(
                        mi.rcMonitor.Left,
                        mi.rcMonitor.Top,
                        mi.rcMonitor.Right - mi.rcMonitor.Left,
                        mi.rcMonitor.Bottom - mi.rcMonitor.Top
                    );

                    var monitorInfo = new MonitorInfo(
                        DeviceName: mi.szDevice,
                        IsPrimary: (mi.dwFlags & 1) == 1, // MONITORINFOF_PRIMARY
                        Bounds: bounds
                    );
                    monitors.Add(monitorInfo);
                }
                return true; // Continue enumeration
            };

            if (!NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero))
            {
                return Task.FromResult(Result<List<MonitorInfo>>.Failure("Failed to enumerate display monitors."));
            }

            return Task.FromResult(Result<List<MonitorInfo>>.Success(monitors));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<List<MonitorInfo>>.Failure($"An error occurred while getting monitors: {ex.Message}"));
        }
    }
}
