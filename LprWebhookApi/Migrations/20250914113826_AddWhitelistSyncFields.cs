using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LprWebhookApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWhitelistSyncFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "whitelist_start_sync",
                table: "devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "whitelist_sync_batches_sent",
                table: "devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "whitelist_sync_started_at",
                table: "devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "whitelist_sync_status",
                table: "devices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "whitelist_sync_total_batches",
                table: "devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "whitelist_start_sync",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "whitelist_sync_batches_sent",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "whitelist_sync_started_at",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "whitelist_sync_status",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "whitelist_sync_total_batches",
                table: "devices");
        }
    }
}
