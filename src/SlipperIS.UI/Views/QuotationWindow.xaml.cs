using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 报价单管理窗口 - 支持报价单的创建、发送和转换为订单
/// </summary>
public partial class QuotationWindow : Window
{
    private SlipperDbContext _db;
    private List<QuotationViewModel> _allQuotations = new();
    private List<QuoteDetailLine> _newQuoteLines = new();

    public QuotationWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadQuotations();
    }

    private void LoadQuotations()
    {
        var quotations = _db.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .OrderByDescending(q => q.QuotationDate)
            .ToList();

        _allQuotations = quotations.Select(q => new QuotationViewModel(q)).ToList();
        ApplyFilter();
        StatusText.Text = $"共 {_allQuotations.Count} 条报价单";
    }

    private void ApplyFilter()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部";

        var filtered = _allQuotations.AsEnumerable();
        if (!string.IsNullOrEmpty(keyword))
            filtered = filtered.Where(q => q.QuotationNumber.ToLower().Contains(keyword)
                                        || q.CustomerName.ToLower().Contains(keyword));
        if (status != "全部")
            filtered = filtered.Where(q => q.Status == status);

        QuotationGrid.ItemsSource = filtered.ToList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadQuotations();
    }

    private void NewQuotation_Click(object sender, RoutedEventArgs e)
    {
        _newQuoteLines.Clear();
        DetailGrid.ItemsSource = null;
        TxtRemarks.Text = string.Empty;
        DpValidUntil.SelectedDate = DateTime.Today.AddDays(30);
        TxtQuoteTotal.Text = "合计：¥0.00";

        CmbCustomer.ItemsSource = _db.Customers.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToList();
        CmbProduct.ItemsSource = _db.Products.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.ProductName).ToList();

        NewQuotePanel.Visibility = Visibility.Visible;
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

        decimal.TryParse(TxtUnitPrice.Text, out var price);

        if (!decimal.TryParse(TxtDiscount.Text, out var discount))
        {
            MessageBox.Show("折扣格式无效，将使用 0%。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            discount = 0;
        }

        var unitPrice = price > 0 ? price : product.SalesPrice;
        var lineAmount = unitPrice * qty * (1 - discount / 100);

        var line = new QuoteDetailLine
        {
            ProductId = product.Id,
            ProductName = product.ProductName,
            Quantity = qty,
            UnitPrice = unitPrice,
            DiscountPercentage = discount,
            LineAmount = Math.Round(lineAmount, 2)
        };

        _newQuoteLines.Add(line);
        DetailGrid.ItemsSource = null;
        DetailGrid.ItemsSource = _newQuoteLines;
        UpdateTotal();
    }

    private void RemoveDetailLine_Click(object sender, RoutedEventArgs e)
    {
        if (DetailGrid.SelectedItem is QuoteDetailLine line)
        {
            _newQuoteLines.Remove(line);
            DetailGrid.ItemsSource = null;
            DetailGrid.ItemsSource = _newQuoteLines;
            UpdateTotal();
        }
    }

    private void UpdateTotal()
    {
        var total = _newQuoteLines.Sum(l => l.LineAmount);
        TxtQuoteTotal.Text = $"合计：¥{total:F2}";
    }

    private void SaveQuotation_Click(object sender, RoutedEventArgs e)
    {
        if (CmbCustomer.SelectedItem is not Customer customer)
        {
            MessageBox.Show("请选择客户。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_newQuoteLines.Count == 0)
        {
            MessageBox.Show("请至少添加一个报价明细。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var quoteNumber = $"QT{DateTime.Now:yyyyMMddHHmmssfff}";
        var total = _newQuoteLines.Sum(l => l.UnitPrice * l.Quantity);
        var finalAmount = _newQuoteLines.Sum(l => l.LineAmount);

        var quotation = new Quotation
        {
            QuotationNumber = quoteNumber,
            CustomerId = customer.Id,
            CreatedByUserId = CurrentSession.UserId,
            QuotationDate = DateTime.Now,
            ValidUntilDate = DpValidUntil.SelectedDate ?? DateTime.Today.AddDays(30),
            TotalAmount = total,
            DiscountAmount = total - finalAmount,
            FinalAmount = finalAmount,
            Status = "Draft",
            Remarks = TxtRemarks.Text.Trim(),
            ShowDiscount = ChkShowDiscount.IsChecked == true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        foreach (var line in _newQuoteLines)
        {
            quotation.QuotationDetails.Add(new QuotationDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                OriginalPrice = line.UnitPrice,
                DiscountPercentage = line.DiscountPercentage,
                DiscountedAmount = line.UnitPrice * line.Quantity - line.LineAmount,
                LineAmount = line.LineAmount
            });
        }

        _db.Quotations.Add(quotation);
        _db.SaveChanges();

        NewQuotePanel.Visibility = Visibility.Collapsed;
        LoadQuotations();
        StatusText.Text = $"报价单 {quoteNumber} 已创建";
    }

    private void CancelQuotation_Click(object sender, RoutedEventArgs e)
    {
        NewQuotePanel.Visibility = Visibility.Collapsed;
    }

    private void SendQuotation_Click(object sender, RoutedEventArgs e)
    {
        if (QuotationGrid.SelectedItem is not QuotationViewModel selected)
        {
            MessageBox.Show("请先选择一条报价单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (selected.Status != "Draft")
        {
            MessageBox.Show("只能发送草稿状态的报价单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var quotation = _db.Quotations.Find(selected.Id);
        if (quotation != null)
        {
            quotation.Status = "Sent";
            quotation.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
            LoadQuotations();
            StatusText.Text = $"报价单 {selected.QuotationNumber} 已标记为已发送";
        }
    }

    private void ConvertToOrder_Click(object sender, RoutedEventArgs e)
    {
        if (QuotationGrid.SelectedItem is not QuotationViewModel selected)
        {
            MessageBox.Show("请先选择一条报价单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (selected.Status == "Rejected" || selected.ConvertedToOrderId.HasValue)
        {
            MessageBox.Show("该报价单不能转换为订单。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"确定要将报价单 {selected.QuotationNumber} 转为销售订单吗？", "确认",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var quotation = _db.Quotations.Include(q => q.QuotationDetails).FirstOrDefault(q => q.Id == selected.Id);
            if (quotation != null)
            {
                var orderNumber = $"SO{DateTime.Now:yyyyMMddHHmmssfff}";
                var order = new SalesOrder
                {
                    OrderNumber = orderNumber,
                    CustomerId = quotation.CustomerId,
                    CreatedByUserId = CurrentSession.UserId,
                    OrderDate = DateTime.Now,
                    DeliveryDate = DateTime.Today.AddDays(7),
                    TotalAmount = quotation.TotalAmount,
                    FinalAmount = quotation.FinalAmount,
                    Status = "Pending",
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                foreach (var detail in quotation.QuotationDetails)
                {
                    order.OrderDetails.Add(new SalesOrderDetail
                    {
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        LineAmount = detail.LineAmount
                    });
                }

                _db.SalesOrders.Add(order);
                _db.SaveChanges();

                quotation.Status = "Accepted";
                quotation.ConvertedToOrderId = order.Id;
                quotation.UpdatedAt = DateTime.Now;
                _db.SaveChanges();

                LoadQuotations();
                StatusText.Text = $"已转为销售订单 {orderNumber}";
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
/// 报价单视图模型
/// </summary>
public class QuotationViewModel
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime ValidUntilDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public int? ConvertedToOrderId { get; set; }

    public QuotationViewModel(Quotation q)
    {
        Id = q.Id;
        QuotationNumber = q.QuotationNumber;
        CustomerName = q.Customer?.CustomerName ?? string.Empty;
        QuotationDate = q.QuotationDate;
        ValidUntilDate = q.ValidUntilDate;
        TotalAmount = q.TotalAmount;
        FinalAmount = q.FinalAmount;
        Status = q.Status;
        Remarks = q.Remarks;
        ConvertedToOrderId = q.ConvertedToOrderId;
    }
}

/// <summary>
/// 新建报价明细行
/// </summary>
public class QuoteDetailLine
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal LineAmount { get; set; }
}
