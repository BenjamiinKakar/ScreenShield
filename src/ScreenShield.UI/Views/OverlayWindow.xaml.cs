using ScreenShield.Core.Models;
using System.Windows;

namespace ScreenShield.UI.Views;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
    }

    public void PositionOnMonitor(MonitorInfo monitor)
    {
        this.Left = monitor.Bounds.X;
        this.Top = monitor.Bounds.Y;
        this.Width = monitor.Bounds.Width;
        this.Height = monitor.Bounds.Height;
    }
}
