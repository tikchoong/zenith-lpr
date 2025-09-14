using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LprWebhookApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sites",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    site_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_devices = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    site_manager_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    site_manager_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    site_manager_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sites", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    port = table.Column<int>(type: "integer", nullable: true),
                    location_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_online = table.Column<bool>(type: "boolean", nullable: false),
                    last_heartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    firmware_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                    table.ForeignKey(
                        name: "FK_devices_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    staff_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    staff_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_staff", x => x.id);
                    table.ForeignKey(
                        name: "FK_site_staff_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_site_users_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tenant_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    emergency_contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    emergency_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    move_in_date = table.Column<DateOnly>(type: "date", nullable: true),
                    move_out_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenants_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "command_queue",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    command_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    command_data = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_queue", x => x.id);
                    table.ForeignKey(
                        name: "FK_command_queue_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_command_queue_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_heartbeats",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    heartbeat_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    password = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    channel_num = table.Column<int>(type: "integer", nullable: true),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_heartbeats", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_heartbeats_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_device_heartbeats_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "io_trigger_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: true),
                    value = table.Column<int>(type: "integer", nullable: true),
                    triggered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_io_trigger_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_io_trigger_events_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_io_trigger_events_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plate_recognition_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    plate_id = table.Column<int>(type: "integer", nullable: false),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    confidence = table.Column<int>(type: "integer", nullable: true),
                    color_type = table.Column<int>(type: "integer", nullable: true),
                    plate_type = table.Column<int>(type: "integer", nullable: true),
                    direction = table.Column<int>(type: "integer", nullable: true),
                    trigger_type = table.Column<int>(type: "integer", nullable: true),
                    is_offline = table.Column<bool>(type: "boolean", nullable: false),
                    is_fake_plate = table.Column<bool>(type: "boolean", nullable: true),
                    plate_true_width = table.Column<int>(type: "integer", nullable: true),
                    plate_distance = table.Column<int>(type: "integer", nullable: true),
                    plate_location = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    car_location = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    car_brand = table.Column<int>(type: "integer", nullable: true),
                    car_year = table.Column<int>(type: "integer", nullable: true),
                    car_type = table.Column<int>(type: "integer", nullable: true),
                    feature_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    recognition_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_used = table.Column<int>(type: "integer", nullable: true),
                    usec = table.Column<int>(type: "integer", nullable: true),
                    image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_file_base64 = table.Column<string>(type: "text", nullable: true),
                    image_file_length = table.Column<int>(type: "integer", nullable: true),
                    image_fragment_base64 = table.Column<string>(type: "text", nullable: true),
                    image_fragment_length = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plate_recognition_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_plate_recognition_results_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plate_recognition_results_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "response_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: true),
                    request_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    request_data = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    response_data = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    processing_time_ms = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_response_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_response_logs_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_response_logs_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screenshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    image_base64 = table.Column<string>(type: "text", nullable: true),
                    image_length = table.Column<int>(type: "integer", nullable: true),
                    trigger_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screenshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_screenshots_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_screenshots_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "serial_data_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    serial_channel = table.Column<int>(type: "integer", nullable: true),
                    data_base64 = table.Column<string>(type: "text", nullable: true),
                    data_length = table.Column<int>(type: "integer", nullable: true),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_serial_data_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_serial_data_logs_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_serial_data_logs_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_configurations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    config_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    config_value = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_configurations", x => x.id);
                    table.ForeignKey(
                        name: "FK_site_configurations_site_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "site_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_site_configurations_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "whitelists",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: true),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    staff_id = table.Column<int>(type: "integer", nullable: true),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entry_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_blacklist = table.Column<bool>(type: "boolean", nullable: false),
                    visitor_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    visitor_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    visitor_company = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    visit_purpose = table.Column<string>(type: "text", nullable: true),
                    enable_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_entries = table.Column<int>(type: "integer", nullable: true),
                    current_entries = table.Column<int>(type: "integer", nullable: false),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false),
                    recurring_pattern = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    recurring_start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    recurring_end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    allowed_devices = table.Column<int[]>(type: "integer[]", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    approved_by = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whitelists", x => x.id);
                    table.ForeignKey(
                        name: "FK_whitelists_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_whitelists_site_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "site_staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_whitelists_site_users_approved_by",
                        column: x => x.approved_by,
                        principalTable: "site_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_whitelists_site_users_created_by",
                        column: x => x.created_by,
                        principalTable: "site_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_whitelists_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_whitelists_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entry_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    staff_id = table.Column<int>(type: "integer", nullable: true),
                    whitelist_id = table.Column<int>(type: "integer", nullable: true),
                    plate_recognition_id = table.Column<int>(type: "integer", nullable: true),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entry_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    entry_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    confidence = table.Column<int>(type: "integer", nullable: true),
                    entry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    exit_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    gate_opened = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entry_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_entry_logs_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entry_logs_plate_recognition_results_plate_recognition_id",
                        column: x => x.plate_recognition_id,
                        principalTable: "plate_recognition_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_entry_logs_site_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "site_staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_entry_logs_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entry_logs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_entry_logs_whitelists_whitelist_id",
                        column: x => x.whitelist_id,
                        principalTable: "whitelists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_command_queue_device_id",
                table: "command_queue",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_queue_site_id",
                table: "command_queue",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_heartbeats_device_id",
                table: "device_heartbeats",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_heartbeats_site_id",
                table: "device_heartbeats",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_site_id_serial_number",
                table: "devices",
                columns: new[] { "site_id", "serial_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_entry_logs_device",
                table: "entry_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "idx_entry_logs_plate",
                table: "entry_logs",
                column: "license_plate");

            migrationBuilder.CreateIndex(
                name: "idx_entry_logs_site_time",
                table: "entry_logs",
                columns: new[] { "site_id", "entry_time" });

            migrationBuilder.CreateIndex(
                name: "idx_entry_logs_tenant",
                table: "entry_logs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_entry_logs_plate_recognition_id",
                table: "entry_logs",
                column: "plate_recognition_id");

            migrationBuilder.CreateIndex(
                name: "IX_entry_logs_staff_id",
                table: "entry_logs",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_entry_logs_whitelist_id",
                table: "entry_logs",
                column: "whitelist_id");

            migrationBuilder.CreateIndex(
                name: "IX_io_trigger_events_device_id",
                table: "io_trigger_events",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_io_trigger_events_site_id",
                table: "io_trigger_events",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_plate_recognition_results_device_id",
                table: "plate_recognition_results",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_plate_recognition_results_site_id",
                table: "plate_recognition_results",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_response_logs_device_id",
                table: "response_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_response_logs_site_id",
                table: "response_logs",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_screenshots_device_id",
                table: "screenshots",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_screenshots_site_id",
                table: "screenshots",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_serial_data_logs_device_id",
                table: "serial_data_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_serial_data_logs_site_id",
                table: "serial_data_logs",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_configurations_site_id_config_key",
                table: "site_configurations",
                columns: new[] { "site_id", "config_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_site_configurations_updated_by",
                table: "site_configurations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_site_staff_site_id_staff_code",
                table: "site_staff",
                columns: new[] { "site_id", "staff_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_site_users_site_id_email",
                table: "site_users",
                columns: new[] { "site_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sites_site_code",
                table: "sites",
                column: "site_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_site_id_tenant_code",
                table: "tenants",
                columns: new[] { "site_id", "tenant_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_site_id_unit_number",
                table: "tenants",
                columns: new[] { "site_id", "unit_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_whitelists_approved_by",
                table: "whitelists",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_whitelists_created_by",
                table: "whitelists",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_whitelists_device_id",
                table: "whitelists",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_whitelists_site_id_license_plate_entry_type_device_id",
                table: "whitelists",
                columns: new[] { "site_id", "license_plate", "entry_type", "device_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_whitelists_staff_id",
                table: "whitelists",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_whitelists_tenant_id",
                table: "whitelists",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "command_queue");

            migrationBuilder.DropTable(
                name: "device_heartbeats");

            migrationBuilder.DropTable(
                name: "entry_logs");

            migrationBuilder.DropTable(
                name: "io_trigger_events");

            migrationBuilder.DropTable(
                name: "response_logs");

            migrationBuilder.DropTable(
                name: "screenshots");

            migrationBuilder.DropTable(
                name: "serial_data_logs");

            migrationBuilder.DropTable(
                name: "site_configurations");

            migrationBuilder.DropTable(
                name: "plate_recognition_results");

            migrationBuilder.DropTable(
                name: "whitelists");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "site_staff");

            migrationBuilder.DropTable(
                name: "site_users");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "sites");
        }
    }
}
