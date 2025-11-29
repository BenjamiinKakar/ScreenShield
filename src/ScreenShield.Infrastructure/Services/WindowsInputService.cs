using ScreenShield.Core.Common;
using ScreenShield.Core.Interfaces;
using ScreenShield.Infrastructure.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;

namespace ScreenShield.Infrastructure.Services;

public class WindowsInputService : IInputService, IDisposable
{
    // The event must be static so it can be invoked from the static HookCallback.
    private static event EventHandler<Point> MouseMovedInternal;

    private static IntPtr _hookId = IntPtr.Zero;

    // CRITICAL: The delegate must be a static field to prevent garbage collection.
    private static readonly NativeMethods.LowLevelMouseProc _proc = HookCallback;

    // Explicit interface implementation to bridge instance-based event subscription
    // from the interface to the internal static event.
    event EventHandler<Point> IInputService.MouseMoved
    {
        add { MouseMovedInternal += value; }
        remove { MouseMovedInternal -= value; }
    }

    public Result<bool> StartHook()
    {
        try
        {
            // Prevent setting the hook multiple times
            if (_hookId != IntPtr.Zero) return Result<bool>.Success(true);
            
            _hookId = SetHook(_proc);
            return Result<bool>.Success(true);
        }
        catch (Win32Exception ex)
        {
            return Result<bool>.Failure($"Failed to set hook: {ex.Message}");
        }
    }

    public void StopHook()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private static IntPtr SetHook(NativeMethods.LowLevelMouseProc proc)
    {
        using (var curProcess = Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            var hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            if (hookId == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return hookId;
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_MOUSEMOVE)
        {
            var hookStruct = (NativeMethods.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHOOKSTRUCT));
            MouseMovedInternal?.Invoke(null, new Point(hookStruct.pt.x, hookStruct.pt.y));
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    public void Dispose()
    {
        StopHook();
        // If there are any subscribers, make sure to clear them
        if (MouseMovedInternal != null)
        {
            foreach(var d in MouseMovedInternal.GetInvocationList())
            {
                MouseMovedInternal -= (d as EventHandler<Point>);
            }
        }
    }
}
