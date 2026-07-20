using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class OrderDetailRow
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal LineAmount => Math.Round(UnitPrice * Quantity * (1 - DiscountPercentage / 100), 2);
    public string Remarks { get; set; } = string.Empty;
}

public partial class SalesOrderEditDialog : Window
{
    private readonly SalesOrder? _order;
    private List<Customer> _customers = new();
    private List<Product> _products = new();
    private ObservableCollection<OrderDetailRow> _details = new();
    private bool _isReadOnly;

    public SalesOrderEditDialog(SalesOrder? order)
    {
        InitializeComponent();
        _order = order;
        _isReadOnly = order != null && (order.Status == "Cancelled" || order.Status == "Completed");
        LoadData();
        if (order != null) FillForm(order);
        else InitNew();
        DetailGrid.ItemsSource = _details;
        UpdateTotal();
        _details.CollectionChanged += (_, _) => UpdateTotal();
    }

    private void LoadData()
    {
        using var db = AppDbContextFactory.Create();
        _customers = db.Customers.Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToList();
        _products = db.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToList();

        CustomerCombo.ItemsSource = _customers;

        var productCol = DetailGrid.Columns[0] as DataGridComboBoxColumn;
        if (productCol != null)
        {
            productCol.ItemsSource = _products;
        }
    }

    private void InitNew()
    {
        Title = "新建销售订单";
        var now = DateTime.Now;
        OrderNumberText.Text = $"SO{now:yyyyMMddHHmmss}";
        OrderDatePicker.SelectedDate = now;
        DeliveryDatePicker.SelectedDate = now.AddDays(7);
    }

    private void FillForm(SalesOrder order)
    {
        Title = $"订单详情 - {order.OrderNumber}";
        OrderNumberText.Text = order.OrderNumber;
        CustomerCombo.SelectedValue = order.CustomerId;
        OrderDatePicker.SelectedDate = order.OrderDate;
        DeliveryDatePicker.SelectedDate = order.DeliveryDate;
        RemarksBox.Text = order.Remarks;

        foreach (ComboBoxItem item in StatusCombo.Items)
            if (item.Content?.ToString() == order.Status) { StatusCombo.SelectedItem = item; break; }
        foreach (ComboBoxItem item in PaymentStatusCombo.Items)
            if (item.Content?.ToString() == order.PaymentStatus) { PaymentStatusCombo.SelectedItem = item; break; }

        foreach (var detail in order.OrderDetails)
        {
            _details.Add(new OrderDetailRow
            {
                Id = detail.Id,
                ProductId = detail.ProductId,
                ProductName = detail.Product?.ProductName ?? string.Empty,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                DiscountPercentage = detail.DiscountPercentage,
                Remarks = detail.Remarks
            });
        }

        if (_isReadOnly)
        {
            SaveBtn.IsEnabled = false;
            CustomerCombo.IsEnabled = false;
            OrderDatePicker.IsEnabled = false;
            DeliveryDatePicker.IsEnabled = false;
            StatusCombo.IsEnabled = false;
            PaymentStatusCombo.IsEnabled = false;
            RemarksBox.IsReadOnly = true;
        }
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        if (_isReadOnly) return;
        if (_products.Count == 0)
        {
            MessageBox.Show("没有可用产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var first = _products[0];
        _details.Add(new OrderDetailRow
        {
            ProductId = first.Id,
            ProductName = first.ProductName,
            UnitPrice = first.SalesPrice,
            Quantity = 1
        });
        UpdateTotal();
    }

    private void DeleteLine_Click(object sender, RoutedEventArgs e)
    {
        if (_isReadOnly) return;
        if (DetailGrid.SelectedItem is OrderDetailRow row)
        {
            _details.Remove(row);
            UpdateTotal();
        }
    }

    private void UpdateTotal()
    {
        var total = _details.Sum(d => d.LineAmount);
        TotalText.Text = total.ToString("F2");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerCombo.SelectedItem is not Customer customer)
        {
            MessageBox.Show("请选择客户。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_details.Count == 0)
        {
            MessageBox.Show("请添加至少一个产品行。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var statusItem = StatusCombo.SelectedItem as ComboBoxItem;
        var paymentItem = PaymentStatusCombo.SelectedItem as ComboBoxItem;

        try
        {
            using var db = AppDbContextFactory.Create();
            if (_order == null)
            {
                var order = new SalesOrder
                {
                    OrderNumber = OrderNumberText.Text,
                    CustomerId = customer.Id,
                    CreatedByUserId = CurrentSession.CurrentUserId,
                    OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Now,
                    DeliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Now.AddDays(7),
                    Status = statusItem?.Content?.ToString() ?? "Pending",
                    PaymentStatus = paymentItem?.Content?.ToString() ?? "Unpaid",
                    Remarks = RemarksBox.Text.Trim(),
                    TotalAmount = _details.Sum(d => d.LineAmount),
                    FinalAmount = _details.Sum(d => d.LineAmount),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                foreach (var row in _details)
                {
                    order.OrderDetails.Add(new SalesOrderDetail
                    {
                        ProductId = row.ProductId,
                        Quantity = row.Quantity,
                        UnitPrice = row.UnitPrice,
                        DiscountPercentage = row.DiscountPercentage,
                        LineAmount = row.LineAmount,
                        Remarks = row.Remarks
                    });
                }
                db.SalesOrders.Add(order);
            }
            else
            {
                var existing = db.SalesOrders.Include(o => o.OrderDetails).FirstOrDefault(o => o.Id == _order.Id);
                if (existing != null)
                {
                    existing.CustomerId = customer.Id;
                    existing.OrderDate = OrderDatePicker.SelectedDate ?? existing.OrderDate;
                    existing.DeliveryDate = DeliveryDatePicker.SelectedDate ?? existing.DeliveryDate;
                    existing.Status = statusItem?.Content?.ToString() ?? existing.Status;
                    existing.PaymentStatus = paymentItem?.Content?.ToString() ?? existing.PaymentStatus;
                    existing.Remarks = RemarksBox.Text.Trim();
                    existing.TotalAmount = _details.Sum(d => d.LineAmount);
                    existing.FinalAmount = _details.Sum(d => d.LineAmount);
                    existing.UpdatedAt = DateTime.Now;

                    db.SalesOrderDetails.RemoveRange(existing.OrderDetails);
                    foreach (var row in _details)
                    {
                        existing.OrderDetails.Add(new SalesOrderDetail
                        {
                            ProductId = row.ProductId,
                            Quantity = row.Quantity,
                            UnitPrice = row.UnitPrice,
                            DiscountPercentage = row.DiscountPercentage,
                            LineAmount = row.LineAmount,
                            Remarks = row.Remarks
                        });
                    }
                }
            }
            db.SaveChanges();
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
