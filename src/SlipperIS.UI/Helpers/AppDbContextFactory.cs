using Microsoft.EntityFrameworkCore;
using SlipperIS.Data.DbContext;

namespace SlipperIS.UI.Helpers;

/// <summary>
/// 数据库上下文工厂 - 为每个窗口创建独立的 DbContext 实例
/// </summary>
public static class AppDbContextFactory
{
    private static DbContextOptions<SlipperDbContext>? _options;

    /// <summary>
    /// 使用连接字符串初始化工厂
    /// </summary>
    public static void Initialize(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SlipperDbContext>();
        optionsBuilder.UseSqlite(connectionString);
        _options = optionsBuilder.Options;
    }

    /// <summary>
    /// 创建一个新的数据库上下文实例，并确保数据库已创建
    /// </summary>
    public static SlipperDbContext CreateContext()
    {
        if (_options == null)
            throw new InvalidOperationException("DbContext factory has not been initialized. Call Initialize() first.");

        var context = new SlipperDbContext(_options);
        context.Database.EnsureCreated();
        return context;
    }
}
