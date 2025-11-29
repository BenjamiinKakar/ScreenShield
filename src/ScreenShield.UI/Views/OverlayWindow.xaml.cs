using ScreenShield.Core.Models;
using ScreenShield.Infrastructure.Services;
using System;
using System.Windows;
using System.Windows.Interop;

namespace ScreenShield.UI.Views;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var handle = new WindowInteropHelper(this).Handle;
        BlurService.EnableBlur(handle);
    }

    public void PositionOnMonitor(MonitorInfo monitor)
    {
        this.Left = monitor.Bounds.X;
        this.Top = monitor.Bounds.Y;
        this.Width = monitor.Bounds.Width;
        this.Height = monitor.Bounds.Height;
    }
}
