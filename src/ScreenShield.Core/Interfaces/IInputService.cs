using ScreenShield.Core.Common;
using System;
using System.Drawing;

namespace ScreenShield.Core.Interfaces;

public interface IInputService
{
    event EventHandler<Point> MouseMoved;
    Result<bool> StartHook();
    void StopHook();
}
