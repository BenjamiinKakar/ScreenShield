using System;
using System.Runtime.InteropServices;
using ScreenShield.Infrastructure.Win32;
using static ScreenShield.Infrastructure.Win32.NativeMethods;

namespace ScreenShield.Infrastructure.Services;

public static class BlurService
{
    public static void EnableBlur(IntPtr windowHandle)
    {
        var accent = new AccentPolicy { AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, GradientColor = 0x99000000 };
        var accentPolicySize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentPolicySize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentPolicySize,
            Data = accentPtr
        };

        SetWindowCompositionAttribute(windowHandle, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }
}
