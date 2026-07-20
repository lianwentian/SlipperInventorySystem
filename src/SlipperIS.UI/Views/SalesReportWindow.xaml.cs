using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace SlipperIS.UI.Views;

public partial class SalesReportWindow : Window
{
    public SalesReportWindow()
    {
        InitializeComponent();
        StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
        EndDatePicker.SelectedDate = DateTime.Today;
        QueryReport();
    }

    private void Query_Click(object sender, RoutedEventArgs e)
    {
        QueryReport();
    }

    private void QueryReport()
    {
        try
        {
            var start = StartDatePicker.SelectedDate ?? DateTime.Today.AddDays(-30);
            var end = (EndDatePicker.SelectedDate ?? DateTime.Today).AddDays(1);

            using var db = AppDbContextFactory.Create();
            var orders = db.SalesOrders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.Status != "Cancelled")
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new SalesOrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : string.Empty,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    FinalAmount = o.FinalAmount,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus
                }).ToList();

            ReportGrid.ItemsSource = orders;
            TotalOrdersText.Text = orders.Count.ToString();
            var total = orders.Sum(o => o.FinalAmount);
            TotalAmountText.Text = total.ToString("F2");
            AvgAmountText.Text = orders.Count > 0 ? (total / orders.Count).ToString("F2") : "0.00";
            CustomerCountText.Text = orders.Select(o => o.CustomerName).Distinct().Count().ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"查询失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
