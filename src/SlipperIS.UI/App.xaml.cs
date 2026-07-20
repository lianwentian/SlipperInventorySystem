using System.Windows;
using Microsoft.Extensions.Configuration;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 从 appsettings.json 加载配置
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? "Data Source=slippers_inventory.db";

            // 初始化数据库上下文工厂
            AppDbContextFactory.Initialize(connectionString);
        }
    }
}
