using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Serilog;

namespace LprWebhookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LprController : ControllerBase
{
    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true
    };

    [HttpPost("webhook")]
    public IActionResult Webhook([FromBody] object jsonPayload)
    {
        try
        {
            // Log request details
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            var contentType = HttpContext.Request.ContentType ?? "Unknown";
            var contentLength = HttpContext.Request.ContentLength ?? 0;

            Log.Information("=== LPR Webhook Request Received ===");
            Log.Information("Remote IP: {RemoteIP}", remoteIp);
            Log.Information("User-Agent: {UserAgent}", userAgent);
            Log.Information("Content-Type: {ContentType}", contentType);
            Log.Information("Content-Length: {ContentLength}", contentLength);
            Log.Information("Timestamp: {Timestamp}", DateTime.UtcNow);

            // Log all headers
            Log.Information("=== Request Headers ===");
            foreach (var header in HttpContext.Request.Headers)
            {
                Log.Information("Header: {HeaderName} = {HeaderValue}", header.Key, string.Join(", ", header.Value));
            }

            // Convert the object back to JSON string for logging
            string jsonPayloadString = JsonSerializer.Serialize(jsonPayload);

            Log.Information("=== JSON Payload ===");
            Log.Information("Raw JSON: {JsonPayload}", jsonPayloadString);

            // Try to parse and pretty-print the JSON
            try
            {
                if (jsonPayload != null)
                {
                    var prettyJson = JsonSerializer.Serialize(jsonPayload, PrettyJsonOptions);
                    Log.Information("Formatted JSON:\n{FormattedJson}", prettyJson);

                    // Log the JSON payload as structured data for JSON formatters
                    Log.Information("Structured JSON: {@JsonData}", jsonPayload);
                }
                else
                {
                    Log.Warning("Empty or null JSON payload received");
                }
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Failed to parse JSON payload: {JsonPayload}", jsonPayloadString);
            }

            Log.Information("=== End of LPR Webhook Request ===");

            // Return success response
            return Ok(new
            {
                status = "success",
                message = "LPR webhook received successfully",
                timestamp = DateTime.UtcNow,
                receivedDataLength = jsonPayloadString?.Length ?? 0
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing LPR webhook request");
            return Problem("Internal server error processing webhook");
        }
    }
}
