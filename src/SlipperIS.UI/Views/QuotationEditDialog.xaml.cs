using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class QuotationDetailRow
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal LineAmount => Math.Round(UnitPrice * Quantity * (1 - DiscountPercentage / 100), 2);
    public string Remarks { get; set; } = string.Empty;
}

public partial class QuotationEditDialog : Window
{
    private readonly Quotation? _quotation;
    private List<Customer> _customers = new();
    private List<Product> _products = new();
    private ObservableCollection<QuotationDetailRow> _details = new();
    private bool _isReadOnly;

    public QuotationEditDialog(Quotation? quotation)
    {
        InitializeComponent();
        _quotation = quotation;
        _isReadOnly = quotation != null && (quotation.Status == "Converted" || quotation.Status == "Rejected");
        LoadData();
        if (quotation != null) FillForm(quotation);
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
            productCol.ItemsSource = _products;
    }

    private void InitNew()
    {
        Title = "新建报价单";
        var now = DateTime.Now;
        QuotationNumberText.Text = $"QT{now:yyyyMMddHHmmss}";
        QuotationDatePicker.SelectedDate = now;
        ValidUntilPicker.SelectedDate = now.AddDays(30);
    }

    private void FillForm(Quotation q)
    {
        Title = $"报价单详情 - {q.QuotationNumber}";
        QuotationNumberText.Text = q.QuotationNumber;
        CustomerCombo.SelectedValue = q.CustomerId;
        QuotationDatePicker.SelectedDate = q.QuotationDate;
        ValidUntilPicker.SelectedDate = q.ValidUntilDate;
        RemarksBox.Text = q.Remarks;

        foreach (ComboBoxItem item in StatusCombo.Items)
            if (item.Content?.ToString() == q.Status) { StatusCombo.SelectedItem = item; break; }

        foreach (var detail in q.QuotationDetails)
        {
            _details.Add(new QuotationDetailRow
            {
                Id = detail.Id,
                ProductId = detail.ProductId,
                ProductName = detail.Product?.ProductName ?? string.Empty,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                OriginalPrice = detail.OriginalPrice,
                DiscountPercentage = detail.DiscountPercentage,
                Remarks = detail.Remarks
            });
        }

        if (_isReadOnly)
        {
            SaveBtn.IsEnabled = false;
            CustomerCombo.IsEnabled = false;
            QuotationDatePicker.IsEnabled = false;
            ValidUntilPicker.IsEnabled = false;
            StatusCombo.IsEnabled = false;
            RemarksBox.IsReadOnly = true;
        }
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        if (_isReadOnly) return;
        if (_products.Count == 0) return;
        var first = _products[0];
        _details.Add(new QuotationDetailRow
        {
            ProductId = first.Id,
            ProductName = first.ProductName,
            UnitPrice = first.SalesPrice,
            OriginalPrice = first.SalesPrice,
            Quantity = 1
        });
        UpdateTotal();
    }

    private void DeleteLine_Click(object sender, RoutedEventArgs e)
    {
        if (_isReadOnly) return;
        if (DetailGrid.SelectedItem is QuotationDetailRow row)
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

        try
        {
            using var db = AppDbContextFactory.Create();
            var total = _details.Sum(d => d.LineAmount);

            if (_quotation == null)
            {
                var q = new Quotation
                {
                    QuotationNumber = QuotationNumberText.Text,
                    CustomerId = customer.Id,
                    CreatedByUserId = CurrentSession.CurrentUserId,
                    QuotationDate = QuotationDatePicker.SelectedDate ?? DateTime.Now,
                    ValidUntilDate = ValidUntilPicker.SelectedDate ?? DateTime.Now.AddDays(30),
                    Status = statusItem?.Content?.ToString() ?? "Draft",
                    Remarks = RemarksBox.Text.Trim(),
                    TotalAmount = total,
                    FinalAmount = total,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                foreach (var row in _details)
                {
                    q.QuotationDetails.Add(new QuotationDetail
                    {
                        ProductId = row.ProductId,
                        Quantity = row.Quantity,
                        UnitPrice = row.UnitPrice,
                        OriginalPrice = row.OriginalPrice,
                        DiscountPercentage = row.DiscountPercentage,
                        LineAmount = row.LineAmount,
                        DiscountedAmount = row.LineAmount,
                        Remarks = row.Remarks
                    });
                }
                db.Quotations.Add(q);
            }
            else
            {
                var existing = db.Quotations.Include(q2 => q2.QuotationDetails).FirstOrDefault(q2 => q2.Id == _quotation.Id);
                if (existing != null)
                {
                    existing.CustomerId = customer.Id;
                    existing.QuotationDate = QuotationDatePicker.SelectedDate ?? existing.QuotationDate;
                    existing.ValidUntilDate = ValidUntilPicker.SelectedDate ?? existing.ValidUntilDate;
                    existing.Status = statusItem?.Content?.ToString() ?? existing.Status;
                    existing.Remarks = RemarksBox.Text.Trim();
                    existing.TotalAmount = total;
                    existing.FinalAmount = total;
                    existing.UpdatedAt = DateTime.Now;

                    db.QuotationDetails.RemoveRange(existing.QuotationDetails);
                    foreach (var row in _details)
                    {
                        existing.QuotationDetails.Add(new QuotationDetail
                        {
                            ProductId = row.ProductId,
                            Quantity = row.Quantity,
                            UnitPrice = row.UnitPrice,
                            OriginalPrice = row.OriginalPrice,
                            DiscountPercentage = row.DiscountPercentage,
                            LineAmount = row.LineAmount,
                            DiscountedAmount = row.LineAmount,
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
