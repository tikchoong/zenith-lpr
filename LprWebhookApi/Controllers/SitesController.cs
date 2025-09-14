using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Models.Entities;
using Serilog;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites")]
[ApiController]
public class SitesController : ControllerBase
{
    private readonly LprDbContext _context;

    public SitesController(LprDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SiteResponse>>> GetSites()
    {
        try
        {
            var sites = await _context.Sites
                .OrderBy(s => s.SiteName)
                .ToListAsync();

            var response = sites.Select(s => new SiteResponse
            {
                Id = s.Id,
                SiteCode = s.SiteCode,
                SiteName = s.SiteName,
                Address = s.Address,
                City = s.City,
                State = s.State,
                PostalCode = s.PostalCode,
                Country = s.Country,
                IsActive = s.IsActive,
                SiteManagerName = s.SiteManagerName,
                SiteManagerPhone = s.SiteManagerPhone,
                SiteManagerEmail = s.SiteManagerEmail,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving sites");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{siteCode}")]
    public async Task<ActionResult<SiteResponse>> GetSite(string siteCode)
    {
        try
        {
            var site = await _context.Sites
                .FirstOrDefaultAsync(s => s.SiteCode == siteCode);

            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var response = new SiteResponse
            {
                Id = site.Id,
                SiteCode = site.SiteCode,
                SiteName = site.SiteName,
                Address = site.Address,
                City = site.City,
                State = site.State,
                PostalCode = site.PostalCode,
                Country = site.Country,
                IsActive = site.IsActive,
                SiteManagerName = site.SiteManagerName,
                SiteManagerPhone = site.SiteManagerPhone,
                SiteManagerEmail = site.SiteManagerEmail,
                CreatedAt = site.CreatedAt,
                UpdatedAt = site.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SiteResponse>> CreateSite([FromBody] CreateSiteRequest request)
    {
        try
        {
            // Check if site code already exists
            var existingSite = await _context.Sites
                .FirstOrDefaultAsync(s => s.SiteCode == request.SiteCode);

            if (existingSite != null)
            {
                return Conflict($"Site with code '{request.SiteCode}' already exists");
            }

            var site = new Site
            {
                SiteCode = request.SiteCode,
                SiteName = request.SiteName,
                Address = request.Address,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                SiteManagerName = request.SiteManagerName,
                SiteManagerPhone = request.SiteManagerPhone,
                SiteManagerEmail = request.SiteManagerEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Sites.Add(site);
            await _context.SaveChangesAsync();

            var response = new SiteResponse
            {
                Id = site.Id,
                SiteCode = site.SiteCode,
                SiteName = site.SiteName,
                Address = site.Address,
                City = site.City,
                State = site.State,
                PostalCode = site.PostalCode,
                Country = site.Country,
                IsActive = site.IsActive,
                SiteManagerName = site.SiteManagerName,
                SiteManagerPhone = site.SiteManagerPhone,
                SiteManagerEmail = site.SiteManagerEmail,
                CreatedAt = site.CreatedAt,
                UpdatedAt = site.UpdatedAt
            };

            Log.Information("Created new site: {SiteCode} - {SiteName}", site.SiteCode, site.SiteName);

            return CreatedAtAction(nameof(GetSite), new { siteCode = site.SiteCode }, response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating site {SiteCode}", request.SiteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPut("{siteCode}")]
    public async Task<ActionResult<SiteResponse>> UpdateSite(string siteCode, [FromBody] UpdateSiteRequest request)
    {
        try
        {
            var site = await _context.Sites
                .FirstOrDefaultAsync(s => s.SiteCode == siteCode);

            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            site.SiteName = request.SiteName;
            site.Address = request.Address;
            site.City = request.City;
            site.State = request.State;
            site.PostalCode = request.PostalCode;
            site.Country = request.Country;
            site.SiteManagerName = request.SiteManagerName;
            site.SiteManagerPhone = request.SiteManagerPhone;
            site.SiteManagerEmail = request.SiteManagerEmail;
            site.IsActive = request.IsActive;
            site.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new SiteResponse
            {
                Id = site.Id,
                SiteCode = site.SiteCode,
                SiteName = site.SiteName,
                Address = site.Address,
                City = site.City,
                State = site.State,
                PostalCode = site.PostalCode,
                Country = site.Country,
                IsActive = site.IsActive,
                SiteManagerName = site.SiteManagerName,
                SiteManagerPhone = site.SiteManagerPhone,
                SiteManagerEmail = site.SiteManagerEmail,
                CreatedAt = site.CreatedAt,
                UpdatedAt = site.UpdatedAt
            };

            Log.Information("Updated site: {SiteCode} - {SiteName}", site.SiteCode, site.SiteName);

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpDelete("{siteCode}")]
    public async Task<IActionResult> DeleteSite(string siteCode)
    {
        try
        {
            var site = await _context.Sites
                .Include(s => s.Devices)
                .Include(s => s.Tenants)
                .Include(s => s.SiteUsers)
                .FirstOrDefaultAsync(s => s.SiteCode == siteCode);

            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Check if site has associated data
            if (site.Devices.Any() || site.Tenants.Any() || site.SiteUsers.Any())
            {
                return BadRequest("Cannot delete site with associated devices, tenants, or users. Please remove them first.");
            }

            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();

            Log.Information("Deleted site: {SiteCode} - {SiteName}", site.SiteCode, site.SiteName);

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{siteCode}/statistics")]
    public async Task<ActionResult<object>> GetSiteStatistics(string siteCode)
    {
        try
        {
            var site = await _context.Sites
                .FirstOrDefaultAsync(s => s.SiteCode == siteCode);

            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var stats = new
            {
                TotalDevices = await _context.Devices.CountAsync(d => d.SiteId == site.Id),
                OnlineDevices = await _context.Devices.CountAsync(d => d.SiteId == site.Id && d.IsOnline),
                TotalTenants = await _context.Tenants.CountAsync(t => t.SiteId == site.Id && t.IsActive),
                TotalWhitelists = await _context.Whitelists.CountAsync(w => w.SiteId == site.Id && w.IsEnabled),
                TodayEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= today),
                TodayAllowed = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= today && e.EntryStatus == "allowed"),
                MonthlyEntries = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisMonth),
                MonthlyAllowed = await _context.EntryLogs.CountAsync(e => e.SiteId == site.Id && e.EntryTime >= thisMonth && e.EntryStatus == "allowed")
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving statistics for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
