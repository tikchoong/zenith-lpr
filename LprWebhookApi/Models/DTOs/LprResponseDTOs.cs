using System.Text.Json.Serialization;

namespace LprWebhookApi.Models.DTOs;

// Main response to LPR camera (from documentation)
public class LprResponse
{
    [JsonPropertyName("Response_AlarmInfoPlate")]
    public ResponseAlarmInfoPlate ResponseAlarmInfoPlate { get; set; } = new();
}

public class ResponseAlarmInfoPlate
{
    [JsonPropertyName("info")]
    public string Info { get; set; } = "ok"; // "ok" to open gate

    [JsonPropertyName("plateid")]
    public int PlateId { get; set; }

    [JsonPropertyName("channelNum")]
    public int ChannelNum { get; set; } = 0;

    [JsonPropertyName("manualTrigger")]
    public string? ManualTrigger { get; set; } // "ok" for manual triggering

    [JsonPropertyName("TriggerImage")]
    public TriggerImageResponse? TriggerImage { get; set; }

    [JsonPropertyName("is_pay")]
    public string? IsPay { get; set; } // "true" for payment confirmation

    [JsonPropertyName("serialData")]
    public List<SerialDataResponse>? SerialData { get; set; }

    [JsonPropertyName("white_list_operate")]
    public WhiteListOperate? WhiteListOperate { get; set; }

    [JsonPropertyName("ContinuePushOffline")]
    public ContinuePushOffline? ContinuePushOffline { get; set; }
}

public class TriggerImageResponse
{
    [JsonPropertyName("port")]
    public int Port { get; set; } = 80;

    [JsonPropertyName("snapImageRelativeUrl")]
    public string? SnapImageRelativeUrl { get; set; }

    [JsonPropertyName("snapImageAbsolutelyUrl")]
    public string? SnapImageAbsolutelyUrl { get; set; }
}

public class SerialDataResponse
{
    [JsonPropertyName("serialChannel")]
    public int SerialChannel { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("dataLen")]
    public int DataLen { get; set; }
}

public class WhiteListOperate
{
    [JsonPropertyName("operate_type")]
    public int OperateType { get; set; } // 0: add, 1: delete

    [JsonPropertyName("white_list_data")]
    public List<WhiteListData> WhiteListData { get; set; } = new();
}

public class WhiteListData
{
    [JsonPropertyName("plate")]
    public string Plate { get; set; } = string.Empty;

    [JsonPropertyName("enable")]
    public int Enable { get; set; } = 1; // 0: invalid, 1: valid

    [JsonPropertyName("need_alarm")]
    public int NeedAlarm { get; set; } = 0; // 0: whitelist, 1: blacklist

    [JsonPropertyName("enable_time")]
    public string? EnableTime { get; set; } // "2018-01-01 11:11:11"

    [JsonPropertyName("overdue_time")]
    public string? OverdueTime { get; set; } // "2018-01-01 11:11:11"
}

public class ContinuePushOffline
{
    [JsonPropertyName("plateid")]
    public int PlateId { get; set; }

    [JsonPropertyName("continue")]
    public int Continue { get; set; } // 0: No, 1: Yes
}

// Management API DTOs
public class CreateSiteRequest
{
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Malaysia";
    public string? SiteManagerName { get; set; }
    public string? SiteManagerPhone { get; set; }
    public string? SiteManagerEmail { get; set; }
}

public class UpdateSiteRequest
{
    public string SiteName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Malaysia";
    public string? SiteManagerName { get; set; }
    public string? SiteManagerPhone { get; set; }
    public string? SiteManagerEmail { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SiteResponse
{
    public int Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? SiteManagerName { get; set; }
    public string? SiteManagerPhone { get; set; }
    public string? SiteManagerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDeviceRequest
{
    public string SerialNumber { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? LocationDescription { get; set; }
}

public class UpdateDeviceRequest
{
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? LocationDescription { get; set; }
}

public class DeviceResponse
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? LocationDescription { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public string? FirmwareVersion { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantRequest
{
    public string? TenantCode { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? UnitNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateOnly? MoveInDate { get; set; }
}

public class UpdateTenantRequest
{
    public string TenantName { get; set; } = string.Empty;
    public string? UnitNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateOnly? MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TenantResponse
{
    public int Id { get; set; }
    public string? TenantCode { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? UnitNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateOnly? MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWhitelistRequest
{
    public int? DeviceId { get; set; } // NULL = all devices
    public int? TenantId { get; set; }
    public int? StaffId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string EntryType { get; set; } = "tenant"; // 'tenant', 'visitor', 'staff', 'temporary'
    public bool IsEnabled { get; set; } = true;
    public bool IsBlacklist { get; set; } = false;
    
    // Visitor fields
    public string? VisitorName { get; set; }
    public string? VisitorPhone { get; set; }
    public string? VisitorCompany { get; set; }
    public string? VisitPurpose { get; set; }
    
    // Time control
    public DateTime? EnableTime { get; set; }
    public DateTime? ExpiryTime { get; set; }
    public int? MaxEntries { get; set; }
    
    // Recurring access
    public bool IsRecurring { get; set; } = false;
    public string? RecurringPattern { get; set; }
    public TimeOnly? RecurringStartTime { get; set; }
    public TimeOnly? RecurringEndTime { get; set; }
    
    public string? Notes { get; set; }
}

public class WhitelistResponse
{
    public int Id { get; set; }
    public int? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsBlacklist { get; set; }
    public string? VisitorName { get; set; }
    public string? VisitorPhone { get; set; }
    public string? VisitorCompany { get; set; }
    public string? VisitPurpose { get; set; }
    public DateTime? EnableTime { get; set; }
    public DateTime? ExpiryTime { get; set; }
    public int? MaxEntries { get; set; }
    public int CurrentEntries { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurringPattern { get; set; }
    public TimeOnly? RecurringStartTime { get; set; }
    public TimeOnly? RecurringEndTime { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
