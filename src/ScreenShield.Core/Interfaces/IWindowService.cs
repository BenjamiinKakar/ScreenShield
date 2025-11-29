using ScreenShield.Core.Common;
using ScreenShield.Core.Models;
using System.Threading.Tasks;

namespace ScreenShield.Core.Interfaces;

public interface IWindowService
{
    Task<Result<bool>> ShowOverlayAsync(MonitorInfo monitor);
    Task<Result<bool>> HideOverlayAsync(MonitorInfo monitor);
}
