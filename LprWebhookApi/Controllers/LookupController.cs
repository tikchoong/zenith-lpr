using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/lookup")]
[ApiController]
public class LookupController : ControllerBase
{
    private readonly LprDbContext _context;

    public LookupController(LprDbContext context)
    {
        _context = context;
    }

    // GET: api/lpr/lookup/entry-types
    [HttpGet("entry-types")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetEntryTypes()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "entry_type" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/recurring-patterns
    [HttpGet("recurring-patterns")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetRecurringPatterns()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "recurring_pattern" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/entry-statuses
    [HttpGet("entry-statuses")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetEntryStatuses()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "entry_status" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/color-types
    [HttpGet("color-types")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetColorTypes()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "color_type" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/directions
    [HttpGet("directions")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetDirections()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "direction" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/trigger-types
    [HttpGet("trigger-types")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetTriggerTypes()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "trigger_type" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/vehicle-types
    [HttpGet("vehicle-types")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetVehicleTypes()
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == "vehicle_type" && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/categories
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _context.LookupTables
            .Where(l => l.IsActive)
            .Select(l => l.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/lpr/lookup/{category}
    [HttpGet("{category}")]
    public async Task<ActionResult<IEnumerable<LookupResponse>>> GetLookupsByCategory(string category)
    {
        var lookups = await _context.LookupTables
            .Where(l => l.Category == category && l.IsActive)
            .OrderBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .Select(l => new LookupResponse
            {
                Code = l.Code,
                Name = l.Name,
                Description = l.Description,
                NumericValue = l.NumericValue,
                SortOrder = l.SortOrder
            })
            .ToListAsync();

        if (!lookups.Any())
        {
            return NotFound($"No lookup values found for category: {category}");
        }

        return Ok(lookups);
    }

    // GET: api/lpr/lookup/all
    [HttpGet("all")]
    public async Task<ActionResult<Dictionary<string, IEnumerable<LookupResponse>>>> GetAllLookups()
    {
        var allLookups = await _context.LookupTables
            .Where(l => l.IsActive)
            .OrderBy(l => l.Category)
            .ThenBy(l => l.SortOrder ?? int.MaxValue)
            .ThenBy(l => l.Name)
            .ToListAsync();

        var groupedLookups = allLookups
            .GroupBy(l => l.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(l => new LookupResponse
                {
                    Code = l.Code,
                    Name = l.Name,
                    Description = l.Description,
                    NumericValue = l.NumericValue,
                    SortOrder = l.SortOrder
                }).AsEnumerable()
            );

        return Ok(groupedLookups);
    }
}
