using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SlipperIS.Data.DbContext;

namespace SlipperIS.UI;

public static class AppDbContextFactory
{
    private static string? _connectionString;

    public static string ConnectionString
    {
        get
        {
            if (_connectionString == null)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();
                _connectionString = config.GetConnectionString("DefaultConnection")
                    ?? "Data Source=slippers_inventory.db";
            }
            return _connectionString;
        }
    }

    public static SlipperDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SlipperDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        var context = new SlipperDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
