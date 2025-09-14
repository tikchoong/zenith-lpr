using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Models.Entities;
using System.Net;

namespace LprWebhookApi.Data;

public class LprDbContext : DbContext
{
    public LprDbContext(DbContextOptions<LprDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Site> Sites { get; set; }
    public DbSet<SiteUser> SiteUsers { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<SiteStaff> SiteStaff { get; set; }
    public DbSet<PlateRecognitionResult> PlateRecognitionResults { get; set; }
    public DbSet<Whitelist> Whitelists { get; set; }
    public DbSet<EntryLog> EntryLogs { get; set; }
    public DbSet<DeviceHeartbeat> DeviceHeartbeats { get; set; }
    public DbSet<IoTriggerEvent> IoTriggerEvents { get; set; }
    public DbSet<SerialDataLog> SerialDataLogs { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }
    public DbSet<CommandQueue> CommandQueue { get; set; }
    public DbSet<ResponseLog> ResponseLogs { get; set; }
    public DbSet<SiteConfiguration> SiteConfigurations { get; set; }
    public DbSet<LookupTable> LookupTables { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure IPAddress conversion
        modelBuilder.Entity<Device>()
            .Property(e => e.IpAddress)
            .HasConversion(
                v => v == null ? null : v.ToString(),
                v => v == null ? null : IPAddress.Parse(v));

        // Configure unique constraints
        modelBuilder.Entity<Site>()
            .HasIndex(e => e.SiteCode)
            .IsUnique();

        modelBuilder.Entity<SiteUser>()
            .HasIndex(e => new { e.SiteId, e.Email })
            .IsUnique();

        modelBuilder.Entity<Device>()
            .HasIndex(e => new { e.SiteId, e.SerialNumber })
            .IsUnique();

        modelBuilder.Entity<Tenant>()
            .HasIndex(e => new { e.SiteId, e.TenantCode })
            .IsUnique();

        modelBuilder.Entity<Tenant>()
            .HasIndex(e => new { e.SiteId, e.UnitNumber })
            .IsUnique();

        modelBuilder.Entity<SiteStaff>()
            .HasIndex(e => new { e.SiteId, e.StaffCode })
            .IsUnique();

        modelBuilder.Entity<Whitelist>()
            .HasIndex(e => new { e.SiteId, e.LicensePlate, e.EntryType, e.DeviceId })
            .IsUnique();

        modelBuilder.Entity<SiteConfiguration>()
            .HasIndex(e => new { e.SiteId, e.ConfigKey })
            .IsUnique();

        // Configure foreign key relationships
        ConfigureForeignKeys(modelBuilder);

        // Configure indexes for performance
        ConfigureIndexes(modelBuilder);
    }

    private void ConfigureForeignKeys(ModelBuilder modelBuilder)
    {
        // Site relationships
        modelBuilder.Entity<SiteUser>()
            .HasOne(e => e.Site)
            .WithMany(e => e.SiteUsers)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Device>()
            .HasOne(e => e.Site)
            .WithMany(e => e.Devices)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasOne(e => e.Site)
            .WithMany(e => e.Tenants)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SiteStaff>()
            .HasOne(e => e.Site)
            .WithMany(e => e.SiteStaff)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Whitelist relationships
        modelBuilder.Entity<Whitelist>()
            .HasOne(e => e.Site)
            .WithMany(e => e.Whitelists)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Whitelist>()
            .HasOne(e => e.Device)
            .WithMany(e => e.Whitelists)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Whitelist>()
            .HasOne(e => e.Tenant)
            .WithMany(e => e.Whitelists)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Whitelist>()
            .HasOne(e => e.Staff)
            .WithMany(e => e.Whitelists)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Whitelist>()
            .HasOne(e => e.CreatedByUser)
            .WithMany(e => e.CreatedWhitelists)
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Whitelist>()
            .HasOne(e => e.ApprovedByUser)
            .WithMany(e => e.ApprovedWhitelists)
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // PlateRecognitionResult relationships
        modelBuilder.Entity<PlateRecognitionResult>()
            .HasOne(e => e.Site)
            .WithMany(e => e.PlateRecognitionResults)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlateRecognitionResult>()
            .HasOne(e => e.Device)
            .WithMany(e => e.PlateRecognitionResults)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // EntryLog relationships
        modelBuilder.Entity<EntryLog>()
            .HasOne(e => e.Site)
            .WithMany(e => e.EntryLogs)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EntryLog>()
            .HasOne(e => e.Device)
            .WithMany(e => e.EntryLogs)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EntryLog>()
            .HasOne(e => e.Tenant)
            .WithMany(e => e.EntryLogs)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EntryLog>()
            .HasOne(e => e.Staff)
            .WithMany(e => e.EntryLogs)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EntryLog>()
            .HasOne(e => e.Whitelist)
            .WithMany(e => e.EntryLogs)
            .HasForeignKey(e => e.WhitelistId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EntryLog>()
            .HasOne(e => e.PlateRecognitionResult)
            .WithMany(e => e.EntryLogs)
            .HasForeignKey(e => e.PlateRecognitionId)
            .OnDelete(DeleteBehavior.SetNull);

        // DeviceHeartbeat relationships
        modelBuilder.Entity<DeviceHeartbeat>()
            .HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceHeartbeat>()
            .HasOne(e => e.Device)
            .WithMany(e => e.DeviceHeartbeats)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // IoTriggerEvent relationships
        modelBuilder.Entity<IoTriggerEvent>()
            .HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IoTriggerEvent>()
            .HasOne(e => e.Device)
            .WithMany(e => e.IoTriggerEvents)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // SerialDataLog relationships
        modelBuilder.Entity<SerialDataLog>()
            .HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SerialDataLog>()
            .HasOne(e => e.Device)
            .WithMany(e => e.SerialDataLogs)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Screenshot relationships
        modelBuilder.Entity<Screenshot>()
            .HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Screenshot>()
            .HasOne(e => e.Device)
            .WithMany(e => e.Screenshots)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // CommandQueue relationships
        modelBuilder.Entity<CommandQueue>()
            .HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommandQueue>()
            .HasOne(e => e.Device)
            .WithMany(e => e.CommandQueue)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ResponseLog relationships
        modelBuilder.Entity<ResponseLog>()
            .HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponseLog>()
            .HasOne(e => e.Device)
            .WithMany()
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        // SiteConfiguration relationships
        modelBuilder.Entity<SiteConfiguration>()
            .HasOne(e => e.Site)
            .WithMany(e => e.SiteConfigurations)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SiteConfiguration>()
            .HasOne(e => e.UpdatedByUser)
            .WithMany(e => e.UpdatedConfigurations)
            .HasForeignKey(e => e.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Entry logs indexes for performance
        modelBuilder.Entity<EntryLog>()
            .HasIndex(e => new { e.SiteId, e.EntryTime })
            .HasDatabaseName("idx_entry_logs_site_time");

        modelBuilder.Entity<EntryLog>()
            .HasIndex(e => e.LicensePlate)
            .HasDatabaseName("idx_entry_logs_plate");

        modelBuilder.Entity<EntryLog>()
            .HasIndex(e => e.TenantId)
            .HasDatabaseName("idx_entry_logs_tenant");

        modelBuilder.Entity<EntryLog>()
            .HasIndex(e => e.DeviceId)
            .HasDatabaseName("idx_entry_logs_device");

        // Lookup table indexes
        modelBuilder.Entity<LookupTable>()
            .HasIndex(l => new { l.Category, l.Code })
            .IsUnique()
            .HasDatabaseName("idx_lookup_category_code");

        modelBuilder.Entity<LookupTable>()
            .HasIndex(l => new { l.Category, l.SortOrder })
            .HasDatabaseName("idx_lookup_category_sort");

        modelBuilder.Entity<LookupTable>()
            .HasIndex(l => l.Category)
            .HasDatabaseName("idx_lookup_category");
    }
}
