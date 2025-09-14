using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Models.Entities;
using Serilog;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/tenants")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly LprDbContext _context;

    public TenantsController(LprDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantResponse>>> GetTenants(string siteCode, [FromQuery] bool? isActive = null)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var query = _context.Tenants.Where(t => t.SiteId == site.Id);

            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            var tenants = await query
                .OrderBy(t => t.UnitNumber)
                .ThenBy(t => t.TenantName)
                .ToListAsync();

            var response = tenants.Select(t => new TenantResponse
            {
                Id = t.Id,
                TenantCode = t.TenantCode,
                TenantName = t.TenantName,
                UnitNumber = t.UnitNumber,
                Phone = t.Phone,
                Email = t.Email,
                EmergencyContact = t.EmergencyContact,
                EmergencyPhone = t.EmergencyPhone,
                MoveInDate = t.MoveInDate,
                MoveOutDate = t.MoveOutDate,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving tenants for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{tenantId:int}")]
    public async Task<ActionResult<TenantResponse>> GetTenant(string siteCode, int tenantId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.SiteId == site.Id && t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound($"Tenant '{tenantId}' not found in site '{siteCode}'");
            }

            var response = new TenantResponse
            {
                Id = tenant.Id,
                TenantCode = tenant.TenantCode,
                TenantName = tenant.TenantName,
                UnitNumber = tenant.UnitNumber,
                Phone = tenant.Phone,
                Email = tenant.Email,
                EmergencyContact = tenant.EmergencyContact,
                EmergencyPhone = tenant.EmergencyPhone,
                MoveInDate = tenant.MoveInDate,
                MoveOutDate = tenant.MoveOutDate,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving tenant {TenantId} for site {SiteCode}", tenantId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TenantResponse>> CreateTenant(string siteCode, [FromBody] CreateTenantRequest request)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Check if tenant code already exists in this site (if provided)
            if (!string.IsNullOrEmpty(request.TenantCode))
            {
                var existingTenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.SiteId == site.Id && t.TenantCode == request.TenantCode);

                if (existingTenant != null)
                {
                    return Conflict($"Tenant with code '{request.TenantCode}' already exists in site '{siteCode}'");
                }
            }

            var tenant = new Tenant
            {
                SiteId = site.Id,
                TenantCode = request.TenantCode,
                TenantName = request.TenantName,
                UnitNumber = request.UnitNumber,
                Phone = request.Phone,
                Email = request.Email,
                EmergencyContact = request.EmergencyContact,
                EmergencyPhone = request.EmergencyPhone,
                MoveInDate = request.MoveInDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var response = new TenantResponse
            {
                Id = tenant.Id,
                TenantCode = tenant.TenantCode,
                TenantName = tenant.TenantName,
                UnitNumber = tenant.UnitNumber,
                Phone = tenant.Phone,
                Email = tenant.Email,
                EmergencyContact = tenant.EmergencyContact,
                EmergencyPhone = tenant.EmergencyPhone,
                MoveInDate = tenant.MoveInDate,
                MoveOutDate = tenant.MoveOutDate,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };

            Log.Information("Created new tenant: {TenantName} ({TenantCode}) in site {SiteCode}", 
                tenant.TenantName, tenant.TenantCode, siteCode);

            return CreatedAtAction(nameof(GetTenant), new { siteCode, tenantId = tenant.Id }, response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating tenant {TenantName} for site {SiteCode}", request.TenantName, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPut("{tenantId:int}")]
    public async Task<ActionResult<TenantResponse>> UpdateTenant(string siteCode, int tenantId, [FromBody] UpdateTenantRequest request)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.SiteId == site.Id && t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound($"Tenant '{tenantId}' not found in site '{siteCode}'");
            }

            tenant.TenantName = request.TenantName;
            tenant.UnitNumber = request.UnitNumber;
            tenant.Phone = request.Phone;
            tenant.Email = request.Email;
            tenant.EmergencyContact = request.EmergencyContact;
            tenant.EmergencyPhone = request.EmergencyPhone;
            tenant.MoveInDate = request.MoveInDate;
            tenant.MoveOutDate = request.MoveOutDate;
            tenant.IsActive = request.IsActive;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new TenantResponse
            {
                Id = tenant.Id,
                TenantCode = tenant.TenantCode,
                TenantName = tenant.TenantName,
                UnitNumber = tenant.UnitNumber,
                Phone = tenant.Phone,
                Email = tenant.Email,
                EmergencyContact = tenant.EmergencyContact,
                EmergencyPhone = tenant.EmergencyPhone,
                MoveInDate = tenant.MoveInDate,
                MoveOutDate = tenant.MoveOutDate,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };

            Log.Information("Updated tenant: {TenantName} ({TenantCode}) in site {SiteCode}", 
                tenant.TenantName, tenant.TenantCode, siteCode);

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating tenant {TenantId} for site {SiteCode}", tenantId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpDelete("{tenantId:int}")]
    public async Task<IActionResult> DeleteTenant(string siteCode, int tenantId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var tenant = await _context.Tenants
                .Include(t => t.Whitelists)
                .Include(t => t.EntryLogs)
                .FirstOrDefaultAsync(t => t.SiteId == site.Id && t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound($"Tenant '{tenantId}' not found in site '{siteCode}'");
            }

            // Check if tenant has associated data
            if (tenant.Whitelists.Any() || tenant.EntryLogs.Any())
            {
                return BadRequest("Cannot delete tenant with associated whitelists or entry logs. Please deactivate the tenant instead.");
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();

            Log.Information("Deleted tenant: {TenantName} ({TenantCode}) from site {SiteCode}", 
                tenant.TenantName, tenant.TenantCode, siteCode);

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting tenant {TenantId} for site {SiteCode}", tenantId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{tenantId:int}/whitelists")]
    public async Task<ActionResult<IEnumerable<WhitelistResponse>>> GetTenantWhitelists(string siteCode, int tenantId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.SiteId == site.Id && t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound($"Tenant '{tenantId}' not found in site '{siteCode}'");
            }

            var whitelists = await _context.Whitelists
                .Include(w => w.Device)
                .Where(w => w.SiteId == site.Id && w.TenantId == tenantId)
                .OrderBy(w => w.LicensePlate)
                .ToListAsync();

            var response = whitelists.Select(w => new WhitelistResponse
            {
                Id = w.Id,
                DeviceId = w.DeviceId,
                DeviceName = w.Device?.DeviceName,
                TenantId = w.TenantId,
                TenantName = tenant.TenantName,
                LicensePlate = w.LicensePlate,
                EntryType = w.EntryType,
                IsEnabled = w.IsEnabled,
                IsBlacklist = w.IsBlacklist,
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
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving whitelists for tenant {TenantId} in site {SiteCode}", tenantId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{tenantId:int}/entry-logs")]
    public async Task<ActionResult<IEnumerable<object>>> GetTenantEntryLogs(string siteCode, int tenantId, 
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.SiteId == site.Id && t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound($"Tenant '{tenantId}' not found in site '{siteCode}'");
            }

            var query = _context.EntryLogs
                .Include(e => e.Device)
                .Where(e => e.SiteId == site.Id && e.TenantId == tenantId);

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.EntryTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.EntryTime <= toDate.Value);
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
                    e.CreatedAt
                })
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving entry logs for tenant {TenantId} in site {SiteCode}", tenantId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
