using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using Serilog;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly LprDbContext _context;

    public DashboardController(LprDbContext context)
    {
        _context = context;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<object>> GetDashboardOverview(string siteCode)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var overview = new
            {
                SiteInfo = new
                {
                    site.SiteCode,
                    site.SiteName,
                    site.IsActive
                },

                DeviceStatus = new
                {
                    TotalDevices = await _context.Devices.CountAsync(d => d.SiteId == site.Id),
                    OnlineDevices = await _context.Devices.CountAsync(d => d.SiteId == site.Id && d.IsOnline),
                    OfflineDevices = await _context.Devices.CountAsync(d => d.SiteId == site.Id && !d.IsOnline),
                    LastHeartbeat = await _context.Devices
                        .Where(d => d.SiteId == site.Id && d.LastHeartbeat.HasValue)
                        .MaxAsync(d => (DateTime?)d.LastHeartbeat)
                },

                AccessControl = new
                {
                    TotalTenants = await _context.Tenants.CountAsync(t => t.SiteId == site.Id && t.IsActive),
                    TotalStaff = await _context.SiteStaff.CountAsync(s => s.SiteId == site.Id && s.IsActive),
                    ActiveWhitelists = await _context.Whitelists.CountAsync(w => w.SiteId == site.Id && w.IsEnabled),
                    ExpiredWhitelists = await _context.Whitelists.CountAsync(w => w.SiteId == site.Id && w.IsEnabled && w.ExpiryTime.HasValue && w.ExpiryTime < DateTime.UtcNow),
                    VisitorPasses = await _context.Whitelists.CountAsync(w => w.SiteId == site.Id && w.IsEnabled && w.EntryType == "visitor")
                },

                TodayActivity = new
                {
                    TotalEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= today),
                    AllowedEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= today && e.EntryStatus == "allowed"),
                    DeniedEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= today && e.EntryStatus == "denied"),
                    UniqueVehicles = await _context.EntryLogs
                        .Where(e => e.SiteId == site.Id && e.EntryTime >= today)
                        .Select(e => e.LicensePlate)
                        .Distinct()
                        .CountAsync()
                },

                WeeklyActivity = new
                {
                    TotalEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisWeek),
                    AllowedEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisWeek && e.EntryStatus == "allowed"),
                    DeniedEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisWeek && e.EntryStatus == "denied")
                },

                MonthlyActivity = new
                {
                    TotalEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisMonth),
                    AllowedEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisMonth && e.EntryStatus == "allowed"),
                    DeniedEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisMonth && e.EntryStatus == "denied")
                },

                RecentAlerts = await GetRecentAlerts(site.Id),
                
                SystemHealth = new
                {
                    DatabaseConnected = true,
                    LastDataUpdate = await _context.EntryLogs
                        .Where(e => e.SiteId == site.Id)
                        .MaxAsync(e => (DateTime?)e.CreatedAt),
                    PendingCommands = await _context.CommandQueue
                        .CountAsync(c => c.SiteId == site.Id && !c.IsProcessed)
                }
            };

            return Ok(overview);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving dashboard overview for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("activity-chart")]
    public async Task<ActionResult<object>> GetActivityChart(string siteCode, 
        [FromQuery] string period = "today", [FromQuery] string groupBy = "hour")
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var now = DateTime.Now;
            DateTime startDate;
            
            switch (period.ToLower())
            {
                case "today":
                    startDate = DateTime.Today;
                    break;
                case "week":
                    startDate = now.AddDays(-7);
                    break;
                case "month":
                    startDate = now.AddDays(-30);
                    break;
                default:
                    startDate = DateTime.Today;
                    break;
            }

            var query = _context.EntryLogs
                .Where(e => e.SiteId == site.Id && e.EntryTime >= startDate);

            object chartData;

            if (groupBy.ToLower() == "hour" && period.ToLower() == "today")
            {
                chartData = await query
                    .GroupBy(e => e.EntryTime.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        TotalEntries = g.Count(),
                        AllowedEntries = g.Count(e => e.EntryStatus == "allowed"),
                        DeniedEntries = g.Count(e => e.EntryStatus == "denied")
                    })
                    .OrderBy(x => x.Hour)
                    .ToListAsync();
            }
            else
            {
                chartData = await query
                    .GroupBy(e => e.EntryTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalEntries = g.Count(),
                        AllowedEntries = g.Count(e => e.EntryStatus == "allowed"),
                        DeniedEntries = g.Count(e => e.EntryStatus == "denied")
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }

            return Ok(new
            {
                Period = period,
                GroupBy = groupBy,
                StartDate = startDate,
                Data = chartData
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving activity chart for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("device-status")]
    public async Task<ActionResult<object>> GetDeviceStatus(string siteCode)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var devices = await _context.Devices
                .Where(d => d.SiteId == site.Id)
                .Select(d => new
                {
                    d.Id,
                    d.SerialNumber,
                    d.DeviceName,
                    d.LocationDescription,
                    d.IsOnline,
                    d.LastHeartbeat,
                    d.FirmwareVersion,
                    UptimeMinutes = d.LastHeartbeat.HasValue ? 
                        (int)(DateTime.UtcNow - d.LastHeartbeat.Value).TotalMinutes : (int?)null,
                    TodayRecognitions = _context.PlateRecognitionResults
                        .Count(p => p.DeviceId == d.Id && p.CreatedAt >= DateTime.Today)
                })
                .OrderBy(d => d.DeviceName)
                .ToListAsync();

            var summary = new
            {
                TotalDevices = devices.Count,
                OnlineDevices = devices.Count(d => d.IsOnline),
                OfflineDevices = devices.Count(d => !d.IsOnline),
                AverageUptime = devices.Where(d => d.UptimeMinutes.HasValue).Average(d => d.UptimeMinutes),
                TotalRecognitionsToday = devices.Sum(d => d.TodayRecognitions)
            };

            return Ok(new
            {
                Summary = summary,
                Devices = devices
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving device status for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("top-vehicles")]
    public async Task<ActionResult<object>> GetTopVehicles(string siteCode, 
        [FromQuery] int days = 7, [FromQuery] int limit = 10)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var startDate = DateTime.Now.AddDays(-days);

            var topVehicles = await _context.EntryLogs
                .Include(e => e.Tenant)
                .Include(e => e.Staff)
                .Where(e => e.SiteId == site.Id && e.EntryTime >= startDate)
                .GroupBy(e => new { e.LicensePlate, e.TenantId, e.StaffId })
                .Select(g => new
                {
                    LicensePlate = g.Key.LicensePlate,
                    TotalEntries = g.Count(),
                    AllowedEntries = g.Count(e => e.EntryStatus == "allowed"),
                    DeniedEntries = g.Count(e => e.EntryStatus == "denied"),
                    LastEntry = g.Max(e => e.EntryTime),
                    TenantName = g.FirstOrDefault(e => e.Tenant != null) != null ? 
                        g.FirstOrDefault(e => e.Tenant != null)!.Tenant!.TenantName : null,
                    StaffName = g.FirstOrDefault(e => e.Staff != null) != null ? 
                        g.FirstOrDefault(e => e.Staff != null)!.Staff!.StaffName : null,
                    EntryType = g.FirstOrDefault()!.EntryType
                })
                .OrderByDescending(x => x.TotalEntries)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                Period = $"Last {days} days",
                TopVehicles = topVehicles
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving top vehicles for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<object>> GetAlerts(string siteCode, [FromQuery] int limit = 20)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var alerts = await GetRecentAlerts(site.Id, limit);

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving alerts for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    private async Task<List<object>> GetRecentAlerts(int siteId, int limit = 5)
    {
        var alerts = new List<object>();

        // Device offline alerts
        var offlineDevices = await _context.Devices
            .Where(d => d.SiteId == siteId && !d.IsOnline)
            .Select(d => new
            {
                Type = "device_offline",
                Severity = "warning",
                Message = $"Device '{d.DeviceName}' ({d.SerialNumber}) is offline",
                Timestamp = d.LastHeartbeat ?? d.UpdatedAt,
                DeviceId = d.Id,
                DeviceName = d.DeviceName
            })
            .ToListAsync();

        alerts.AddRange(offlineDevices);

        // Expired whitelists
        var expiredWhitelists = await _context.Whitelists
            .Include(w => w.Tenant)
            .Where(w => w.SiteId == siteId && w.IsEnabled && w.ExpiryTime.HasValue && w.ExpiryTime < DateTime.UtcNow)
            .Select(w => new
            {
                Type = "whitelist_expired",
                Severity = "info",
                Message = $"Whitelist for '{w.LicensePlate}' has expired",
                Timestamp = w.ExpiryTime!.Value,
                WhitelistId = w.Id,
                LicensePlate = w.LicensePlate,
                TenantName = w.Tenant != null ? w.Tenant.TenantName : null
            })
            .ToListAsync();

        alerts.AddRange(expiredWhitelists);

        // Recent denied entries
        var recentDenied = await _context.EntryLogs
            .Include(e => e.Device)
            .Where(e => e.SiteId == siteId && e.EntryStatus == "denied" && e.EntryTime >= DateTime.Now.AddHours(-24))
            .OrderByDescending(e => e.EntryTime)
            .Take(10)
            .Select(e => new
            {
                Type = "entry_denied",
                Severity = "warning",
                Message = $"Entry denied for '{e.LicensePlate}' at {e.Device!.DeviceName}",
                Timestamp = e.EntryTime,
                EntryLogId = e.Id,
                LicensePlate = e.LicensePlate,
                DeviceName = e.Device.DeviceName
            })
            .ToListAsync();

        alerts.AddRange(recentDenied);

        return alerts
            .OrderByDescending(a => a.GetType().GetProperty("Timestamp")?.GetValue(a))
            .Take(limit)
            .ToList();
    }
}
