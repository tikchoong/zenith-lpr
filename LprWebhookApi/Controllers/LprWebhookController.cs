using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Models.Entities;
using LprWebhookApi.Services;
using Serilog;
using System.Text.Json;
using System.Net;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/webhook")]
[ApiController]
public class LprWebhookController : ControllerBase
{
    private readonly LprDbContext _context;
    private readonly WhitelistSyncService _whitelistSyncService;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LprWebhookController(LprDbContext context, WhitelistSyncService whitelistSyncService)
    {
        _context = context;
        _whitelistSyncService = whitelistSyncService;
    }

    [HttpPost("plate-recognition")]
    public async Task<IActionResult> PlateRecognition(string siteCode, [FromBody] AlarmInfoPlateRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Log.Information("=== LPR Plate Recognition Request Received ===");
            Log.Information("Site Code: {SiteCode}", siteCode);
            Log.Information("Remote IP: {RemoteIP}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            Log.Information("User-Agent: {UserAgent}", Request.Headers.UserAgent.ToString());
            Log.Information("Content-Type: {ContentType}", Request.ContentType);

            // Log the JSON payload
            var jsonString = JsonSerializer.Serialize(request, JsonOptions);
            Log.Information("Raw JSON: {JsonPayload}", jsonString);

            // Pretty print JSON
            Log.Information("Formatted JSON:\n{FormattedJson}", jsonString);

            // Find site
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                Log.Warning("Site not found: {SiteCode}", siteCode);
                return NotFound($"Site '{siteCode}' not found");
            }

            // Find or create device
            var device = await FindOrCreateDevice(site.Id, request.AlarmInfoPlate);

            // Save plate recognition result
            var plateResult = await SavePlateRecognitionResult(site.Id, device.Id, request.AlarmInfoPlate);

            // Process whitelist sync if needed
            await _whitelistSyncService.ProcessWhitelistSync(device.Id);

            // Process whitelist and create entry log
            var (entryLog, whitelistEntry) = await ProcessPlateRecognition(site.Id, device.Id, plateResult);

            // Build response
            var response = await BuildPlateRecognitionResponse(plateResult, entryLog, whitelistEntry, device.Id);

            // Log response
            var responseJson = JsonSerializer.Serialize(response, JsonOptions);
            Log.Information("Response JSON:\n{ResponseJson}", responseJson);

            // Save response log
            await SaveResponseLog(site.Id, device.Id, "plate-recognition", request, response, startTime);

            Log.Information("=== End of LPR Plate Recognition Request ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing plate recognition request for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat(string siteCode, [FromForm] HeartbeatRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Log.Information("=== LPR Heartbeat Request Received ===");
            Log.Information("Site Code: {SiteCode}", siteCode);
            Log.Information("Device: {DeviceName} ({SerialNumber})", request.DeviceName, request.SerialNumber);
            Log.Information("IP: {IpAddress}:{Port}", request.IpAddress, request.Port);

            // Find site
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                Log.Warning("Site not found: {SiteCode}", siteCode);
                return NotFound($"Site '{siteCode}' not found");
            }

            // Find device
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.SerialNumber == request.SerialNumber);

            if (device != null)
            {
                // Update device status
                device.IsOnline = true;
                device.LastHeartbeat = DateTime.UtcNow;
                device.UpdatedAt = DateTime.UtcNow;

                // Save heartbeat record
                var heartbeat = new DeviceHeartbeat
                {
                    SiteId = site.Id,
                    DeviceId = device.Id,
                    HeartbeatType = "normal",
                    UserName = request.UserName,
                    Password = request.Password,
                    ChannelNum = int.TryParse(request.ChannelNum, out var channelNum) ? channelNum : null,
                    ReceivedAt = DateTime.UtcNow
                };

                _context.DeviceHeartbeats.Add(heartbeat);
                await _context.SaveChangesAsync();

                Log.Information("Heartbeat processed for device {SerialNumber}", request.SerialNumber);
            }
            else
            {
                Log.Warning("Device not found: {SerialNumber} in site {SiteCode}", request.SerialNumber, siteCode);
            }

            // Process whitelist sync if needed
            if (device != null)
            {
                await _whitelistSyncService.ProcessWhitelistSync(device.Id);
            }

            // Check for pending commands
            var response = await GetPendingCommands(site.Id, device?.Id);

            // Save response log
            await SaveResponseLog(site.Id, device?.Id, "heartbeat", request, response, startTime);

            Log.Information("=== End of LPR Heartbeat Request ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing heartbeat request for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("comet-poll")]
    public async Task<IActionResult> CometPoll(string siteCode, [FromBody] HeartbeatRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Log.Information("=== LPR Comet Poll Request Received ===");
            Log.Information("Site Code: {SiteCode}", siteCode);
            Log.Information("Device: {DeviceName} ({SerialNumber})", request.DeviceName, request.SerialNumber);

            // Find site
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Find device
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.SerialNumber == request.SerialNumber);

            if (device != null)
            {
                // Update device status
                device.IsOnline = true;
                device.LastHeartbeat = DateTime.UtcNow;
                device.UpdatedAt = DateTime.UtcNow;

                // Save heartbeat record
                var heartbeat = new DeviceHeartbeat
                {
                    SiteId = site.Id,
                    DeviceId = device.Id,
                    HeartbeatType = "comet",
                    UserName = request.UserName,
                    Password = request.Password,
                    ChannelNum = int.TryParse(request.ChannelNum, out var channelNum) ? channelNum : null,
                    ReceivedAt = DateTime.UtcNow
                };

                _context.DeviceHeartbeats.Add(heartbeat);
                await _context.SaveChangesAsync();
            }

            // Process whitelist sync if needed
            if (device != null)
            {
                await _whitelistSyncService.ProcessWhitelistSync(device.Id);
            }

            // Get pending commands and business logic
            var response = await GetPendingCommands(site.Id, device?.Id);

            // Save response log
            await SaveResponseLog(site.Id, device?.Id, "comet-poll", request, response, startTime);

            Log.Information("=== End of LPR Comet Poll Request ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing comet poll request for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("io-trigger")]
    public async Task<IActionResult> IoTrigger(string siteCode, [FromBody] IoTriggerRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Log.Information("=== LPR IO Trigger Request Received ===");
            Log.Information("Site Code: {SiteCode}", siteCode);
            Log.Information("Device: {DeviceName} ({SerialNumber})", request.AlarmGioIn.DeviceName, request.AlarmGioIn.SerialNumber);
            Log.Information("IO Source: {Source}, Value: {Value}",
                request.AlarmGioIn.Result.TriggerResult.Source,
                request.AlarmGioIn.Result.TriggerResult.Value);

            // Find site
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Find device
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.SerialNumber == request.AlarmGioIn.SerialNumber);

            if (device != null)
            {
                // Save IO trigger event
                var ioEvent = new IoTriggerEvent
                {
                    SiteId = site.Id,
                    DeviceId = device.Id,
                    Source = request.AlarmGioIn.Result.TriggerResult.Source,
                    Value = request.AlarmGioIn.Result.TriggerResult.Value,
                    TriggeredAt = DateTime.UtcNow
                };

                _context.IoTriggerEvents.Add(ioEvent);
                await _context.SaveChangesAsync();
            }

            // Simple acknowledgment response
            var response = new { status = "ok", message = "IO trigger received" };

            // Save response log
            await SaveResponseLog(site.Id, device?.Id, "io-trigger", request, response, startTime);

            Log.Information("=== End of LPR IO Trigger Request ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing IO trigger request for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("serial-data")]
    public async Task<IActionResult> SerialData(string siteCode, [FromBody] SerialDataRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Log.Information("=== LPR Serial Data Request Received ===");
            Log.Information("Site Code: {SiteCode}", siteCode);
            Log.Information("Device: {DeviceName} ({SerialNumber})", request.SerialData.DeviceName, request.SerialData.SerialNumber);
            Log.Information("Serial Channel: {Channel}, Data Length: {Length}",
                request.SerialData.SerialChannel, request.SerialData.DataLen);

            // Find site
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Find device
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.SerialNumber == request.SerialData.SerialNumber);

            if (device != null)
            {
                // Save serial data log
                var serialLog = new SerialDataLog
                {
                    SiteId = site.Id,
                    DeviceId = device.Id,
                    SerialChannel = request.SerialData.SerialChannel,
                    DataBase64 = request.SerialData.Data,
                    DataLength = request.SerialData.DataLen,
                    ReceivedAt = DateTime.UtcNow
                };

                _context.SerialDataLogs.Add(serialLog);
                await _context.SaveChangesAsync();
            }

            // Simple acknowledgment response
            var response = new { status = "ok", message = "Serial data received" };

            // Save response log
            await SaveResponseLog(site.Id, device?.Id, "serial-data", request, response, startTime);

            Log.Information("=== End of LPR Serial Data Request ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing serial data request for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("screenshot")]
    public async Task<IActionResult> Screenshot(string siteCode, [FromBody] ScreenshotRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Log.Information("=== LPR Screenshot Request Received ===");
            Log.Information("Site Code: {SiteCode}", siteCode);
            Log.Information("IP Address: {IpAddress}", request.IpAddress);
            Log.Information("Image Length: {Length}", request.TriggerImage.ImageFileLen);

            // Find site
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Find device by IP address
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.IpAddress != null && d.IpAddress.ToString() == request.IpAddress);

            if (device != null)
            {
                // Save screenshot
                var screenshot = new Screenshot
                {
                    SiteId = site.Id,
                    DeviceId = device.Id,
                    ImageBase64 = request.TriggerImage.ImageFile,
                    ImageLength = request.TriggerImage.ImageFileLen,
                    TriggerSource = "webhook",
                    CapturedAt = DateTime.UtcNow
                };

                _context.Screenshots.Add(screenshot);
                await _context.SaveChangesAsync();
            }

            // Simple acknowledgment response
            var response = new { status = "ok", message = "Screenshot received" };

            // Save response log
            await SaveResponseLog(site.Id, device?.Id, "screenshot", request, response, startTime);

            Log.Information("=== End of LPR Screenshot Request ===");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing screenshot request for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    private async Task<Device> FindOrCreateDevice(int siteId, AlarmInfoPlate alarmInfo)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.SiteId == siteId && d.SerialNumber == alarmInfo.SerialNumber);

        if (device == null)
        {
            // Create new device
            device = new Device
            {
                SiteId = siteId,
                SerialNumber = alarmInfo.SerialNumber,
                DeviceName = alarmInfo.DeviceName,
                IpAddress = IPAddress.TryParse(alarmInfo.IpAddress, out var ip) ? ip : null,
                IsOnline = true,
                LastHeartbeat = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            Log.Information("Created new device: {SerialNumber} for site {SiteId}", alarmInfo.SerialNumber, siteId);
        }
        else
        {
            // Update existing device
            device.IsOnline = true;
            device.LastHeartbeat = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return device;
    }

    private async Task<PlateRecognitionResult> SavePlateRecognitionResult(int siteId, int deviceId, AlarmInfoPlate alarmInfo)
    {
        var plateData = alarmInfo.Result.PlateRecognitionData;

        var plateResult = new PlateRecognitionResult
        {
            SiteId = siteId,
            DeviceId = deviceId,
            PlateId = plateData.PlateId,
            LicensePlate = plateData.License,
            Confidence = plateData.Confidence,
            ColorType = plateData.ColorType,
            PlateType = plateData.Type,
            Direction = plateData.Direction,
            TriggerType = plateData.TriggerType,
            IsOffline = plateData.IsOffline == 1,
            IsFakePlate = plateData.IsFakePlate == 1,
            PlateTrueWidth = plateData.PlateTrueWidth,
            PlateDistance = plateData.PlateDistance,
            CarBrand = plateData.CarBrand?.Brand,
            CarYear = plateData.CarBrand?.Year,
            CarType = plateData.CarBrand?.Type,
            FeatureCode = plateData.FeatureCode,
            RecognitionTimestamp = ConvertTimestamp(plateData.TimeStamp),
            TimeUsed = plateData.TimeUsed,
            Usec = plateData.TimeStamp.Timeval.Usec,
            ImagePath = plateData.ImagePath,
            ImageFileBase64 = plateData.ImageFile,
            ImageFileLength = plateData.ImageFileLen,
            ImageFragmentBase64 = plateData.ImageFragmentFile,
            ImageFragmentLength = plateData.ImageFragmentFileLen,
            CreatedAt = DateTime.UtcNow
        };

        // Convert location data to JSON
        if (plateData.Location?.Rect != null)
        {
            plateResult.PlateLocation = JsonDocument.Parse(JsonSerializer.Serialize(plateData.Location));
        }

        if (plateData.CarLocation?.Rect != null)
        {
            plateResult.CarLocation = JsonDocument.Parse(JsonSerializer.Serialize(plateData.CarLocation));
        }

        _context.PlateRecognitionResults.Add(plateResult);
        await _context.SaveChangesAsync();

        return plateResult;
    }

    private DateTime? ConvertTimestamp(TimeStampData timeStamp)
    {
        try
        {
            if (timeStamp?.Timeval?.Sec > 0)
            {
                return DateTimeOffset.FromUnixTimeSeconds(timeStamp.Timeval.Sec).DateTime;
            }

            if (timeStamp?.Timeval?.DecYear > 0)
            {
                return new DateTime(
                    timeStamp.Timeval.DecYear,
                    Math.Max(1, timeStamp.Timeval.DecMonth),
                    Math.Max(1, timeStamp.Timeval.DecDay),
                    timeStamp.Timeval.DecHour,
                    timeStamp.Timeval.DecMinute,
                    timeStamp.Timeval.DecSecond);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to convert timestamp: {TimeStamp}", JsonSerializer.Serialize(timeStamp));
        }

        return null;
    }

    private async Task<(EntryLog entryLog, Whitelist? whitelist)> ProcessPlateRecognition(int siteId, int deviceId, PlateRecognitionResult plateResult)
    {
        // Simplified whitelist check - just match license plate (ignore case and whitespace)
        var normalizedInputPlate = plateResult.LicensePlate?.Replace(" ", "").ToUpperInvariant() ?? "";

        var whitelist = await _context.Whitelists
            .Include(w => w.Tenant)
            .Include(w => w.Staff)
            .FirstOrDefaultAsync(w =>
                w.SiteId == siteId &&
                w.LicensePlate.Replace(" ", "").ToUpper() == normalizedInputPlate);

        string entryStatus;
        string entryType = "unknown";
        bool gateOpened = false;

        if (whitelist != null)
        {
            // Simple logic: if license plate matches, allow entry (ignore all other conditions)
            entryStatus = "allowed";
            entryType = whitelist.EntryType;
            gateOpened = !whitelist.IsBlacklist; // Only deny if it's explicitly a blacklist entry

            // Optional: Still increment entry count for tracking purposes
            whitelist.CurrentEntries++;
            whitelist.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            entryStatus = "denied";
        }

        // Create entry log
        var entryLog = new EntryLog
        {
            SiteId = siteId,
            DeviceId = deviceId,
            TenantId = whitelist?.TenantId,
            StaffId = whitelist?.StaffId,
            WhitelistId = whitelist?.Id,
            PlateRecognitionId = plateResult.Id,
            LicensePlate = plateResult.LicensePlate ?? string.Empty,
            EntryType = entryType,
            EntryStatus = entryStatus,
            Confidence = plateResult.Confidence,
            EntryTime = DateTime.UtcNow,
            GateOpened = gateOpened
        };

        _context.EntryLogs.Add(entryLog);
        await _context.SaveChangesAsync();

        Log.Information("Entry processed: Plate={Plate}, Status={Status}, Type={Type}, Gate={Gate}",
            plateResult.LicensePlate, entryStatus, entryType, gateOpened);

        return (entryLog, whitelist);
    }

    private bool IsWithinAllowedTime(Whitelist whitelist)
    {
        var now = DateTime.UtcNow;

        // Check absolute time window
        if (whitelist.EnableTime.HasValue && now < whitelist.EnableTime.Value)
            return false;

        if (whitelist.ExpiryTime.HasValue && now > whitelist.ExpiryTime.Value)
            return false;

        // Check recurring time patterns
        if (whitelist.IsRecurring && whitelist.RecurringStartTime.HasValue && whitelist.RecurringEndTime.HasValue)
        {
            var currentTime = TimeOnly.FromDateTime(now);

            if (whitelist.RecurringPattern == "daily")
            {
                return currentTime >= whitelist.RecurringStartTime && currentTime <= whitelist.RecurringEndTime;
            }
            else if (whitelist.RecurringPattern == "weekdays")
            {
                var dayOfWeek = now.DayOfWeek;
                if (dayOfWeek >= DayOfWeek.Monday && dayOfWeek <= DayOfWeek.Friday)
                {
                    return currentTime >= whitelist.RecurringStartTime && currentTime <= whitelist.RecurringEndTime;
                }
                return false;
            }
            // Add more recurring patterns as needed
        }

        return true;
    }

    private async Task<LprResponse> BuildPlateRecognitionResponse(PlateRecognitionResult plateResult, EntryLog entryLog, Whitelist? whitelist, int deviceId)
    {
        var response = new LprResponse
        {
            ResponseAlarmInfoPlate = new ResponseAlarmInfoPlate
            {
                Info = entryLog.GateOpened ? "ok" : "denied",
                PlateId = plateResult.PlateId,
                ChannelNum = 0
            }
        };

        // Check for pending whitelist operations
        var pendingWhitelistCommands = await _context.CommandQueue
            .Where(c => c.DeviceId == deviceId && !c.IsProcessed && c.CommandType!.StartsWith("whitelist_"))
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.CreatedAt)
            .Take(5) // Max 5 as per documentation
            .ToListAsync();

        if (pendingWhitelistCommands.Any())
        {
            response.ResponseAlarmInfoPlate.WhiteListOperate = await BuildWhitelistOperations(pendingWhitelistCommands);
        }

        return response;
    }

    private async Task<WhiteListOperate> BuildWhitelistOperations(List<CommandQueue> commands)
    {
        var whitelistOp = new WhiteListOperate
        {
            WhiteListData = new List<WhiteListData>()
        };

        foreach (var command in commands)
        {
            try
            {
                if (command.CommandData != null)
                {
                    var commandData = JsonSerializer.Deserialize<Dictionary<string, object>>(command.CommandData.RootElement.GetRawText());

                    if (command.CommandType == "whitelist_add")
                    {
                        whitelistOp.OperateType = 0; // Add
                        whitelistOp.WhiteListData.Add(new WhiteListData
                        {
                            Plate = commandData.GetValueOrDefault("plate", "").ToString() ?? "",
                            Enable = int.Parse(commandData.GetValueOrDefault("enable", "1").ToString() ?? "1"),
                            NeedAlarm = int.Parse(commandData.GetValueOrDefault("need_alarm", "0").ToString() ?? "0"),
                            EnableTime = commandData.GetValueOrDefault("enable_time", "")?.ToString(),
                            OverdueTime = commandData.GetValueOrDefault("overdue_time", "")?.ToString()
                        });
                    }
                    else if (command.CommandType == "whitelist_remove")
                    {
                        whitelistOp.OperateType = 1; // Delete
                        whitelistOp.WhiteListData.Add(new WhiteListData
                        {
                            Plate = commandData.GetValueOrDefault("plate", "").ToString() ?? ""
                        });
                    }
                    else if (command.CommandType == "whitelist_clear")
                    {
                        whitelistOp.OperateType = 1; // Delete
                        whitelistOp.WhiteListData.Add(new WhiteListData
                        {
                            Plate = "" // Empty plate clears all
                        });
                    }
                    else if (command.CommandType == "whitelist_add_batch")
                    {
                        whitelistOp.OperateType = 0; // Add
                        var whitelistData = commandData.GetValueOrDefault("whitelist_data", new object[0]);
                        if (whitelistData is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in jsonElement.EnumerateArray())
                            {
                                var itemDict = JsonSerializer.Deserialize<Dictionary<string, object>>(item.GetRawText());
                                whitelistOp.WhiteListData.Add(new WhiteListData
                                {
                                    Plate = itemDict?.GetValueOrDefault("plate", "").ToString() ?? "",
                                    Enable = int.Parse(itemDict?.GetValueOrDefault("enable", "1").ToString() ?? "1"),
                                    NeedAlarm = int.Parse(itemDict?.GetValueOrDefault("need_alarm", "0").ToString() ?? "0"),
                                    EnableTime = itemDict?.GetValueOrDefault("enable_time", "")?.ToString(),
                                    OverdueTime = itemDict?.GetValueOrDefault("overdue_time", "")?.ToString()
                                });
                            }
                        }
                    }

                    // Mark command as processed
                    command.IsProcessed = true;
                    command.ProcessedAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to process whitelist command {CommandId}", command.Id);
            }
        }

        await _context.SaveChangesAsync();
        return whitelistOp;
    }

    private async Task<object> GetPendingCommands(int siteId, int? deviceId)
    {
        if (deviceId == null)
        {
            return new { status = "ok", message = "No device found" };
        }

        // Get pending commands for this device
        var pendingCommands = await _context.CommandQueue
            .Where(c => c.DeviceId == deviceId && !c.IsProcessed)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync();

        var response = new ResponseAlarmInfoPlate
        {
            Info = "ok",
            PlateId = 0,
            ChannelNum = 0
        };

        // Check for whitelist commands first
        var whitelistCommands = pendingCommands
            .Where(c => c.CommandType!.StartsWith("whitelist_"))
            .Take(5)
            .ToList();

        if (whitelistCommands.Any())
        {
            response.WhiteListOperate = await BuildWhitelistOperations(whitelistCommands);

            // Process next sync step if needed
            if (deviceId.HasValue)
            {
                await _whitelistSyncService.ProcessNextSyncStep(deviceId.Value);
            }
        }

        // Process other command types
        foreach (var command in pendingCommands.Where(c => !c.CommandType!.StartsWith("whitelist_")))
        {
            switch (command.CommandType)
            {
                case "gate_open":
                    response.Info = "ok";
                    break;
                case "manual_trigger":
                    response.ManualTrigger = "ok";
                    break;
                case "screenshot":
                    if (command.CommandData != null)
                    {
                        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(command.CommandData.RootElement.GetRawText());
                        response.TriggerImage = new TriggerImageResponse
                        {
                            Port = int.Parse(data.GetValueOrDefault("port", "80").ToString() ?? "80"),
                            SnapImageAbsolutelyUrl = data.GetValueOrDefault("url", "")?.ToString()
                        };
                    }
                    break;
            }

            // Mark as processed
            command.IsProcessed = true;
            command.ProcessedAt = DateTime.UtcNow;
        }

        if (pendingCommands.Any())
        {
            await _context.SaveChangesAsync();
        }

        return new LprResponse { ResponseAlarmInfoPlate = response };
    }

    private async Task SaveResponseLog(int siteId, int? deviceId, string requestType, object request, object response, DateTime startTime)
    {
        try
        {
            var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            var responseLog = new ResponseLog
            {
                SiteId = siteId,
                DeviceId = deviceId,
                RequestType = requestType,
                RequestData = JsonDocument.Parse(JsonSerializer.Serialize(request)),
                ResponseData = JsonDocument.Parse(JsonSerializer.Serialize(response)),
                ProcessingTimeMs = processingTime,
                CreatedAt = DateTime.UtcNow
            };

            _context.ResponseLogs.Add(responseLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save response log for {RequestType}", requestType);
        }
    }
}
