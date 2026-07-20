using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 库存报表窗口 - 显示库存统计数据
/// </summary>
public partial class StockReportWindow : Window
{
    private SlipperDbContext _db;
    private List<StockReportItem> _allItems = new();

    public StockReportWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadReport();
    }

    private void LoadReport()
    {
        var products = _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductCode)
            .ToList();

        _allItems = products.Select(p => new StockReportItem(p)).ToList();

        // 更新类别下拉框
        var currentCategory = (CmbCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部";
        CmbCategory.Items.Clear();
        CmbCategory.Items.Add(new ComboBoxItem { Content = "全部", IsSelected = true });
        foreach (var cat in _allItems.Select(x => x.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c))
            CmbCategory.Items.Add(new ComboBoxItem { Content = cat });

        ApplyFilter(currentCategory);
    }

    private void ApplyFilter(string? category = null)
    {
        category ??= (CmbCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部";

        var filtered = category == "全部"
            ? _allItems
            : _allItems.Where(x => x.Category == category).ToList();

        ReportGrid.ItemsSource = filtered;

        TxtProductCount.Text = filtered.Count.ToString();
        TxtTotalQty.Text = filtered.Sum(x => x.StockQuantity).ToString();
        TxtTotalValue.Text = $"¥{filtered.Sum(x => x.StockValue):F2}";
        TxtLowStockCount.Text = filtered.Count(x => x.IsLowStock).ToString();
        StatusText.Text = $"共 {filtered.Count} 种产品，库存总价值 ¥{filtered.Sum(x => x.StockValue):F2}";
    }

    private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadReport();
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}

/// <summary>
/// 库存报表行数据
/// </summary>
public class StockReportItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal StockValue => StockQuantity * CostPrice;
    public bool IsLowStock => StockQuantity < MinStockLevel;
    public string StatusText => IsLowStock ? "⚠️ 库存不足" : "✅ 正常";

    public StockReportItem(Product p)
    {
        ProductCode = p.ProductCode;
        ProductName = p.ProductName;
        Category = p.Category;
        StockQuantity = p.StockQuantity;
        MinStockLevel = p.MinStockLevel;
        CostPrice = p.CostPrice;
        SalesPrice = p.SalesPrice;
    }
}
