using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CastleOps.Core.Models;

namespace CastleOps.Api.Infrastructure.Database;

public class DatabaseContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Peon> Peons { get; set; }
    public DbSet<PeonConfig> PeonConfigs { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientCommand> ClientCommands { get; set; }
    public DbSet<ClientMetric> ClientMetrics { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Peon ---
        modelBuilder.Entity<Peon>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        modelBuilder.Entity<Peon>()
            .Property(p => p.DefaultEnvironment)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new());

        // --- PeonConfig (composite PK: one config per Peon per Device) ---
        modelBuilder.Entity<PeonConfig>()
            .HasKey(pc => new { pc.PeonId, pc.DeviceId });

        modelBuilder.Entity<PeonConfig>()
            .HasOne(pc => pc.Peon)
            .WithMany(p => p.Configs)
            .HasForeignKey(pc => pc.PeonId);

        modelBuilder.Entity<PeonConfig>()
            .HasOne(pc => pc.Device)
            .WithMany(d => d.PeonConfigs)
            .HasForeignKey(pc => pc.DeviceId);

        modelBuilder.Entity<PeonConfig>()
            .Property(e => e.Environment)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new());

        // --- Device ---
        // A device optionally links to the Client agent registered on it.
        // One agent per device; deleting/unlinking the agent nulls the FK.
        modelBuilder.Entity<Device>()
            .HasOne(d => d.Client)
            .WithMany()
            .HasForeignKey(d => d.ClientId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Client ---
        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Hostname);

        // --- ClientCommand ---
        modelBuilder.Entity<ClientCommand>()
            .HasIndex(c => c.ClientId);

        modelBuilder.Entity<ClientCommand>()
            .HasIndex(c => new { c.ClientId, c.Sent, c.Completed });

        // --- ClientMetric ---
        modelBuilder.Entity<ClientMetric>()
            .HasIndex(c => c.ClientId);

        modelBuilder.Entity<ClientMetric>()
            .HasIndex(c => new { c.ClientId, c.Timestamp });
    }
}
