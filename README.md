# LPR Webhook API

A comprehensive License Plate Recognition (LPR) webhook API system with multi-site support, whitelist management, screenshot capture, device management, and advanced synchronization capabilities.

## Features

- **Multi-Site Architecture**: Support for multiple properties/sites with isolated data
- **Real-time LPR Processing**: Handle plate recognition results from LPR cameras
- **Advanced Whitelist Sync**: Robust synchronization system with progress tracking
- **Screenshot Capture System**: Automatic screenshot capture and storage with image management
- **Device Management**: Comprehensive device monitoring, status tracking, and feature control
- **Gate Control Integration**: Automated entry/exit gate control based on recognition results
- **Comet Polling Support**: Continuous communication with LPR devices
- **Comprehensive Logging**: Structured logging with Serilog
- **RESTful API**: Complete CRUD operations for all entities
- **Lookup Tables**: Centralized constants and enumerations
- **Dashboard & Analytics**: Real-time monitoring and reporting capabilities

## Architecture

### Database Schema

The system uses PostgreSQL with Entity Framework Core and supports:

- **Sites**: Multi-tenant site management with isolated data
- **Devices**: LPR camera device registration, management, and feature control
- **Whitelists**: License plate whitelist/blacklist management with tenant associations
- **Entry Logs**: Complete audit trail of all entry attempts with gate control results
- **Plate Recognition Results**: Raw LPR camera data with confidence scores and metadata
- **Screenshots**: Image capture system linked to plate recognition events
- **Command Queue**: Asynchronous command processing for device operations
- **Device Heartbeats**: Real-time device status monitoring and connectivity tracking
- **Lookup Tables**: System constants, enumerations, and dropdown values
- **Site Configurations**: Per-site settings and customizations

### Key Components

1. **LprWebhookController**: Main webhook endpoint for LPR camera communication
2. **ScreenshotsController**: Image management and retrieval API
3. **DeviceManagementController**: Device status monitoring and feature control
4. **WhitelistSyncService**: Advanced whitelist synchronization engine
5. **ScreenshotService**: Screenshot capture, storage, and retrieval service
6. **WhitelistSyncController**: Management API for sync operations
7. **DashboardController**: Real-time analytics and monitoring
8. **LookupController**: API for system constants and dropdowns

## Screenshot Capture System

### Overview

The screenshot capture system automatically captures and stores images when license plates are recognized, providing visual evidence and audit trails for all entry attempts.

### Key Features

- **Automatic Capture**: Screenshots are captured automatically when plates are recognized
- **Image Storage**: Base64-encoded images stored in PostgreSQL with metadata
- **Device-Level Control**: Screenshot capture can be enabled/disabled per device
- **Recognition Context**: Screenshots linked to plate recognition results and entry decisions
- **Multiple Formats**: Support for JPEG and PNG image formats
- **API Access**: RESTful endpoints for image retrieval, display, and download

### Screenshot Process Flow

```
1. License plate recognized → Check device screenshot settings
2. If enabled → Extract image data from recognition request
3. Save screenshot with metadata → Link to plate recognition result
4. Update device status → Track capture statistics
5. API endpoints available → Image display, download, metadata
```

### Screenshot Database Schema

**plate_recognition_screenshots table:**

- `id`: Primary key
- `plate_recognition_id`: Link to recognition event
- `site_id`, `device_id`: Multi-tenant isolation
- `image_base64`: Base64-encoded image data
- `image_length`: Image size in bytes
- `license_plate`: Recognized plate number
- `recognition_result`: Entry decision (allowed/denied)
- `screenshot_status`: Processing status
- `requested_at`, `received_at`: Timing metadata
- `camera_ip`: Source device IP
- `trigger_source`: Capture trigger type

## Gate Control System

### Overview

The gate control system automatically manages entry/exit gates based on license plate recognition results, providing real-time access control with immediate response to LPR cameras.

### Gate Control Process Flow

```
1. License Plate Recognized → Whitelist Check → Entry Decision
2. Entry Decision Made → Gate Control Response
3. Response Sent to Camera → Gate Opens/Remains Closed
4. Entry Log Created → Audit Trail Maintained
```

### Entry Decision Logic

**Whitelist Match Found:**

- ✅ **Allowed**: `entryStatus = "allowed"`, `gateOpened = true`
- ❌ **Blacklist**: `entryStatus = "allowed"`, `gateOpened = false` (if `IsBlacklist = true`)
- 📊 **Entry Count**: Increments `CurrentEntries` for tracking

**No Whitelist Match:**

- ❌ **Denied**: `entryStatus = "denied"`, `gateOpened = false`

### Response Format to Camera

**Gate Open Response:**

```json
{
  "Response_AlarmInfoPlate": {
    "info": "ok", // Gate opens
    "plateId": 12345,
    "channelNum": 0
  }
}
```

**Gate Deny Response:**

```json
{
  "Response_AlarmInfoPlate": {
    "info": "denied", // Gate remains closed
    "plateId": 12345,
    "channelNum": 0
  }
}
```

### Entry Log Database Record

Each recognition creates an audit trail:

- **License Plate**: Recognized plate number
- **Entry Status**: "allowed" or "denied"
- **Entry Type**: "tenant", "visitor", "staff", "unknown"
- **Gate Opened**: Boolean flag for actual gate action
- **Confidence**: Recognition confidence score
- **Whitelist Reference**: Link to matching whitelist entry (if any)
- **Timestamps**: Entry time and processing time

## Device Heartbeat & Online Status Tracking

### Overview

The heartbeat system monitors device connectivity and health, ensuring real-time status tracking and automatic offline detection.

### Heartbeat Types

**1. Normal Heartbeat (`/heartbeat`):**

- Periodic device status updates
- Includes device credentials and channel info
- Updates `IsOnline = true` and `LastHeartbeat` timestamp

**2. Comet Polling (`/comet-poll`):**

- Continuous communication for command retrieval
- Long-polling mechanism for real-time commands
- Same status updates as normal heartbeat

### Device Status Tracking

**Online Status Determination:**

- **Online**: Recent heartbeat within expected interval
- **Offline**: No heartbeat for extended period
- **Uptime Calculation**: `DateTime.UtcNow - LastHeartbeat`

**Status Information Tracked:**

- `IsOnline`: Boolean online/offline status
- `LastHeartbeat`: UTC timestamp of last communication
- `FirmwareVersion`: Device firmware information
- `UptimeMinutes`: Calculated uptime duration

### Heartbeat Database Records

**device_heartbeats table:**

- `site_id`, `device_id`: Multi-tenant device identification
- `heartbeat_type`: "normal" or "comet"
- `user_name`, `password`: Device authentication credentials
- `channel_num`: Device channel information
- `received_at`: Server timestamp of heartbeat receipt

### Comet Polling vs. Heartbeat: Key Differences

| Aspect              | Heartbeat (`/heartbeat`)       | Comet Polling (`/comet-poll`) |
| ------------------- | ------------------------------ | ----------------------------- |
| **Purpose**         | Device status monitoring       | Real-time command delivery    |
| **Frequency**       | Periodic (every 30-60 seconds) | Continuous (long-polling)     |
| **Response Time**   | Standard HTTP response         | May hold connection longer    |
| **Primary Use**     | Health monitoring              | Command synchronization       |
| **Connection Type** | Short-lived request/response   | Long-polling connection       |
| **Data Priority**   | Status updates                 | Command delivery              |

### Detailed Comparison

**Heartbeat Communication:**

- **Function**: Primary purpose is device health monitoring
- **Timing**: Sent at regular intervals (typically 30-60 seconds)
- **Response**: Quick acknowledgment with basic status
- **Use Case**: "Is the device online and functioning?"
- **Connection**: Standard HTTP request/response cycle
- **Data**: Device credentials, channel info, basic status

**Comet Polling Communication:**

- **Function**: Real-time command delivery and business logic
- **Timing**: Continuous or frequent polling for immediate response
- **Response**: May include complex commands and operations
- **Use Case**: "Are there any commands waiting for me?"
- **Connection**: Long-polling technique for near real-time communication
- **Data**: Commands, whitelist operations, gate controls, screenshots

### When Each is Used

**Heartbeat is used for:**

- ✅ Device registration and identification
- ✅ Online/offline status tracking
- ✅ Basic connectivity verification
- ✅ Firmware version reporting
- ✅ Network health monitoring

**Comet Polling is used for:**

- ✅ Whitelist synchronization commands
- ✅ Gate control operations
- ✅ Screenshot capture requests
- ✅ Manual trigger commands
- ✅ Emergency override operations
- ✅ Real-time business logic execution

### Technical Implementation

**Heartbeat Endpoint:**

```http
POST /api/lpr/sites/{siteCode}/webhook/heartbeat
Content-Type: application/x-www-form-urlencoded

serialNumber=device123&userName=admin&password=pass&channelNum=0
```

**Comet Poll Endpoint:**

```http
POST /api/lpr/sites/{siteCode}/webhook/comet-poll
Content-Type: application/x-www-form-urlencoded

serialNumber=device123&userName=admin&password=pass&channelNum=0
```

### Response Differences

**Heartbeat Response (Simple):**

```json
{
  "Response_AlarmInfoPlate": {
    "info": "ok",
    "plateId": 0,
    "channelNum": 0
  }
}
```

**Comet Poll Response (With Commands):**

```json
{
  "Response_AlarmInfoPlate": {
    "info": "ok",
    "plateId": 0,
    "channelNum": 0,
    "white_list_operate": {
      "operate_type": 0,
      "white_list_data": [
        {
          "plate": "ABC123",
          "enable": 1,
          "need_alarm": 0
        }
      ]
    },
    "triggerImage": {
      "port": 80,
      "snapImageAbsolutelyUrl": "http://192.168.1.1:5174/api/screenshot"
    }
  }
}
```

### Best Practices

**For Device Manufacturers:**

- Use **heartbeat** for regular status updates
- Use **comet polling** for command retrieval
- Implement both endpoints for full functionality
- Handle long-polling timeouts gracefully

**For System Administrators:**

- Monitor heartbeat frequency for device health
- Use comet polling for urgent command delivery
- Configure appropriate timeout values
- Balance polling frequency with network load

## Command Queue System

### Overview

The command queue system enables server-to-device communication through heartbeat and comet polling responses, supporting various device operations.

### Command Types Supported

**1. Gate Control Commands:**

```json
{
  "command_type": "gate_open",
  "command_data": {},
  "priority": 1
}
```

**2. Whitelist Synchronization:**

```json
{
  "command_type": "whitelist_clear",
  "command_data": { "plate": "" },
  "priority": 1
}
```

```json
{
  "command_type": "whitelist_add_batch",
  "command_data": {
    "whitelist_data": [
      {
        "plate": "ABC123",
        "enable": 1,
        "need_alarm": 0,
        "enable_time": "2025-09-15 00:00:00",
        "overdue_time": "2025-12-31 23:59:59"
      }
    ]
  },
  "priority": 1
}
```

**3. Screenshot Capture:**

```json
{
  "command_type": "screenshot",
  "command_data": {
    "port": 80,
    "url": "http://192.168.1.1:5174/api/lpr/sites/site1/webhook/screenshot"
  },
  "priority": 1
}
```

**4. Manual Trigger:**

```json
{
  "command_type": "manual_trigger",
  "command_data": {},
  "priority": 1
}
```

### Command Processing Flow

```
1. Command Queued → Database storage with priority
2. Device Heartbeat/Poll → Server checks pending commands
3. Commands Retrieved → Up to 5 commands per response
4. Response Sent → Commands included in heartbeat response
5. Commands Marked → `is_processed = true`, `processed_at = timestamp`
```

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

### Whitelist Operations

**1. Clear All Whitelist Entries:**

```json
{
  "Response_AlarmInfoPlate": {
    "white_list_operate": {
      "operate_type": 1,
      "white_list_data": [
        {
          "plate": "" // Empty plate clears all entries
        }
      ]
    }
  }
}
```

**2. Add Batch of 5 Entries:**

```json
{
  "Response_AlarmInfoPlate": {
    "white_list_operate": {
      "operate_type": 0,
      "white_list_data": [
        {
          "plate": "ABC123",
          "enable": 1,
          "need_alarm": 0,
          "enable_time": "2025-09-15 00:00:00",
          "overdue_time": "2025-12-31 23:59:59"
        },
        {
          "plate": "DEF456",
          "enable": 1,
          "need_alarm": 0,
          "enable_time": "2025-09-15 00:00:00",
          "overdue_time": "2025-12-31 23:59:59"
        }
        // ... up to 5 entries per batch
      ]
    }
  }
}
```

**3. Remove Specific Entries:**

```json
{
  "Response_AlarmInfoPlate": {
    "white_list_operate": {
      "operate_type": 1,
      "white_list_data": [{ "plate": "ABC123" }, { "plate": "DEF456" }]
    }
  }
}
```

### Sync States

- **idle**: No sync in progress
- **clearing**: Clearing existing whitelist entries
- **adding**: Adding new whitelist entries in batches
- **completed**: Sync successfully completed
- **failed**: Sync failed or timed out

### Batch Processing Details

- **Batch Size**: 5 entries per batch (configurable)
- **Processing Order**: Priority-based command queue
- **Progress Tracking**: `batches_sent / total_batches`
- **Error Handling**: Automatic retry and failure detection
- **Completion**: All batches sent and acknowledged

## RS485 Serial Data Communication

### Overview

The RS485 serial communication system enables bidirectional data exchange between LPR cameras and external devices (access control panels, gate controllers, etc.).

### Serial Data Endpoint

**Webhook URL:** `POST /api/lpr/sites/{siteCode}/webhook/serial-data`

### Request Format

```json
{
  "SerialData": {
    "channel": 0,
    "serialno": "cead13eb-1a198cd7",
    "ipaddr": "192.168.1.100",
    "deviceName": "IVS",
    "serialChannel": 0,
    "data": "Y2guY29tFw==",
    "dataLen": 7
  }
}
```

### Field Descriptions

- **channel**: Camera channel number (currently 0)
- **serialno**: Device serial number for identification
- **ipaddr**: Device IP address
- **deviceName**: Device name/model
- **serialChannel**: Serial port channel (0 = RS485, 1 = RS232)
- **data**: Base64-encoded serial data
- **dataLen**: Actual length of serial data in bytes

### Serial Data Processing

**1. Data Reception:**

- Validates device and site information
- Decodes Base64 serial data
- Stores in `serial_data_logs` table

**2. Database Storage:**

```sql
CREATE TABLE serial_data_logs (
    id SERIAL PRIMARY KEY,
    site_id INTEGER NOT NULL,
    device_id INTEGER NOT NULL,
    serial_channel INTEGER,
    data_base64 TEXT,
    data_length INTEGER,
    received_at TIMESTAMP WITH TIME ZONE
);
```

**3. Response:**

```json
{
  "status": "ok",
  "message": "Serial data received"
}
```

### Use Cases

**Access Control Integration:**

- Gate controller commands
- Card reader data
- Sensor status updates
- Emergency override signals

**External System Communication:**

- Building management systems
- Security panels
- Parking management
- Visitor management systems

### Serial Channel Configuration

- **Channel 0 (RS485)**: Multi-drop communication bus
- **Channel 1 (RS232/RS485)**: Point-to-point or jumper-configured
- **Baud Rate**: Configurable on device
- **Data Format**: 8N1 (8 data bits, no parity, 1 stop bit)

## Complete System Workflow

### License Plate Recognition Flow

```
1. 📷 Camera detects vehicle → License plate recognized
2. 📡 HTTP POST to /webhook/plate-recognition → Server receives data
3. 🔍 Whitelist lookup → Check if plate is authorized
4. ⚖️ Entry decision → Allow/Deny based on whitelist status
5. 🚪 Gate control → Send "ok" or "denied" response
6. 📊 Audit logging → Create entry log with full details
7. 📸 Screenshot capture → Save image if enabled
8. 📋 Command processing → Include any pending commands in response
```

### Device Communication Cycle

```
1. 💓 Heartbeat/Comet Poll → Device contacts server periodically
2. 🔄 Status update → Update device online status and timestamp
3. 📋 Command check → Server checks for pending commands
4. 📤 Command delivery → Include commands in response
5. ✅ Command execution → Device processes commands
6. 🔁 Repeat cycle → Continuous communication loop
```

### Whitelist Synchronization Workflow

```
1. 👨‍💼 Admin triggers sync → Set WhitelistStartSync = true
2. 📡 Next device communication → Heartbeat or plate recognition
3. 🧹 Clear phase → Send "whitelist_clear" command
4. 📦 Batch phase → Send batches of 5 entries each
5. 📊 Progress tracking → Monitor batches_sent / total_batches
6. ✅ Completion → Mark sync as completed
```

### Screenshot Capture Workflow

```
1. 📷 Plate recognition → Check if screenshot enabled for device
2. 📸 Image extraction → Extract Base64 image from recognition data
3. 💾 Database storage → Save screenshot with metadata
4. 🔗 Linking → Associate with plate recognition and entry log
5. 🌐 API access → Screenshots available via REST endpoints
```

### Command Queue Processing

```
1. 📋 Command creation → Admin/system creates command
2. 🗃️ Queue storage → Store in command_queue table with priority
3. 📡 Device communication → Heartbeat or comet poll
4. 📤 Command retrieval → Get up to 5 pending commands
5. 📨 Response inclusion → Add commands to response JSON
6. ✅ Mark processed → Update is_processed = true
```

### Real-World Usage Examples

**Example 1: Tenant Entry**

```
1. Tenant arrives → License plate "ABC123" recognized
2. Whitelist check → Found in tenant whitelist
3. Gate opens → Response: {"info": "ok"}
4. Entry logged → Status: "allowed", Type: "tenant"
5. Screenshot saved → Image stored with metadata
```

**Example 2: Visitor Entry**

```
1. Visitor arrives → License plate "XYZ789" recognized
2. Whitelist check → Not found in any whitelist
3. Gate denied → Response: {"info": "denied"}
4. Entry logged → Status: "denied", Type: "unknown"
5. Screenshot saved → Evidence of denied entry
```

**Example 3: Whitelist Update**

```
1. Admin adds new tenant → Creates whitelist entry
2. Triggers sync → WhitelistStartSync = true
3. Next heartbeat → Device receives clear command
4. Batch processing → Device receives new entries
5. Sync complete → Device whitelist updated
```

**Example 4: Emergency Gate Open**

```
1. Security override → Admin queues "gate_open" command
2. Next communication → Command included in response
3. Gate opens → Regardless of plate recognition
4. Command processed → Marked as completed
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

- HTTP: `http://192.168.1.1:5174`

Note: The server is configured to bind to a specific IP address (192.168.1.1) for production use. HTTPS is disabled in development by default.

### Network Configuration

The application is configured to listen on a specific IP address (`192.168.1.1:5174`) for production deployment. This configuration ensures consistent network access for LPR devices.

**Current Server Configuration:**

- **IP Address**: `192.168.1.1`
- **Port**: `5174`
- **Protocol**: HTTP (HTTPS disabled for development)

Configure your LPR camera to send webhooks to:

```
http://192.168.1.1:5174/api/lpr/sites/{siteCode}/webhook/plate-recognition
```

**To change the IP address:**

1. Update `Program.cs` line 37: `options.Listen(System.Net.IPAddress.Parse("YOUR_IP"), 5174);`
2. Update `Properties/launchSettings.json` applicationUrl
3. Restart the application

## Usage

### Testing with Swagger UI

1. Open your browser and navigate to: `http://localhost:5174/swagger`
2. Expand the LprWebhookController endpoints, e.g. `POST /api/lpr/sites/{siteCode}/webhook/plate-recognition`
3. Click "Try it out"
4. Enter your JSON payload in the request body
5. Click "Execute"

### Testing with curl

**Test Plate Recognition:**

```bash
curl -X POST "http://192.168.1.1:5174/api/lpr/sites/site1/webhook/plate-recognition" \
  -H "Content-Type: application/json" \
  -H "User-Agent: LPR-Camera/1.0" \
  -d '{
    "AlarmInfoPlate": {
      "channel": 0,
      "deviceName": "TestCamera",
      "ipaddr": "192.168.1.100",
      "result": {
        "PlateResult": {
          "license": "ABC123",
          "confidence": 95,
          "colorType": 3,
          "type": 26
        }
      },
      "serialno": "test-device-001"
    }
  }'
```

**Test Screenshot Retrieval:**

```bash
# Get screenshot metadata
curl -X GET "http://192.168.1.1:5174/api/screenshots/1"

# View screenshot image
curl -X GET "http://192.168.1.1:5174/api/screenshots/1/image"

# Download screenshot
curl -X GET "http://192.168.1.1:5174/api/screenshots/1/download" -o screenshot.jpg
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
│   ├── LprWebhookController.cs      # Main webhook endpoints (device → server)
│   ├── ScreenshotsController.cs     # Screenshot management API
│   ├── DeviceManagementController.cs # Device status and feature control
│   ├── WhitelistSyncController.cs   # Whitelist synchronization API
│   ├── DashboardController.cs       # Analytics and monitoring
│   ├── WhitelistsController.cs      # Whitelist CRUD operations
│   ├── EntryLogsController.cs       # Entry log management
│   ├── SitesController.cs           # Site management
│   ├── TenantsController.cs         # Tenant management
│   └── LookupController.cs          # System constants and dropdowns
├── Services/
│   ├── ScreenshotService.cs         # Screenshot capture and retrieval
│   ├── WhitelistSyncService.cs      # Whitelist synchronization logic
│   └── IScreenshotService.cs        # Screenshot service interface
├── Models/
│   ├── Entities/                    # Database entity models
│   │   ├── PlateRecognitionScreenshot.cs # Screenshot data model
│   │   ├── Device.cs                # Device entity with screenshot fields
│   │   ├── PlateRecognitionResult.cs # Recognition results
│   │   └── ...                      # Other entities
│   ├── DTOs/
│   │   ├── ScreenshotDTOs.cs        # Screenshot request/response models
│   │   └── ...                      # Other DTOs
│   └── PlateRecognitionResult.cs    # Main recognition model
├── Data/
│   └── LprDbContext.cs              # Entity Framework context
├── Migrations/
│   ├── AddScreenshotFeature.sql     # Screenshot system migration
│   └── ...                          # Other migrations
├── Middleware/
│   └── RequestResponseLoggingMiddleware.cs # HTTP logging
├── Properties/
│   └── launchSettings.json          # Launch configuration
├── logs/                            # Auto-generated log files
├── Program.cs                       # Application configuration
├── LprWebhookApi.csproj            # Project file
└── appsettings.json                # Application settings
```

## Dependencies

- **Microsoft.EntityFrameworkCore** - ORM framework
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider
- **Serilog.AspNetCore** - Logging framework
- **Serilog.Sinks.File** - File logging
- **Serilog.Formatting.Compact** - JSON log formatting
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **System.Text.Json** - JSON serialization

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

## API Endpoints Overview

### Screenshot Management API

**ScreenshotsController** - Image management and retrieval:

```http
GET    /api/screenshots/{id}                    # Get screenshot metadata
GET    /api/screenshots/{id}/image              # View screenshot image
GET    /api/screenshots/{id}/download           # Download screenshot file
GET    /api/screenshots/plate/{licensePlate}    # Get screenshots by plate
GET    /api/screenshots/device/{deviceId}       # Get screenshots by device
```

**Key Features:**

- **Metadata API**: Get screenshot info without downloading image data
- **Image Display**: Direct image viewing with proper content types
- **File Download**: Browser-friendly download with proper filenames
- **Search Capabilities**: Find screenshots by license plate or device
- **Caching Headers**: Optimized for performance with ETag and Cache-Control

### Device Management API

**DeviceManagementController** - Device monitoring and feature control:

```http
GET    /api/lpr/sites/{siteCode}/devices/status           # All device statuses
POST   /api/lpr/sites/{siteCode}/devices/{deviceId}/screenshot/enable   # Enable screenshots
POST   /api/lpr/sites/{siteCode}/devices/{deviceId}/screenshot/disable  # Disable screenshots
```

**Device Status Response includes:**

- Device connectivity (online/offline, last heartbeat)
- Whitelist sync status (enabled, progress, batches)
- Screenshot capture status (enabled, last request)
- Device metadata (name, serial, IP, location)

### Webhook Endpoints (Device → Server)

**LprWebhookController** - Main communication endpoints:

```http
POST   /api/lpr/sites/{siteCode}/webhook/plate-recognition  # Plate recognition results
POST   /api/lpr/sites/{siteCode}/webhook/heartbeat         # Device heartbeat
POST   /api/lpr/sites/{siteCode}/webhook/comet-poll        # Comet polling
POST   /api/lpr/sites/{siteCode}/webhook/io-trigger        # IO trigger events
POST   /api/lpr/sites/{siteCode}/webhook/screenshot        # Screenshot capture
POST   /api/lpr/sites/{siteCode}/webhook/serial-data       # Serial communication
```

### Whitelist Management API

**WhitelistSyncController** - Synchronization control:

```http
POST   /api/lpr/sites/{siteCode}/devices/{deviceId}/sync-whitelist  # Trigger sync
POST   /api/lpr/sites/{siteCode}/devices/{deviceId}/cancel-sync     # Cancel sync
GET    /api/lpr/sites/{siteCode}/devices/{deviceId}/sync-status     # Get sync status
GET    /api/lpr/sites/{siteCode}/devices/sync-status               # Site-wide status
```

**WhitelistsController** - CRUD operations:

```http
GET    /api/lpr/sites/{siteCode}/whitelists                    # List whitelists
POST   /api/lpr/sites/{siteCode}/whitelists                    # Create whitelist
GET    /api/lpr/sites/{siteCode}/whitelists/{whitelistId}      # Get whitelist
PUT    /api/lpr/sites/{siteCode}/whitelists/{whitelistId}      # Update whitelist
DELETE /api/lpr/sites/{siteCode}/whitelists/{whitelistId}      # Delete whitelist
POST   /api/lpr/sites/{siteCode}/whitelists/{whitelistId}/enable   # Enable whitelist
POST   /api/lpr/sites/{siteCode}/whitelists/{whitelistId}/disable  # Disable whitelist
```

### Analytics and Monitoring

**DashboardController** - Real-time analytics:

```http
GET    /api/lpr/sites/{siteCode}/dashboard/overview         # Site overview stats
GET    /api/lpr/sites/{siteCode}/dashboard/device-status    # Device health summary
GET    /api/lpr/sites/{siteCode}/dashboard/activity-chart   # Activity trends
GET    /api/lpr/sites/{siteCode}/dashboard/top-vehicles     # Most frequent plates
GET    /api/lpr/sites/{siteCode}/dashboard/alerts           # System alerts
```

### Entry Logs and Audit Trail

**EntryLogsController** - Entry management:

```http
GET    /api/lpr/sites/{siteCode}/entry-logs                 # List entry logs
GET    /api/lpr/sites/{siteCode}/entry-logs/recent          # Recent entries
GET    /api/lpr/sites/{siteCode}/entry-logs/statistics      # Entry statistics
GET    /api/lpr/sites/{siteCode}/entry-logs/export          # Export data
GET    /api/lpr/sites/{siteCode}/entry-logs/{entryLogId}    # Get specific entry
```

### Site and Tenant Management

**SitesController** - Multi-tenant site management:

```http
GET    /api/lpr/sites                           # List all sites
GET    /api/lpr/sites/{siteCode}                # Get site details
GET    /api/lpr/sites/{siteCode}/statistics     # Site statistics
```

**TenantsController** - Tenant management:

```http
GET    /api/lpr/sites/{siteCode}/tenants                        # List tenants
GET    /api/lpr/sites/{siteCode}/tenants/{tenantId}             # Get tenant
GET    /api/lpr/sites/{siteCode}/tenants/{tenantId}/entry-logs  # Tenant entries
GET    /api/lpr/sites/{siteCode}/tenants/{tenantId}/whitelists  # Tenant whitelists
```

### System Configuration

**LookupController** - System constants and dropdowns:

```http
GET    /api/lpr/lookup/categories              # Available categories
GET    /api/lpr/lookup/{category}              # Get lookups by category
GET    /api/lpr/lookup/all                     # All lookups grouped
GET    /api/lpr/lookup/entry-types             # Entry type constants
GET    /api/lpr/lookup/recurring-patterns      # Recurring pattern options
GET    /api/lpr/lookup/color-types             # Vehicle color types
GET    /api/lpr/lookup/directions              # Direction constants
GET    /api/lpr/lookup/trigger-types           # Trigger type options
GET    /api/lpr/lookup/vehicle-types           # Vehicle type constants
GET    /api/lpr/lookup/entry-statuses          # Entry status options
```

## Database Architecture

### Core Tables

**sites** - Multi-tenant site management:

- `id`, `site_code`, `site_name`, `description`
- `created_at`, `updated_at`

**devices** - LPR camera device management:

- `id`, `site_id`, `serial_number`, `device_name`
- `ip_address`, `port`, `location_description`
- `is_online`, `last_heartbeat`, `firmware_version`
- **Whitelist Sync Fields**: `whitelist_start_sync`, `whitelist_sync_status`, `whitelist_sync_batches_sent`, `whitelist_sync_total_batches`, `whitelist_sync_started_at`
- **Screenshot Fields**: `capture_screenshot_enabled`, `screenshot_capture_status`, `last_screenshot_request`

**plate_recognition_results** - Raw LPR camera data:

- `id`, `site_id`, `device_id`, `license_plate`
- `confidence`, `color_type`, `direction`, `trigger_type`
- `recognition_timestamp`, `image_path`, `location_data`
- `screenshot_requested`, `screenshot_status`

**plate_recognition_screenshots** - Screenshot storage:

- `id`, `plate_recognition_id`, `site_id`, `device_id`
- `image_base64`, `image_length`, `image_format`
- `license_plate`, `recognition_result`, `confidence_score`
- `screenshot_status`, `requested_at`, `received_at`
- `camera_ip`, `trigger_source`

**whitelists** - License plate whitelist/blacklist:

- `id`, `site_id`, `device_id`, `tenant_id`
- `license_plate`, `entry_type`, `is_active`
- `valid_from`, `valid_until`, `recurring_pattern`
- `created_by`, `approved_by`, `approval_status`

**entry_logs** - Complete audit trail:

- `id`, `site_id`, `device_id`, `plate_recognition_id`
- `license_plate`, `entry_status`, `entry_type`
- `entry_time`, `gate_opened`, `whitelist_id`

**command_queue** - Asynchronous command processing:

- `site_id`, `device_id`, `command_type`, `command_data`
- `priority`, `is_processed`, `created_at`, `processed_at`

### Database Views

**screenshot_summary** - Optimized screenshot querying:

```sql
SELECT
    prs.id, prs.license_plate, prs.recognition_result,
    prs.screenshot_status, prs.requested_at, prs.received_at,
    prs.image_length, prs.image_format,
    pr.recognition_timestamp, d.device_name, d.serial_number,
    s.site_code, s.site_name,
    EXTRACT(EPOCH FROM (prs.received_at - prs.requested_at)) as response_time_seconds
FROM plate_recognition_screenshots prs
JOIN plate_recognition_results pr ON prs.plate_recognition_id = pr.id
JOIN devices d ON prs.device_id = d.id
JOIN sites s ON prs.site_id = s.id;
```

### Database Connection

Current connection string in `Program.cs`:

```csharp
builder.Services.AddDbContext<LprDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=lpr_webhook;Username=postgres;Password=postgres"));
```

**To update database connection:**

1. Modify the connection string in `Program.cs` line 49-50
2. Update credentials, host, database name as needed
3. Restart the application

## Development

### Database Migrations

**Adding New Migrations:**

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

**Recent Migrations:**

- `AddScreenshotFeature.sql`: Screenshot capture system
- `InitialCreate`: Core database schema
- `AddLookupTable`: System constants and enumerations
- `AddWhitelistSyncFields`: Whitelist synchronization features

### Configuration Management

**Server Configuration (Program.cs):**

- **IP Binding**: Currently set to `192.168.1.1:5174`
- **Database**: PostgreSQL connection string
- **Logging**: Serilog with console, file, and JSON outputs
- **HTTPS**: Disabled for development (can be enabled)

**Launch Settings (Properties/launchSettings.json):**

- Application URLs for development
- Environment variables
- Debug settings

### Service Architecture

**Dependency Injection Services:**

- `IScreenshotService` → `ScreenshotService`: Screenshot management
- `WhitelistSyncService`: Whitelist synchronization logic
- `LprDbContext`: Entity Framework database context

**Middleware Pipeline:**

- `RequestResponseLoggingMiddleware`: HTTP request/response logging
- Swagger/OpenAPI documentation
- Entity Framework integration
- Serilog logging integration

### Testing and Debugging

**Comprehensive Logging:**

- Console: Real-time structured output with color coding
- `logs/lpr-webhook-human-*.log`: Human-readable format
- `logs/lpr-webhook-json-*.log`: JSON format for log aggregation
- Request markers: `30--` (requests), `30**` (responses)

**Swagger UI Testing:**

- Available at: `http://192.168.1.1:5174/swagger`
- Interactive API documentation
- Test endpoints directly from browser
- Request/response examples

**Database Debugging:**

- Entity Framework logging (configurable level)
- SQL query logging for performance analysis
- Migration history tracking

### Performance Considerations

**Database Indexing:**

- Screenshot queries: indexed on `plate_recognition_id`, `device_id`, `license_plate`
- Entry logs: indexed on `site_id`, `device_id`, `entry_time`
- Whitelist: indexed on `license_plate`, `site_id`, `is_active`

**Caching Strategy:**

- Screenshot images: ETag and Cache-Control headers
- Static lookups: In-memory caching for constants
- Device status: Real-time updates with heartbeat tracking

**Image Storage:**

- Base64 encoding for PostgreSQL storage
- Future consideration: S3 or blob storage for large volumes
- Image compression and format optimization

## Recent Updates (September 2025)

### Major Features Added

**Screenshot Capture System (v2.0):**

- ✅ Automatic screenshot capture from plate recognition requests
- ✅ Device-level screenshot enable/disable control
- ✅ RESTful API for image retrieval and management
- ✅ Database storage with metadata and indexing
- ✅ Image display, download, and search capabilities
- ✅ Integration with existing plate recognition workflow

**Device Management Enhancement:**

- ✅ Comprehensive device status monitoring
- ✅ Feature control (whitelist sync, screenshot capture)
- ✅ Real-time connectivity tracking
- ✅ Device-specific configuration management

**Network Configuration:**

- ✅ Server binding to specific IP address (192.168.1.1:5174)
- ✅ Production-ready network configuration
- ✅ Optimized for LPR camera communication

### Current System Status

**✅ Fully Tested and Working:**

- Whitelist synchronization with progress tracking
- Screenshot capture and storage (12+ screenshots captured)
- Gate control integration (allow/deny responses)
- Device heartbeat and status monitoring
- Multi-site architecture with data isolation
- Real-time logging and debugging

**🔧 Areas for Future Improvement:**

- Code organization and cleanup (noted in commit message)
- Performance optimization for high-volume deployments
- Enhanced error handling and recovery mechanisms
- Advanced analytics and reporting features
- Mobile/web dashboard interface

### Deployment Notes

**Current Configuration:**

- **Server**: Running on `192.168.1.1:5174`
- **Database**: PostgreSQL with 12+ tables and optimized indexes
- **Storage**: Base64 images in database (4-8KB per screenshot)
- **Logging**: Multi-format logging with 7-day retention

**Production Readiness:**

- ✅ Multi-tenant architecture
- ✅ Comprehensive error handling
- ✅ Performance monitoring
- ✅ Database migrations and versioning
- ✅ API documentation with Swagger
- ⚠️ Code refactoring recommended for maintainability

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes with comprehensive tests
4. Update documentation as needed
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please create an issue in the GitHub repository at:
https://github.com/tikchoong/zenith-lpr

**System Requirements:**

- .NET 8 SDK
- PostgreSQL 12+
- Network access for LPR camera communication
