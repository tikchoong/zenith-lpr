using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Models.Entities;
using Serilog;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/whitelists")]
[ApiController]
public class WhitelistsController : ControllerBase
{
    private readonly LprDbContext _context;

    public WhitelistsController(LprDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WhitelistResponse>>> GetWhitelists(string siteCode,
        [FromQuery] string? entryType = null, [FromQuery] bool? isEnabled = null, [FromQuery] bool? isBlacklist = null,
        [FromQuery] int? deviceId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var query = _context.Whitelists
                .Include(w => w.Device)
                .Include(w => w.Tenant)
                .Include(w => w.Staff)
                .Where(w => w.SiteId == site.Id);

            if (!string.IsNullOrEmpty(entryType))
            {
                query = query.Where(w => w.EntryType == entryType);
            }

            if (isEnabled.HasValue)
            {
                query = query.Where(w => w.IsEnabled == isEnabled.Value);
            }

            if (isBlacklist.HasValue)
            {
                query = query.Where(w => w.IsBlacklist == isBlacklist.Value);
            }

            if (deviceId.HasValue)
            {
                query = query.Where(w => w.DeviceId == deviceId.Value || w.DeviceId == null);
            }

            var totalCount = await query.CountAsync();
            var whitelists = await query
                .OrderBy(w => w.LicensePlate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Whitelists = whitelists.Select(w => new WhitelistResponse
                {
                    Id = w.Id,
                    DeviceId = w.DeviceId,
                    DeviceName = w.Device?.DeviceName,
                    TenantId = w.TenantId,
                    TenantName = w.Tenant?.TenantName,
                    StaffId = w.StaffId,
                    StaffName = w.Staff?.StaffName,
                    LicensePlate = w.LicensePlate,
                    EntryType = w.EntryType,
                    IsEnabled = w.IsEnabled,
                    IsBlacklist = w.IsBlacklist,
                    VisitorName = w.VisitorName,
                    VisitorPhone = w.VisitorPhone,
                    VisitorCompany = w.VisitorCompany,
                    VisitPurpose = w.VisitPurpose,
                    EnableTime = w.EnableTime,
                    ExpiryTime = w.ExpiryTime,
                    MaxEntries = w.MaxEntries,
                    CurrentEntries = w.CurrentEntries,
                    IsRecurring = w.IsRecurring,
                    RecurringPattern = w.RecurringPattern,
                    RecurringStartTime = w.RecurringStartTime,
                    RecurringEndTime = w.RecurringEndTime,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt
                })
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving whitelists for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{whitelistId:int}")]
    public async Task<ActionResult<WhitelistResponse>> GetWhitelist(string siteCode, int whitelistId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var whitelist = await _context.Whitelists
                .Include(w => w.Device)
                .Include(w => w.Tenant)
                .Include(w => w.Staff)
                .FirstOrDefaultAsync(w => w.SiteId == site.Id && w.Id == whitelistId);

            if (whitelist == null)
            {
                return NotFound($"Whitelist '{whitelistId}' not found in site '{siteCode}'");
            }

            var response = new WhitelistResponse
            {
                Id = whitelist.Id,
                DeviceId = whitelist.DeviceId,
                DeviceName = whitelist.Device?.DeviceName,
                TenantId = whitelist.TenantId,
                TenantName = whitelist.Tenant?.TenantName,
                StaffId = whitelist.StaffId,
                StaffName = whitelist.Staff?.StaffName,
                LicensePlate = whitelist.LicensePlate,
                EntryType = whitelist.EntryType,
                IsEnabled = whitelist.IsEnabled,
                IsBlacklist = whitelist.IsBlacklist,
                VisitorName = whitelist.VisitorName,
                VisitorPhone = whitelist.VisitorPhone,
                VisitorCompany = whitelist.VisitorCompany,
                VisitPurpose = whitelist.VisitPurpose,
                EnableTime = whitelist.EnableTime,
                ExpiryTime = whitelist.ExpiryTime,
                MaxEntries = whitelist.MaxEntries,
                CurrentEntries = whitelist.CurrentEntries,
                IsRecurring = whitelist.IsRecurring,
                RecurringPattern = whitelist.RecurringPattern,
                RecurringStartTime = whitelist.RecurringStartTime,
                RecurringEndTime = whitelist.RecurringEndTime,
                Notes = whitelist.Notes,
                CreatedAt = whitelist.CreatedAt,
                UpdatedAt = whitelist.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving whitelist {WhitelistId} for site {SiteCode}", whitelistId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<WhitelistResponse>> CreateWhitelist(string siteCode, [FromBody] CreateWhitelistRequest request)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Validate device exists if specified
            if (request.DeviceId.HasValue)
            {
                var deviceExists = await _context.Devices
                    .AnyAsync(d => d.SiteId == site.Id && d.Id == request.DeviceId.Value);
                if (!deviceExists)
                {
                    return BadRequest($"Device '{request.DeviceId}' not found in site '{siteCode}'");
                }
            }

            // Validate tenant exists if specified
            if (request.TenantId.HasValue)
            {
                var tenantExists = await _context.Tenants
                    .AnyAsync(t => t.SiteId == site.Id && t.Id == request.TenantId.Value);
                if (!tenantExists)
                {
                    return BadRequest($"Tenant '{request.TenantId}' not found in site '{siteCode}'");
                }
            }

            // Validate staff exists if specified
            if (request.StaffId.HasValue)
            {
                var staffExists = await _context.SiteStaff
                    .AnyAsync(s => s.SiteId == site.Id && s.Id == request.StaffId.Value);
                if (!staffExists)
                {
                    return BadRequest($"Staff '{request.StaffId}' not found in site '{siteCode}'");
                }
            }

            // Check for duplicate license plate in same device/site
            var existingWhitelist = await _context.Whitelists
                .FirstOrDefaultAsync(w => w.SiteId == site.Id &&
                                        w.LicensePlate == request.LicensePlate &&
                                        (w.DeviceId == request.DeviceId || (w.DeviceId == null && request.DeviceId == null)));

            if (existingWhitelist != null)
            {
                return Conflict($"License plate '{request.LicensePlate}' already exists in whitelist for this device/site");
            }

            var whitelist = new Whitelist
            {
                SiteId = site.Id,
                DeviceId = request.DeviceId,
                TenantId = request.TenantId,
                StaffId = request.StaffId,
                LicensePlate = request.LicensePlate,
                EntryType = request.EntryType,
                IsEnabled = request.IsEnabled,
                IsBlacklist = request.IsBlacklist,
                VisitorName = request.VisitorName,
                VisitorPhone = request.VisitorPhone,
                VisitorCompany = request.VisitorCompany,
                VisitPurpose = request.VisitPurpose,
                EnableTime = request.EnableTime,
                ExpiryTime = request.ExpiryTime,
                MaxEntries = request.MaxEntries,
                CurrentEntries = 0,
                IsRecurring = request.IsRecurring,
                RecurringPattern = request.RecurringPattern,
                RecurringStartTime = request.RecurringStartTime,
                RecurringEndTime = request.RecurringEndTime,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Whitelists.Add(whitelist);
            await _context.SaveChangesAsync();

            // Load related entities for response
            await _context.Entry(whitelist)
                .Reference(w => w.Device)
                .LoadAsync();
            await _context.Entry(whitelist)
                .Reference(w => w.Tenant)
                .LoadAsync();
            await _context.Entry(whitelist)
                .Reference(w => w.Staff)
                .LoadAsync();

            var response = new WhitelistResponse
            {
                Id = whitelist.Id,
                DeviceId = whitelist.DeviceId,
                DeviceName = whitelist.Device?.DeviceName,
                TenantId = whitelist.TenantId,
                TenantName = whitelist.Tenant?.TenantName,
                StaffId = whitelist.StaffId,
                StaffName = whitelist.Staff?.StaffName,
                LicensePlate = whitelist.LicensePlate,
                EntryType = whitelist.EntryType,
                IsEnabled = whitelist.IsEnabled,
                IsBlacklist = whitelist.IsBlacklist,
                VisitorName = whitelist.VisitorName,
                VisitorPhone = whitelist.VisitorPhone,
                VisitorCompany = whitelist.VisitorCompany,
                VisitPurpose = whitelist.VisitPurpose,
                EnableTime = whitelist.EnableTime,
                ExpiryTime = whitelist.ExpiryTime,
                MaxEntries = whitelist.MaxEntries,
                CurrentEntries = whitelist.CurrentEntries,
                IsRecurring = whitelist.IsRecurring,
                RecurringPattern = whitelist.RecurringPattern,
                RecurringStartTime = whitelist.RecurringStartTime,
                RecurringEndTime = whitelist.RecurringEndTime,
                Notes = whitelist.Notes,
                CreatedAt = whitelist.CreatedAt,
                UpdatedAt = whitelist.UpdatedAt
            };

            Log.Information("Created new whitelist: {LicensePlate} ({EntryType}) in site {SiteCode}",
                whitelist.LicensePlate, whitelist.EntryType, siteCode);

            return CreatedAtAction(nameof(GetWhitelist), new { siteCode, whitelistId = whitelist.Id }, response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating whitelist {LicensePlate} for site {SiteCode}", request.LicensePlate, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPut("{whitelistId:int}")]
    public async Task<ActionResult<WhitelistResponse>> UpdateWhitelist(string siteCode, int whitelistId, [FromBody] CreateWhitelistRequest request)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var whitelist = await _context.Whitelists
                .Include(w => w.Device)
                .Include(w => w.Tenant)
                .Include(w => w.Staff)
                .FirstOrDefaultAsync(w => w.SiteId == site.Id && w.Id == whitelistId);

            if (whitelist == null)
            {
                return NotFound($"Whitelist '{whitelistId}' not found in site '{siteCode}'");
            }

            // Validate device exists if specified
            if (request.DeviceId.HasValue)
            {
                var deviceExists = await _context.Devices
                    .AnyAsync(d => d.SiteId == site.Id && d.Id == request.DeviceId.Value);
                if (!deviceExists)
                {
                    return BadRequest($"Device '{request.DeviceId}' not found in site '{siteCode}'");
                }
            }

            // Check for duplicate license plate (excluding current record)
            var existingWhitelist = await _context.Whitelists
                .FirstOrDefaultAsync(w => w.SiteId == site.Id &&
                                        w.Id != whitelistId &&
                                        w.LicensePlate == request.LicensePlate &&
                                        (w.DeviceId == request.DeviceId || (w.DeviceId == null && request.DeviceId == null)));

            if (existingWhitelist != null)
            {
                return Conflict($"License plate '{request.LicensePlate}' already exists in whitelist for this device/site");
            }

            whitelist.DeviceId = request.DeviceId;
            whitelist.TenantId = request.TenantId;
            whitelist.StaffId = request.StaffId;
            whitelist.LicensePlate = request.LicensePlate;
            whitelist.EntryType = request.EntryType;
            whitelist.IsEnabled = request.IsEnabled;
            whitelist.IsBlacklist = request.IsBlacklist;
            whitelist.VisitorName = request.VisitorName;
            whitelist.VisitorPhone = request.VisitorPhone;
            whitelist.VisitorCompany = request.VisitorCompany;
            whitelist.VisitPurpose = request.VisitPurpose;
            whitelist.EnableTime = request.EnableTime;
            whitelist.ExpiryTime = request.ExpiryTime;
            whitelist.MaxEntries = request.MaxEntries;
            whitelist.IsRecurring = request.IsRecurring;
            whitelist.RecurringPattern = request.RecurringPattern;
            whitelist.RecurringStartTime = request.RecurringStartTime;
            whitelist.RecurringEndTime = request.RecurringEndTime;
            whitelist.Notes = request.Notes;
            whitelist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new WhitelistResponse
            {
                Id = whitelist.Id,
                DeviceId = whitelist.DeviceId,
                DeviceName = whitelist.Device?.DeviceName,
                TenantId = whitelist.TenantId,
                TenantName = whitelist.Tenant?.TenantName,
                StaffId = whitelist.StaffId,
                StaffName = whitelist.Staff?.StaffName,
                LicensePlate = whitelist.LicensePlate,
                EntryType = whitelist.EntryType,
                IsEnabled = whitelist.IsEnabled,
                IsBlacklist = whitelist.IsBlacklist,
                VisitorName = whitelist.VisitorName,
                VisitorPhone = whitelist.VisitorPhone,
                VisitorCompany = whitelist.VisitorCompany,
                VisitPurpose = whitelist.VisitPurpose,
                EnableTime = whitelist.EnableTime,
                ExpiryTime = whitelist.ExpiryTime,
                MaxEntries = whitelist.MaxEntries,
                CurrentEntries = whitelist.CurrentEntries,
                IsRecurring = whitelist.IsRecurring,
                RecurringPattern = whitelist.RecurringPattern,
                RecurringStartTime = whitelist.RecurringStartTime,
                RecurringEndTime = whitelist.RecurringEndTime,
                Notes = whitelist.Notes,
                CreatedAt = whitelist.CreatedAt,
                UpdatedAt = whitelist.UpdatedAt
            };

            Log.Information("Updated whitelist: {LicensePlate} ({EntryType}) in site {SiteCode}",
                whitelist.LicensePlate, whitelist.EntryType, siteCode);

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating whitelist {WhitelistId} for site {SiteCode}", whitelistId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpDelete("{whitelistId:int}")]
    public async Task<IActionResult> DeleteWhitelist(string siteCode, int whitelistId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var whitelist = await _context.Whitelists
                .Include(w => w.EntryLogs)
                .FirstOrDefaultAsync(w => w.SiteId == site.Id && w.Id == whitelistId);

            if (whitelist == null)
            {
                return NotFound($"Whitelist '{whitelistId}' not found in site '{siteCode}'");
            }

            // Check if whitelist has associated entry logs
            if (whitelist.EntryLogs.Any())
            {
                return BadRequest("Cannot delete whitelist with associated entry logs. Please disable the whitelist instead.");
            }

            _context.Whitelists.Remove(whitelist);
            await _context.SaveChangesAsync();

            Log.Information("Deleted whitelist: {LicensePlate} ({EntryType}) from site {SiteCode}",
                whitelist.LicensePlate, whitelist.EntryType, siteCode);

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting whitelist {WhitelistId} for site {SiteCode}", whitelistId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("{whitelistId:int}/enable")]
    public async Task<IActionResult> EnableWhitelist(string siteCode, int whitelistId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var whitelist = await _context.Whitelists
                .FirstOrDefaultAsync(w => w.SiteId == site.Id && w.Id == whitelistId);

            if (whitelist == null)
            {
                return NotFound($"Whitelist '{whitelistId}' not found in site '{siteCode}'");
            }

            whitelist.IsEnabled = true;
            whitelist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Log.Information("Enabled whitelist: {LicensePlate} in site {SiteCode}", whitelist.LicensePlate, siteCode);

            return Ok(new { message = "Whitelist enabled successfully" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error enabling whitelist {WhitelistId} for site {SiteCode}", whitelistId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("{whitelistId:int}/disable")]
    public async Task<IActionResult> DisableWhitelist(string siteCode, int whitelistId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var whitelist = await _context.Whitelists
                .FirstOrDefaultAsync(w => w.SiteId == site.Id && w.Id == whitelistId);

            if (whitelist == null)
            {
                return NotFound($"Whitelist '{whitelistId}' not found in site '{siteCode}'");
            }

            whitelist.IsEnabled = false;
            whitelist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Log.Information("Disabled whitelist: {LicensePlate} in site {SiteCode}", whitelist.LicensePlate, siteCode);

            return Ok(new { message = "Whitelist disabled successfully" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error disabling whitelist {WhitelistId} for site {SiteCode}", whitelistId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
