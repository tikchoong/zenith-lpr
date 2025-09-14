# LPR Webhook API

A comprehensive License Plate Recognition (LPR) webhook API system with multi-site support, whitelist management, and advanced synchronization capabilities.

## Features

- **Multi-Site Architecture**: Support for multiple properties/sites with isolated data
- **Real-time LPR Processing**: Handle plate recognition results from LPR cameras
- **Advanced Whitelist Sync**: Robust synchronization system with progress tracking
- **Comet Polling Support**: Continuous communication with LPR devices
- **Comprehensive Logging**: Structured logging with Serilog
- **RESTful API**: Complete CRUD operations for all entities
- **Lookup Tables**: Centralized constants and enumerations

## Architecture

### Database Schema

The system uses PostgreSQL with Entity Framework Core and supports:

- **Sites**: Multi-tenant site management
- **Devices**: LPR camera device registration and management
- **Whitelists**: License plate whitelist/blacklist management
- **Entry Logs**: Complete audit trail of all entry attempts
- **Command Queue**: Asynchronous command processing
- **Lookup Tables**: System constants and enumerations

### Key Components

1. **LprWebhookController**: Main webhook endpoint for LPR camera communication
2. **WhitelistSyncService**: Advanced whitelist synchronization engine
3. **WhitelistSyncController**: Management API for sync operations
4. **LookupController**: API for system constants and dropdowns

## Whitelist Synchronization System

### Overview

The whitelist sync system ensures LPR cameras maintain synchronized whitelist data with the server using a robust, progress-tracked approach.

### Sync Process Flow

```
1. Admin triggers sync → WhitelistStartSync = true
2. Next communication (poll/recognition) detects flag
3. System calculates total batches needed (5 entries per batch)
4. Status = "clearing" → Send clear all command
5. Status = "adding" → Send batches of whitelist entries
6. Track progress: batches_sent / total_batches
7. When complete → Status = "completed", flag = false
```

### Sync States

- **idle**: No sync in progress
- **clearing**: Clearing existing whitelist entries
- **adding**: Adding new whitelist entries in batches
- **completed**: Sync successfully completed
- **failed**: Sync failed or timed out

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

Note: HTTPS is disabled in development by default. You can enable it by configuring Kestrel with UseHttps() in Program.cs.

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
2. Expand the LprWebhookController endpoints, e.g. `POST /api/lpr/sites/{siteCode}/webhook/plate-recognition`
3. Click "Try it out"
4. Enter your JSON payload in the request body
5. Click "Execute"

### Testing with curl

```bash
curl -X POST "http://YOUR_MACHINE_IP:5174/api/lpr/sites/pavalionmallsite1/webhook/plate-recognition" \
  -H "Content-Type: application/json" \
  -H "User-Agent: LPR-Camera/1.0" \
  -d '{
    "license_plate": "ABC123",
    "confidence": 0.95,
    "timestamp": "2025-09-12T17:24:00Z",
    "camera_id": "CAM001"
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
- **Retention**: 7 days (automatic cleanup)
- Console request/response markers: request lines start with `30--`, response lines start with `30**`
- Console colors: requests in cyan, responses in yellow

## Project Structure

```
LprWebhookApi/
├── Controllers/
│   └── LprWebhookController.cs   # Main webhook controller
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

### Database Fields

The `devices` table includes these sync-related fields:

```sql
whitelist_start_sync         BOOLEAN   -- Trigger flag
whitelist_sync_started_at    TIMESTAMP -- When sync began
whitelist_sync_batches_sent  INTEGER   -- Progress tracking
whitelist_sync_total_batches INTEGER   -- Total batches needed
whitelist_sync_status        VARCHAR   -- Current sync state
```

### Whitelist Sync API Endpoints

#### Trigger Sync for Single Device

```http
POST /api/lpr/sites/{siteCode}/devices/{deviceId}/sync-whitelist
```

#### Get Sync Status for Single Device

```http
GET /api/lpr/sites/{siteCode}/devices/{deviceId}/sync-status
```

Response:

```json
{
  "deviceId": 1,
  "syncInProgress": true,
  "status": "adding",
  "startedAt": "2025-09-14T10:30:00Z",
  "batchesSent": 15,
  "totalBatches": 20,
  "progress": 75.0
}
```

#### Cancel Ongoing Sync

```http
POST /api/lpr/sites/{siteCode}/devices/{deviceId}/cancel-sync
```

#### Get Sync Status for All Devices in Site

```http
GET /api/lpr/sites/{siteCode}/devices/sync-status
```

<!-- Bulk trigger endpoint removed by design; use per-device trigger instead. -->

### Sync Features

#### Progress Tracking

- Real-time progress monitoring (batches sent / total batches)
- Detailed status information for admin interfaces
- Timestamp tracking for sync duration analysis

#### Timeout Protection

- Automatic failure detection after 30 minutes
- Prevents stuck sync states
- Automatic cleanup of failed syncs

#### Race Condition Prevention

- Only one sync per device at a time
- Proper state management prevents conflicts
- Thread-safe operations

#### Failure Recovery

- Failed syncs can be retried by triggering new sync
- Clear error states and logging
- Automatic state reset on new sync

## Lookup Tables System

### Overview

Centralized system for managing constants, enumerations, and dropdown values used throughout the LPR system.

### Available Categories

1. **entry_type**: tenant, visitor, staff, contractor, delivery, emergency, temporary
2. **recurring_pattern**: daily, weekdays, weekends, weekly, monthly, custom
3. **entry_status**: allowed, denied, expired, exceeded_limit, blacklisted, time_restricted
4. **color_type**: white, black, red, blue, yellow, green, gray, silver, brown, other
5. **direction**: unknown, left_to_right, right_to_left, top_to_bottom, bottom_to_top
6. **trigger_type**: auto, manual, external, scheduled
7. **vehicle_type**: unknown, car, suv, truck, van, motorcycle, bus, trailer
8. **device_status**: online, offline, maintenance, error
9. **command_type**: open_gate, close_gate, reboot, update_firmware, whitelist_add, etc.

### Lookup API Endpoints

```http
GET /api/lpr/lookup/entry-types        # Get all entry types
GET /api/lpr/lookup/recurring-patterns # Get all recurring patterns
GET /api/lpr/lookup/color-types        # Get all color types
GET /api/lpr/lookup/categories         # Get all available categories
GET /api/lpr/lookup/{category}         # Get lookups by category
GET /api/lpr/lookup/all               # Get all lookups grouped by category
```

## Main Webhook Endpoints

```http
POST /api/lpr/sites/{siteCode}/webhook/plate-recognition  # Plate recognition results
POST /api/lpr/sites/{siteCode}/webhook/heartbeat         # Device heartbeat
POST /api/lpr/sites/{siteCode}/webhook/comet-poll        # Comet polling
POST /api/lpr/sites/{siteCode}/webhook/io-trigger        # IO trigger events
```

## Management Endpoints

```http
GET    /api/lpr/sites/{siteCode}/whitelists              # List whitelists
POST   /api/lpr/sites/{siteCode}/whitelists              # Create whitelist
PUT    /api/lpr/sites/{siteCode}/whitelists/{id}         # Update whitelist
DELETE /api/lpr/sites/{siteCode}/whitelists/{id}         # Delete whitelist
```

## Configuration

## Controllers and Endpoints Overview

Below is a concise map of controllers and their key routes. See Swagger at /swagger for full request/response models.

- SitesController

  - GET /api/lpr/sites
  - GET /api/lpr/sites/{siteCode}
  - GET /api/lpr/sites/{siteCode}/statistics

- DashboardController

  - GET /api/lpr/sites/{siteCode}/dashboard/overview
  - GET /api/lpr/sites/{siteCode}/dashboard/device-status
  - GET /api/lpr/sites/{siteCode}/dashboard/activity-chart
  - GET /api/lpr/sites/{siteCode}/dashboard/top-vehicles
  - GET /api/lpr/sites/{siteCode}/dashboard/alerts

- DevicesController and WhitelistSyncController

  - GET /api/lpr/sites/{siteCode}/devices
  - GET /api/lpr/sites/{siteCode}/devices/{deviceId}
  - GET /api/lpr/sites/{siteCode}/devices/{deviceId}/status
  - GET /api/lpr/sites/{siteCode}/devices/{deviceId}/commands
  - POST /api/lpr/sites/{siteCode}/devices/{deviceId}/sync-whitelist (trigger per-device sync)
  - POST /api/lpr/sites/{siteCode}/devices/{deviceId}/cancel-sync
  - GET /api/lpr/sites/{siteCode}/devices/{deviceId}/sync-status
  - GET /api/lpr/sites/{siteCode}/devices/sync-status (site-wide status snapshot)
  - Note: the bulk trigger endpoint for all devices has been removed by design.

- LprWebhookController (device → server)

  - POST /api/lpr/sites/{siteCode}/webhook/plate-recognition
  - POST /api/lpr/sites/{siteCode}/webhook/heartbeat
  - POST /api/lpr/sites/{siteCode}/webhook/comet-poll
  - POST /api/lpr/sites/{siteCode}/webhook/io-trigger
  - POST /api/lpr/sites/{siteCode}/webhook/screenshot
  - POST /api/lpr/sites/{siteCode}/webhook/serial-data

- WhitelistsController (admin → server)

  - GET, POST /api/lpr/sites/{siteCode}/whitelists
  - GET, PUT, DELETE /api/lpr/sites/{siteCode}/whitelists/{whitelistId}
  - POST /api/lpr/sites/{siteCode}/whitelists/{whitelistId}/enable
  - POST /api/lpr/sites/{siteCode}/whitelists/{whitelistId}/disable

- EntryLogsController

  - GET /api/lpr/sites/{siteCode}/entry-logs
  - GET /api/lpr/sites/{siteCode}/entry-logs/recent
  - GET /api/lpr/sites/{siteCode}/entry-logs/statistics
  - GET /api/lpr/sites/{siteCode}/entry-logs/export
  - GET /api/lpr/sites/{siteCode}/entry-logs/{entryLogId}

- TenantsController

  - GET /api/lpr/sites/{siteCode}/tenants
  - GET /api/lpr/sites/{siteCode}/tenants/{tenantId}
  - GET /api/lpr/sites/{siteCode}/tenants/{tenantId}/entry-logs
  - GET /api/lpr/sites/{siteCode}/tenants/{tenantId}/whitelists

- LookupController

  - GET /api/lpr/lookup/categories
  - GET /api/lpr/lookup/{category}
  - GET /api/lpr/lookup/all
  - GET /api/lpr/lookup/entry-types
  - GET /api/lpr/lookup/recurring-patterns
  - GET /api/lpr/lookup/color-types
  - GET /api/lpr/lookup/directions
  - GET /api/lpr/lookup/trigger-types
  - GET /api/lpr/lookup/vehicle-types
  - GET /api/lpr/lookup/entry-statuses

- TestController
  - GET /api/test
  - GET /api/test/hello

### Database: Command Queue

Command queue persists device commands generated by the server during sync and operations. Key columns:

- site_id (FK → sites.id) — required
- device_id (FK → devices.id) — required
- command_type (varchar) — e.g., whitelist_clear, whitelist_add_batch
- command_data (jsonb) — payload (e.g., whitelist_data array)
- priority (int)
- is_processed (bool)
- created_at, processed_at (timestamps)

Note: Commands created by WhitelistSyncService always include both site_id and device_id.

### Database Connection

Update the connection string in `Program.cs`:

```csharp
builder.Services.AddDbContext<LprDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=lpr_webhook;Username=postgres;Password=postgres"));
```

### Logging

Logs are written to:

- Console: Real-time structured output
- `logs/lpr-webhook-human-*.log`: Human-readable format
- `logs/lpr-webhook-json-*.log`: JSON format for log aggregation

## Development

### Adding New Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Testing

The system includes comprehensive logging for debugging and monitoring. Check the logs directory for detailed request/response information.

For issues and questions, please create an issue in the GitHub repository.
