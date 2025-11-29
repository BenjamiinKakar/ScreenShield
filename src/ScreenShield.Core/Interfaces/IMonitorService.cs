using ScreenShield.Core.Common;
using ScreenShield.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScreenShield.Core.Interfaces;

public interface IMonitorService
{
    Task<Result<List<MonitorInfo>>> GetMonitorsAsync();
}
