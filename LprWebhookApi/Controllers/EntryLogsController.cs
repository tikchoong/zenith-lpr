using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.Entities;
using Serilog;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/entry-logs")]
[ApiController]
public class EntryLogsController : ControllerBase
{
    private readonly LprDbContext _context;

    public EntryLogsController(LprDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetEntryLogs(string siteCode,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? entryStatus = null,
        [FromQuery] string? entryType = null,
        [FromQuery] int? deviceId = null,
        [FromQuery] int? tenantId = null,
        [FromQuery] string? licensePlate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var query = _context.EntryLogs
                .Include(e => e.Device)
                .Include(e => e.Tenant)
                .Include(e => e.Staff)
                .Include(e => e.PlateRecognitionResult)
                .Where(e => e.SiteId == site.Id);

            // Apply filters
            if (fromDate.HasValue)
            {
                query = query.Where(e => e.EntryTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.EntryTime <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(entryStatus))
            {
                query = query.Where(e => e.EntryStatus == entryStatus);
            }

            if (!string.IsNullOrEmpty(entryType))
            {
                query = query.Where(e => e.EntryType == entryType);
            }

            if (deviceId.HasValue)
            {
                query = query.Where(e => e.DeviceId == deviceId.Value);
            }

            if (tenantId.HasValue)
            {
                query = query.Where(e => e.TenantId == tenantId.Value);
            }

            if (!string.IsNullOrEmpty(licensePlate))
            {
                query = query.Where(e => e.LicensePlate.Contains(licensePlate));
            }

            var totalCount = await query.CountAsync();
            var entryLogs = await query
                .OrderByDescending(e => e.EntryTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                EntryLogs = entryLogs.Select(e => new
                {
                    e.Id,
                    e.LicensePlate,
                    e.EntryType,
                    e.EntryStatus,
                    e.Confidence,
                    e.EntryTime,
                    e.GateOpened,
                    DeviceName = e.Device?.DeviceName,
                    TenantName = e.Tenant?.TenantName,
                    StaffName = e.Staff?.StaffName,
                    PlateRecognitionId = e.PlateRecognitionId,
                    e.CreatedAt
                })
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving entry logs for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{entryLogId:int}")]
    public async Task<ActionResult<object>> GetEntryLog(string siteCode, int entryLogId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var entryLog = await _context.EntryLogs
                .Include(e => e.Device)
                .Include(e => e.Tenant)
                .Include(e => e.Staff)
                .Include(e => e.Whitelist)
                .Include(e => e.PlateRecognitionResult)
                .FirstOrDefaultAsync(e => e.SiteId == site.Id && e.Id == entryLogId);

            if (entryLog == null)
            {
                return NotFound($"Entry log '{entryLogId}' not found in site '{siteCode}'");
            }

            var response = new
            {
                entryLog.Id,
                entryLog.LicensePlate,
                entryLog.EntryType,
                entryLog.EntryStatus,
                entryLog.Confidence,
                entryLog.EntryTime,
                entryLog.GateOpened,
                Device = entryLog.Device != null ? new
                {
                    entryLog.Device.Id,
                    entryLog.Device.DeviceName,
                    entryLog.Device.SerialNumber,
                    entryLog.Device.LocationDescription
                } : null,
                Tenant = entryLog.Tenant != null ? new
                {
                    entryLog.Tenant.Id,
                    entryLog.Tenant.TenantName,
                    entryLog.Tenant.UnitNumber,
                    entryLog.Tenant.Phone
                } : null,
                Staff = entryLog.Staff != null ? new
                {
                    entryLog.Staff.Id,
                    entryLog.Staff.StaffName,
                    entryLog.Staff.Department,
                    entryLog.Staff.Position
                } : null,
                Whitelist = entryLog.Whitelist != null ? new
                {
                    entryLog.Whitelist.Id,
                    entryLog.Whitelist.EntryType,
                    entryLog.Whitelist.IsBlacklist,
                    entryLog.Whitelist.VisitorName,
                    entryLog.Whitelist.VisitorCompany
                } : null,
                PlateRecognition = entryLog.PlateRecognitionResult != null ? new
                {
                    entryLog.PlateRecognitionResult.Id,
                    entryLog.PlateRecognitionResult.PlateId,
                    entryLog.PlateRecognitionResult.Confidence,
                    entryLog.PlateRecognitionResult.Direction,
                    entryLog.PlateRecognitionResult.TriggerType,
                    entryLog.PlateRecognitionResult.RecognitionTimestamp,
                    entryLog.PlateRecognitionResult.ImagePath
                } : null,
                entryLog.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving entry log {EntryLogId} for site {SiteCode}", entryLogId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetEntryStatistics(string siteCode,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
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

            // Use provided date range or default to this month
            var startDate = fromDate ?? thisMonth;
            var endDate = toDate ?? DateTime.Now;

            var query = _context.EntryLogs.Where(e => e.SiteId == site.Id);

            var stats = new
            {
                // Overall counts
                TotalEntries = await query.Where(e => e.EntryTime >= startDate && e.EntryTime <= endDate).CountAsync(),
                AllowedEntries = await query.Where(e => e.EntryTime >= startDate && e.EntryTime <= endDate && e.EntryStatus == "allowed").CountAsync(),
                DeniedEntries = await query.Where(e => e.EntryTime >= startDate && e.EntryTime <= endDate && e.EntryStatus == "denied").CountAsync(),

                // Today's stats
                TodayTotal = await query.Where(e => e.EntryTime >= today).CountAsync(),
                TodayAllowed = await query.Where(e => e.EntryTime >= today && e.EntryStatus == "allowed").CountAsync(),
                TodayDenied = await query.Where(e => e.EntryTime >= today && e.EntryStatus == "denied").CountAsync(),

                // This week's stats
                WeekTotal = await query.Where(e => e.EntryTime >= thisWeek).CountAsync(),
                WeekAllowed = await query.Where(e => e.EntryTime >= thisWeek && e.EntryStatus == "allowed").CountAsync(),
                WeekDenied = await query.Where(e => e.EntryTime >= thisWeek && e.EntryStatus == "denied").CountAsync(),

                // This month's stats
                MonthTotal = await query.Where(e => e.EntryTime >= thisMonth).CountAsync(),
                MonthAllowed = await query.Where(e => e.EntryTime >= thisMonth && e.EntryStatus == "allowed").CountAsync(),
                MonthDenied = await query.Where(e => e.EntryTime >= thisMonth && e.EntryStatus == "denied").CountAsync(),

                // Entry type breakdown
                EntryTypeBreakdown = await query
                    .Where(e => e.EntryTime >= startDate && e.EntryTime <= endDate)
                    .GroupBy(e => e.EntryType)
                    .Select(g => new { EntryType = g.Key, Count = g.Count() })
                    .ToListAsync(),

                // Entry status breakdown
                EntryStatusBreakdown = await query
                    .Where(e => e.EntryTime >= startDate && e.EntryTime <= endDate)
                    .GroupBy(e => e.EntryStatus)
                    .Select(g => new { EntryStatus = g.Key, Count = g.Count() })
                    .ToListAsync(),

                // Device breakdown
                DeviceBreakdown = await query
                    .Include(e => e.Device)
                    .Where(e => e.EntryTime >= startDate && e.EntryTime <= endDate)
                    .GroupBy(e => new { e.DeviceId, e.Device!.DeviceName })
                    .Select(g => new { 
                        DeviceId = g.Key.DeviceId, 
                        DeviceName = g.Key.DeviceName, 
                        Count = g.Count(),
                        AllowedCount = g.Count(e => e.EntryStatus == "allowed")
                    })
                    .ToListAsync(),

                // Hourly breakdown for today
                HourlyBreakdown = await query
                    .Where(e => e.EntryTime >= today)
                    .GroupBy(e => e.EntryTime.Hour)
                    .Select(g => new { 
                        Hour = g.Key, 
                        Count = g.Count(),
                        AllowedCount = g.Count(e => e.EntryStatus == "allowed")
                    })
                    .OrderBy(x => x.Hour)
                    .ToListAsync(),

                // Date range
                DateRange = new { StartDate = startDate, EndDate = endDate }
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving entry statistics for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("recent")]
    public async Task<ActionResult<object>> GetRecentEntries(string siteCode, [FromQuery] int count = 10)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var recentEntries = await _context.EntryLogs
                .Include(e => e.Device)
                .Include(e => e.Tenant)
                .Include(e => e.Staff)
                .Where(e => e.SiteId == site.Id)
                .OrderByDescending(e => e.EntryTime)
                .Take(Math.Min(count, 100)) // Max 100 entries
                .Select(e => new
                {
                    e.Id,
                    e.LicensePlate,
                    e.EntryType,
                    e.EntryStatus,
                    e.EntryTime,
                    e.GateOpened,
                    DeviceName = e.Device!.DeviceName,
                    TenantName = e.Tenant != null ? e.Tenant.TenantName : null,
                    StaffName = e.Staff != null ? e.Staff.StaffName : null,
                    MinutesAgo = (int)(DateTime.UtcNow - e.EntryTime).TotalMinutes
                })
                .ToListAsync();

            return Ok(recentEntries);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving recent entries for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("export")]
    public async Task<ActionResult> ExportEntryLogs(string siteCode,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string format = "csv")
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var query = _context.EntryLogs
                .Include(e => e.Device)
                .Include(e => e.Tenant)
                .Include(e => e.Staff)
                .Where(e => e.SiteId == site.Id);

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.EntryTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.EntryTime <= toDate.Value);
            }

            var entryLogs = await query
                .OrderByDescending(e => e.EntryTime)
                .Take(10000) // Limit to 10k records for export
                .ToListAsync();

            if (format.ToLower() == "csv")
            {
                var csv = "EntryTime,LicensePlate,EntryType,EntryStatus,GateOpened,DeviceName,TenantName,StaffName,Confidence\n";
                foreach (var entry in entryLogs)
                {
                    csv += $"{entry.EntryTime:yyyy-MM-dd HH:mm:ss}," +
                           $"{entry.LicensePlate}," +
                           $"{entry.EntryType}," +
                           $"{entry.EntryStatus}," +
                           $"{entry.GateOpened}," +
                           $"{entry.Device?.DeviceName ?? ""}," +
                           $"{entry.Tenant?.TenantName ?? ""}," +
                           $"{entry.Staff?.StaffName ?? ""}," +
                           $"{entry.Confidence}\n";
                }

                var fileName = $"entry_logs_{siteCode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }

            return BadRequest("Unsupported export format. Use 'csv'.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting entry logs for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
