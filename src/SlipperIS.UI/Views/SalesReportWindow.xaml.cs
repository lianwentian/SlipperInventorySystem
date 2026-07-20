using System.Windows;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 销售报表窗口 - 显示销售统计数据
/// </summary>
public partial class SalesReportWindow : Window
{
    private SlipperDbContext _db;

    public SalesReportWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        DpStart.SelectedDate = FirstDayOfCurrentMonth();
        DpEnd.SelectedDate = DateTime.Today;
        LoadReport();
    }

    /// <summary>返回当月第一天的日期</summary>
    private static DateTime FirstDayOfCurrentMonth() =>
        new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private void LoadReport()
    {
        var start = DpStart.SelectedDate ?? new DateTime(DateTime.Today.Year, 1, 1);
        var end = (DpEnd.SelectedDate ?? DateTime.Today).AddDays(1);

        var orders = _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Where(o => o.OrderDate >= start && o.OrderDate < end
                     && o.Status != "Cancelled")
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        // 更新汇总卡片
        TxtTotalOrders.Text = orders.Count.ToString();
        var totalAmount = orders.Sum(o => o.FinalAmount);
        TxtTotalAmount.Text = $"¥{totalAmount:F2}";
        TxtAvgAmount.Text = orders.Count > 0 ? $"¥{totalAmount / orders.Count:F2}" : "¥0.00";
        TxtCustomerCount.Text = orders.Select(o => o.CustomerId).Distinct().Count().ToString();

        // 更新列表
        ReportGrid.ItemsSource = orders.Select(o => new
        {
            o.OrderNumber,
            CustomerName = o.Customer?.CustomerName ?? string.Empty,
            o.OrderDate,
            o.TotalAmount,
            o.FinalAmount,
            o.Status,
            o.PaymentStatus
        }).ToList();

        StatusText.Text = $"查询期间：{start:yyyy-MM-dd} 至 {end.AddDays(-1):yyyy-MM-dd}，共 {orders.Count} 条订单";
    }

    private void Query_Click(object sender, RoutedEventArgs e) => LoadReport();

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        DpStart.SelectedDate = FirstDayOfCurrentMonth();
        DpEnd.SelectedDate = DateTime.Today;
        LoadReport();
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}
