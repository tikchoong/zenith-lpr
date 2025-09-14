using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.AspNetCore.Http;
using Serilog;

namespace LprWebhookApi.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            var method = context.Request.Method;
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
            var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var requestId = context.TraceIdentifier;

            const string requestMarker = "30--";
            const string responseMarker = "30**";

            // Console color properties (used only by console sink via output template)
            const string reqColor = "\u001b[36m"; // Cyan
            const string resColor = "\u001b[33m"; // Yellow
            const string colorReset = "\u001b[0m";

            // Log request start line
            Log.ForContext("ColorStart", reqColor)
               .ForContext("ColorReset", colorReset)
               .ForContext("RequestId", requestId)
               .Information("{Marker} HTTP {Method} {Path}{Query} from {RemoteIP}", requestMarker, method, path, query, remoteIp);

            // Optionally log JSON request body
            string? requestBody = null;
            var isJsonRequest = context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
            if (isJsonRequest && (context.Request.ContentLength ?? 0) > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    Log.ForContext("ColorStart", reqColor)
                       .ForContext("ColorReset", colorReset)
                       .ForContext("RequestId", requestId)
                       .Information("{Marker} Request JSON: {Body}", requestMarker, requestBody);
                }
            }

            // Capture response body for logging
            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log exception (both console and files)
                Log.ForContext("RequestId", requestId)
                   .Error(ex, "{Marker} Exception while processing {Method} {Path}{Query}", responseMarker, method, path, query);
                throw;
            }
            finally
            {
                sw.Stop();
                var statusCode = context.Response?.StatusCode;

                // Read response body if JSON
                string? responseBody = null;
                var isJsonResponse = context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
                memStream.Position = 0;
                if (isJsonResponse)
                {
                    using var respReader = new StreamReader(memStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                    responseBody = await respReader.ReadToEndAsync();
                    memStream.Position = 0;
                }

                // Copy the contents of the new memory stream (which contains the response) to the original stream.
                await memStream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;

                // Log response status line
                Log.ForContext("ColorStart", resColor)
                   .ForContext("ColorReset", colorReset)
                   .ForContext("RequestId", requestId)
                   .Information("{Marker} HTTP {Method} {Path}{Query} => {StatusCode} in {ElapsedMs:0.000} ms", responseMarker, method, path, query, statusCode, sw.Elapsed.TotalMilliseconds);

                // Log response JSON if available
                if (isJsonResponse && !string.IsNullOrWhiteSpace(responseBody))
                {
                    Log.ForContext("ColorStart", resColor)
                       .ForContext("ColorReset", colorReset)
                       .ForContext("RequestId", requestId)
                       .Information("{Marker} Response JSON: {Body}", responseMarker, responseBody);
                }
            }
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}

