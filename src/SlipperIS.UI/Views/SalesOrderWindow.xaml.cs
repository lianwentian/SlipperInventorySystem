using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class SalesOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}

public partial class SalesOrderWindow : Window
{
    private List<SalesOrderViewModel> _allOrders = new();

    public SalesOrderWindow()
    {
        InitializeComponent();
        LoadOrders();
    }

    private void LoadOrders()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allOrders = db.SalesOrders
                .Include(o => o.Customer)
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

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载订单失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        var statusItem = StatusFilter.SelectedItem as ComboBoxItem;
        var status = statusItem?.Content?.ToString() ?? "全部";
        OrderGrid.ItemsSource = status == "全部"
            ? _allOrders
            : _allOrders.Where(o => o.Status == status).ToList();
    }

    private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void NewOrder_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SalesOrderEditDialog(null);
        if (dlg.ShowDialog() == true) LoadOrders();
    }

    private void ViewOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderGrid.SelectedItem is not SalesOrderViewModel selected)
        {
            MessageBox.Show("请先选择一个订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        using var db = AppDbContextFactory.Create();
        var order = db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.Id == selected.Id);
        if (order != null)
        {
            var dlg = new SalesOrderEditDialog(order);
            if (dlg.ShowDialog() == true) LoadOrders();
        }
    }

    private void CancelOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderGrid.SelectedItem is not SalesOrderViewModel selected)
        {
            MessageBox.Show("请先选择一个订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (selected.Status == "Cancelled")
        {
            MessageBox.Show("该订单已经是取消状态。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"确定要取消订单「{selected.OrderNumber}」吗？", "确认取消",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var order = db.SalesOrders.Find(selected.Id);
            if (order != null)
            {
                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
            LoadOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"取消失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }
}
