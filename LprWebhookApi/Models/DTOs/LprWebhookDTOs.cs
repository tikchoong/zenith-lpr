using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace LprWebhookApi.Models.DTOs;

// Main LPR Recognition Request (from documentation)
public class AlarmInfoPlateRequest
{
    [JsonPropertyName("AlarmInfoPlate")]
    public AlarmInfoPlate AlarmInfoPlate { get; set; } = new();
}

public class AlarmInfoPlate
{
    [JsonPropertyName("channel")]
    public int Channel { get; set; }

    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("ipaddr")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public PlateResult Result { get; set; } = new();

    [JsonPropertyName("serialno")]
    public string SerialNumber { get; set; } = string.Empty;
}

public class PlateResult
{
    [JsonPropertyName("PlateResult")]
    public PlateRecognitionData PlateRecognitionData { get; set; } = new();
}

public class PlateRecognitionData
{
    [JsonPropertyName("license")]
    public string License { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public int Confidence { get; set; }

    [JsonPropertyName("colorType")]
    public int ColorType { get; set; }

    [JsonPropertyName("colorValue")]
    public int ColorValue { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("direction")]
    public int Direction { get; set; }

    [JsonPropertyName("bright")]
    public int Bright { get; set; }

    [JsonPropertyName("carBright")]
    public int CarBright { get; set; }

    [JsonPropertyName("carColor")]
    public int CarColor { get; set; }

    [JsonPropertyName("location")]
    public LocationData Location { get; set; } = new();

    [JsonPropertyName("timeStamp")]
    public TimeStampData TimeStamp { get; set; } = new();

    [JsonPropertyName("timeUsed")]
    public int TimeUsed { get; set; }

    [JsonPropertyName("triggerType")]
    public int TriggerType { get; set; }

    [JsonPropertyName("imagePath")]
    public string? ImagePath { get; set; }

    [JsonPropertyName("imageFile")]
    public string? ImageFile { get; set; }

    [JsonPropertyName("imageFileLen")]
    public int? ImageFileLen { get; set; }

    [JsonPropertyName("imageFragmentFile")]
    public string? ImageFragmentFile { get; set; }

    [JsonPropertyName("imageFragmentFileLen")]
    public int? ImageFragmentFileLen { get; set; }

    [JsonPropertyName("plateid")]
    public int PlateId { get; set; }

    [JsonPropertyName("isoffline")]
    public int IsOffline { get; set; }

    [JsonPropertyName("gioouts")]
    public List<GioOut> GioOuts { get; set; } = new();

    // Extended fields for Series cameras
    [JsonPropertyName("plate_true_width")]
    public int? PlateTrueWidth { get; set; }

    [JsonPropertyName("plate_distance")]
    public int? PlateDistance { get; set; }

    [JsonPropertyName("is_fake_plate")]
    public int? IsFakePlate { get; set; }

    [JsonPropertyName("car_location")]
    public LocationData? CarLocation { get; set; }

    [JsonPropertyName("car_brand")]
    public CarBrandData? CarBrand { get; set; }

    [JsonPropertyName("feature_Code")]
    public string? FeatureCode { get; set; }
}

public class LocationData
{
    [JsonPropertyName("RECT")]
    public RectData Rect { get; set; } = new();
}

public class RectData
{
    [JsonPropertyName("left")]
    public int Left { get; set; }

    [JsonPropertyName("right")]
    public int Right { get; set; }

    [JsonPropertyName("top")]
    public int Top { get; set; }

    [JsonPropertyName("bottom")]
    public int Bottom { get; set; }
}

public class TimeStampData
{
    [JsonPropertyName("Timeval")]
    public TimevalData Timeval { get; set; } = new();
}

public class TimevalData
{
    [JsonPropertyName("sec")]
    public long Sec { get; set; }

    [JsonPropertyName("usec")]
    public int Usec { get; set; }

    [JsonPropertyName("decyear")]
    public int DecYear { get; set; }

    [JsonPropertyName("decmon")]
    public int DecMonth { get; set; }

    [JsonPropertyName("decday")]
    public int DecDay { get; set; }

    [JsonPropertyName("dechour")]
    public int DecHour { get; set; }

    [JsonPropertyName("decmin")]
    public int DecMinute { get; set; }

    [JsonPropertyName("decsec")]
    public int DecSecond { get; set; }
}

public class GioOut
{
    [JsonPropertyName("ionum")]
    public int IoNum { get; set; }

    [JsonPropertyName("ctrltype")]
    public int CtrlType { get; set; }
}

public class CarBrandData
{
    [JsonPropertyName("brand")]
    public int Brand { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}

// Heartbeat Request (form-data format)
public class HeartbeatRequest
{
    [FromForm(Name = "device_name")]
    public string DeviceName { get; set; } = string.Empty;

    [FromForm(Name = "ipaddr")]
    public string IpAddress { get; set; } = string.Empty;

    [FromForm(Name = "port")]
    public string Port { get; set; } = string.Empty;

    [FromForm(Name = "user_name")]
    public string UserName { get; set; } = string.Empty;

    [FromForm(Name = "pass_wd")]
    public string Password { get; set; } = string.Empty;

    [FromForm(Name = "serialno")]
    public string SerialNumber { get; set; } = string.Empty;

    [FromForm(Name = "channel_num")]
    public string ChannelNum { get; set; } = string.Empty;
}

// IO Trigger Request
public class IoTriggerRequest
{
    [JsonPropertyName("AlarmGioIn")]
    public AlarmGioIn AlarmGioIn { get; set; } = new();
}

public class AlarmGioIn
{
    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("ipaddr")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public TriggerResultContainer Result { get; set; } = new();

    [JsonPropertyName("serialno")]
    public string SerialNumber { get; set; } = string.Empty;
}

public class TriggerResultContainer
{
    [JsonPropertyName("TriggerResult")]
    public TriggerResult TriggerResult { get; set; } = new();
}

public class TriggerResult
{
    [JsonPropertyName("source")]
    public int Source { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

// Serial Data Request
public class SerialDataRequest
{
    [JsonPropertyName("SerialData")]
    public SerialData SerialData { get; set; } = new();
}

public class SerialData
{
    [JsonPropertyName("channel")]
    public int Channel { get; set; }

    [JsonPropertyName("serialno")]
    public string SerialNumber { get; set; } = string.Empty;

    [JsonPropertyName("ipaddr")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("serialChannel")]
    public int SerialChannel { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("dataLen")]
    public int DataLen { get; set; }
}

// Screenshot Request
public class ScreenshotRequest
{
    [JsonPropertyName("ipaddr")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("TriggerImage")]
    public TriggerImage TriggerImage { get; set; } = new();
}

public class TriggerImage
{
    [JsonPropertyName("imageFile")]
    public string ImageFile { get; set; } = string.Empty;

    [JsonPropertyName("imageFileLen")]
    public int ImageFileLen { get; set; }
}
