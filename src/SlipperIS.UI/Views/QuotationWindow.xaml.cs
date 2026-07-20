using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class QuotationViewModel
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime ValidUntilDate { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ConvertedOrderId { get; set; }
}

public partial class QuotationWindow : Window
{
    private List<QuotationViewModel> _allQuotations = new();

    public QuotationWindow()
    {
        InitializeComponent();
        LoadQuotations();
    }

    private void LoadQuotations()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allQuotations = db.Quotations
                .Include(q => q.Customer)
                .OrderByDescending(q => q.QuotationDate)
                .Select(q => new QuotationViewModel
                {
                    Id = q.Id,
                    QuotationNumber = q.QuotationNumber,
                    CustomerName = q.Customer != null ? q.Customer.CustomerName : string.Empty,
                    QuotationDate = q.QuotationDate,
                    ValidUntilDate = q.ValidUntilDate,
                    FinalAmount = q.FinalAmount,
                    Status = q.Status,
                    ConvertedOrderId = q.ConvertedToOrderId
                }).ToList();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载报价单失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        var statusItem = StatusFilter.SelectedItem as ComboBoxItem;
        var status = statusItem?.Content?.ToString() ?? "全部";
        QuotationGrid.ItemsSource = status == "全部"
            ? _allQuotations
            : _allQuotations.Where(q => q.Status == status).ToList();
    }

    private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void NewQuotation_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new QuotationEditDialog(null);
        if (dlg.ShowDialog() == true) LoadQuotations();
    }

    private void ViewQuotation_Click(object sender, RoutedEventArgs e)
    {
        if (QuotationGrid.SelectedItem is not QuotationViewModel selected)
        {
            MessageBox.Show("请先选择一个报价单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        using var db = AppDbContextFactory.Create();
        var quotation = db.Quotations
            .Include(q => q.Customer)
            .Include(q => q.QuotationDetails).ThenInclude(d => d.Product)
            .FirstOrDefault(q => q.Id == selected.Id);
        if (quotation != null)
        {
            var dlg = new QuotationEditDialog(quotation);
            if (dlg.ShowDialog() == true) LoadQuotations();
        }
    }

    private void ConvertToOrder_Click(object sender, RoutedEventArgs e)
    {
        if (QuotationGrid.SelectedItem is not QuotationViewModel selected)
        {
            MessageBox.Show("请先选择一个报价单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (selected.Status == "Converted")
        {
            MessageBox.Show("该报价单已转换为销售订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"确定将报价单「{selected.QuotationNumber}」转换为销售订单吗？", "确认",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var quotation = db.Quotations.Include(q => q.QuotationDetails).FirstOrDefault(q => q.Id == selected.Id);
            if (quotation == null) return;

            var now = DateTime.Now;
            var order = new SalesOrder
            {
                OrderNumber = $"SO{now:yyyyMMddHHmmss}",
                CustomerId = quotation.CustomerId,
                CreatedByUserId = CurrentSession.CurrentUserId,
                OrderDate = now,
                DeliveryDate = now.AddDays(7),
                Status = "Pending",
                PaymentStatus = "Unpaid",
                Remarks = $"由报价单 {quotation.QuotationNumber} 转换",
                TotalAmount = quotation.TotalAmount,
                DiscountAmount = quotation.DiscountAmount,
                FinalAmount = quotation.FinalAmount,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var detail in quotation.QuotationDetails)
            {
                order.OrderDetails.Add(new SalesOrderDetail
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    DiscountPercentage = detail.DiscountPercentage,
                    LineAmount = detail.LineAmount,
                    Remarks = detail.Remarks
                });
            }

            db.SalesOrders.Add(order);
            db.SaveChanges();

            quotation.Status = "Converted";
            quotation.ConvertedToOrderId = order.Id;
            quotation.UpdatedAt = now;
            db.SaveChanges();

            MessageBox.Show($"已成功转换为销售订单 {order.OrderNumber}。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadQuotations();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"转换失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteQuotation_Click(object sender, RoutedEventArgs e)
    {
        if (QuotationGrid.SelectedItem is not QuotationViewModel selected)
        {
            MessageBox.Show("请先选择一个报价单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"确定要删除报价单「{selected.QuotationNumber}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var quotation = db.Quotations.Find(selected.Id);
            if (quotation != null)
            {
                db.Quotations.Remove(quotation);
                db.SaveChanges();
            }
            LoadQuotations();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadQuotations();
    }
}
