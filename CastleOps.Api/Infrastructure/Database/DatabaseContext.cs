using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CastleOps.Core.Models;

namespace CastleOps.Api.Infrastructure.Database;

public class DatabaseContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Peon> Peons { get; set; }
    
    public DbSet<PeonConfig> PeonConfigs { get; set; }
    // public DbSet<MarketplaceItem> MarketplaceItems { get; set; }

    // Client agent entities
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientCommand> ClientCommands { get; set; }
    public DbSet<ClientMetric> ClientMetrics { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Peon Configuration ---
        // 1. Make the Peon's Slug unique, as required.
        modelBuilder.Entity<Peon>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        // Add this new configuration for the Peon's default environment
        modelBuilder.Entity<Peon>()
            .Property(p => p.DefaultEnvironment)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>()
            );

        // --- PeonConfig (The Join Table) ---
        // 2. Define the composite primary key for PeonConfig.
        // This ensures a Device can only have ONE config for a specific Peon.
        modelBuilder.Entity<PeonConfig>()
            .HasKey(pc => new { pc.PeonId, pc.DeviceId });

        // 3. Define the relationship from PeonConfig to Peon.
        modelBuilder.Entity<PeonConfig>()
            .HasOne(pc => pc.Peon)
            .WithMany(p => p.Configs)
            .HasForeignKey(pc => pc.PeonId);

        // 4. Define the relationship from PeonConfig to Device.
        modelBuilder.Entity<PeonConfig>()
            .HasOne(pc => pc.Device)
            .WithMany(d => d.PeonConfigs)
            .HasForeignKey(pc => pc.DeviceId);

        // 5. Configure the Environment dictionary to be stored as JSON.
        modelBuilder.Entity<PeonConfig>()
            .Property(e => e.Environment)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>()
            );

        // --- Client Configuration ---
        // Configure Client entity
        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Hostname);

        // Configure ClientCommand entity
        modelBuilder.Entity<ClientCommand>()
            .HasIndex(c => c.ClientId);
        
        modelBuilder.Entity<ClientCommand>()
            .HasIndex(c => new { c.ClientId, c.Sent, c.Completed });

        // Configure ClientMetric entity
        modelBuilder.Entity<ClientMetric>()
            .HasIndex(c => c.ClientId);
        
        modelBuilder.Entity<ClientMetric>()
            .HasIndex(c => new { c.ClientId, c.Timestamp });
    }
}