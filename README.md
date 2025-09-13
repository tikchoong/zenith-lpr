# Zenith LPR - License Plate Recognition Webhook API

A .NET 8 Web API designed for receiving and logging HTTP webhook requests from LPR (License Plate Recognition) cameras. This application provides comprehensive logging capabilities for troubleshooting and analyzing LPR camera integrations.

## Features

- **MVC-based Web API** with .NET 8
- **Comprehensive Logging** with Serilog
  - Console output (human-readable)
  - File logging in dual formats:
    - Human-readable format (`logs/lpr-webhook-human-YYYYMMDD.log`)
    - JSON format (`logs/lpr-webhook-json-YYYYMMDD.log`)
- **3-day log rotation** with automatic cleanup
- **Network accessible** - Listens on all IP addresses
- **Swagger UI** for API testing and documentation
- **Error handling** for malformed JSON payloads
- **Request metadata logging** (IP, User-Agent, headers, timestamps)

## API Endpoints

### POST `/api/lpr/webhook`

Receives webhook requests from LPR cameras and logs all details.

**Request:**
- Method: `POST`
- Content-Type: `application/json`
- Body: Any valid JSON payload

**Response:**
```json
{
  "status": "success",
  "message": "LPR webhook received successfully",
  "timestamp": "2025-09-13T09:30:00.000Z",
  "receivedDataLength": 156
}
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Git

### Installation

1. Clone the repository:
```bash
git clone https://github.com/tikchoong/zenith-lpr.git
cd zenith-lpr
```

2. Navigate to the project directory:
```bash
cd LprWebhookApi
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Build the project:
```bash
dotnet build
```

5. Run the application:
```bash
dotnet run
```

The application will start and listen on:
- HTTP: `http://0.0.0.0:5174`
- HTTPS: `https://0.0.0.0:7214`

### Network Configuration

The application is configured to listen on all network interfaces (`0.0.0.0`), making it accessible from any device on your network.

To find your machine's IP address:
```bash
ifconfig | grep "inet " | grep -v 127.0.0.1
```

Configure your LPR camera to send webhooks to:
```
http://YOUR_MACHINE_IP:5174/api/lpr/webhook
```

## Usage

### Testing with Swagger UI

1. Open your browser and navigate to: `http://localhost:5174/swagger`
2. Expand the `/api/lpr/webhook` POST endpoint
3. Click "Try it out"
4. Enter your JSON payload in the request body
5. Click "Execute"

### Testing with curl

```bash
curl -X POST http://YOUR_MACHINE_IP:5174/api/lpr/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: LPR-Camera/1.0" \
  -d '{
    "license_plate": "ABC123",
    "confidence": 0.95,
    "timestamp": "2025-09-12T17:24:00Z",
    "camera_id": "CAM001",
    "location": {
      "latitude": 37.7749,
      "longitude": -122.4194
    }
  }'
```

## Logging

### Console Output
Real-time human-readable logs are displayed in the console:
```
[17:45:23 INF] === LPR Webhook Request Received ===
[17:45:23 INF] Remote IP: 192.168.1.100
[17:45:23 INF] User-Agent: LPR-Camera/1.0
[17:45:23 INF] Content-Type: application/json
[17:45:23 INF] === JSON Payload ===
[17:45:23 INF] Formatted JSON:
{
  "license_plate": "ABC123",
  "confidence": 0.95,
  "timestamp": "2025-09-12T17:24:00Z",
  "camera_id": "CAM001"
}
```

### Log Files
- **Human-readable**: `logs/lpr-webhook-human-YYYYMMDD.log`
- **JSON format**: `logs/lpr-webhook-json-YYYYMMDD.log`
- **Retention**: 3 days (automatic cleanup)

## Project Structure

```
LprWebhookApi/
├── Controllers/
│   └── LprController.cs          # Main webhook controller
├── Properties/
│   └── launchSettings.json       # Launch configuration
├── logs/                         # Auto-generated log files
├── Program.cs                    # Application configuration
├── LprWebhookApi.csproj         # Project file
└── appsettings.json             # Application settings
```

## Dependencies

- **Serilog.AspNetCore** - Logging framework
- **Serilog.Sinks.File** - File logging
- **Serilog.Formatting.Compact** - JSON log formatting
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please create an issue in the GitHub repository.
