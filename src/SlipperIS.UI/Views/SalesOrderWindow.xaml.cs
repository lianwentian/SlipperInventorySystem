using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 销售订单窗口 - 支持订单的查看、创建和状态管理
/// </summary>
public partial class SalesOrderWindow : Window
{
    private SlipperDbContext _db;
    private List<SalesOrderViewModel> _allOrders = new();
    private List<OrderDetailLine> _newOrderLines = new();

    public SalesOrderWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadOrders();
    }

    private void LoadOrders()
    {
        var orders = _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        _allOrders = orders.Select(o => new SalesOrderViewModel(o)).ToList();
        ApplyFilter();
        StatusText.Text = $"共 {_allOrders.Count} 条订单";
    }

    private void ApplyFilter()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部";

        var filtered = _allOrders.AsEnumerable();
        if (!string.IsNullOrEmpty(keyword))
            filtered = filtered.Where(o => o.OrderNumber.ToLower().Contains(keyword)
                                        || o.CustomerName.ToLower().Contains(keyword));
        if (status != "全部")
            filtered = filtered.Where(o => o.Status == status);

        OrderGrid.ItemsSource = filtered.ToList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadOrders();
    }

    private void NewOrder_Click(object sender, RoutedEventArgs e)
    {
        _newOrderLines.Clear();
        DetailGrid.ItemsSource = null;
        TxtAddress.Text = string.Empty;
        TxtRemarks.Text = string.Empty;
        DpDelivery.SelectedDate = DateTime.Today.AddDays(7);
        TxtOrderTotal.Text = "合计：¥0.00";

        // 加载客户和产品下拉框
        CmbCustomer.ItemsSource = _db.Customers.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToList();
        CmbProduct.ItemsSource = _db.Products.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.ProductName).ToList();

        NewOrderPanel.Visibility = Visibility.Visible;
    }

    private void AddDetailLine_Click(object sender, RoutedEventArgs e)
    {
        if (CmbProduct.SelectedItem is not Product product)
        {
            MessageBox.Show("请选择产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!int.TryParse(TxtQty.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("请输入有效的数量。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtUnitPrice.Text, out var price) || price < 0)
        {
            MessageBox.Show("请输入有效的单价。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var line = new OrderDetailLine
        {
            ProductId = product.Id,
            ProductName = product.ProductName,
            Quantity = qty,
            UnitPrice = price > 0 ? price : product.SalesPrice,
            LineAmount = qty * (price > 0 ? price : product.SalesPrice)
        };
        line.UnitPrice = line.LineAmount / qty;

        _newOrderLines.Add(line);
        DetailGrid.ItemsSource = null;
        DetailGrid.ItemsSource = _newOrderLines;
        UpdateOrderTotal();
    }

    private void RemoveDetailLine_Click(object sender, RoutedEventArgs e)
    {
        if (DetailGrid.SelectedItem is OrderDetailLine line)
        {
            _newOrderLines.Remove(line);
            DetailGrid.ItemsSource = null;
            DetailGrid.ItemsSource = _newOrderLines;
            UpdateOrderTotal();
        }
    }

    private void UpdateOrderTotal()
    {
        var total = _newOrderLines.Sum(l => l.LineAmount);
        TxtOrderTotal.Text = $"合计：¥{total:F2}";
    }

    private void SaveOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CmbCustomer.SelectedItem is not Customer customer)
        {
            MessageBox.Show("请选择客户。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_newOrderLines.Count == 0)
        {
            MessageBox.Show("请至少添加一个订单明细。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 生成订单号
        var orderNumber = $"SO{DateTime.Now:yyyyMMddHHmmss}";
        var total = _newOrderLines.Sum(l => l.LineAmount);

        var order = new SalesOrder
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            CreatedByUserId = 1,
            OrderDate = DateTime.Now,
            DeliveryDate = DpDelivery.SelectedDate ?? DateTime.Today.AddDays(7),
            TotalAmount = total,
            FinalAmount = total,
            Status = "Pending",
            PaymentStatus = "Unpaid",
            ShippingAddress = TxtAddress.Text.Trim(),
            Remarks = TxtRemarks.Text.Trim(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        foreach (var line in _newOrderLines)
        {
            order.OrderDetails.Add(new SalesOrderDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineAmount = line.LineAmount
            });
        }

        _db.SalesOrders.Add(order);
        _db.SaveChanges();

        NewOrderPanel.Visibility = Visibility.Collapsed;
        LoadOrders();
        StatusText.Text = $"订单 {orderNumber} 已创建";
    }

    private void CancelOrder_Click(object sender, RoutedEventArgs e)
    {
        NewOrderPanel.Visibility = Visibility.Collapsed;
    }

    private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderGrid.SelectedItem is not SalesOrderViewModel selected)
        {
            MessageBox.Show("请先选择一条订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (selected.Status != "Pending")
        {
            MessageBox.Show("只能确认状态为 Pending 的订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var order = _db.SalesOrders.Find(selected.Id);
        if (order != null)
        {
            order.Status = "Confirmed";
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
            LoadOrders();
            StatusText.Text = $"订单 {selected.OrderNumber} 已确认";
        }
    }

    private void CancelOrderStatus_Click(object sender, RoutedEventArgs e)
    {
        if (OrderGrid.SelectedItem is not SalesOrderViewModel selected)
        {
            MessageBox.Show("请先选择一条订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (selected.Status == "Completed" || selected.Status == "Cancelled")
        {
            MessageBox.Show("已完成或已取消的订单不能再取消。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"确定要取消订单 {selected.OrderNumber} 吗？", "确认",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var order = _db.SalesOrders.Find(selected.Id);
            if (order != null)
            {
                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                LoadOrders();
                StatusText.Text = $"订单 {selected.OrderNumber} 已取消";
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}

/// <summary>
/// 销售订单视图模型
/// </summary>
public class SalesOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;

    public SalesOrderViewModel(SalesOrder o)
    {
        Id = o.Id;
        OrderNumber = o.OrderNumber;
        CustomerName = o.Customer?.CustomerName ?? string.Empty;
        OrderDate = o.OrderDate;
        DeliveryDate = o.DeliveryDate;
        TotalAmount = o.TotalAmount;
        FinalAmount = o.FinalAmount;
        Status = o.Status;
        PaymentStatus = o.PaymentStatus;
        Remarks = o.Remarks;
    }
}

/// <summary>
/// 新建订单明细行
/// </summary>
public class OrderDetailLine
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
}
