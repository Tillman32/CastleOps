using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CastleOps.Api.Infrastructure.Database;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        
        // Use the same path logic as your Program.cs
        var directory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CastleOps");
        Directory.CreateDirectory(directory);
        var dbPath = Path.Join(directory, "app.db");
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new DatabaseContext(optionsBuilder.Options);
    }
}