using System.Windows;
using System.Windows.Controls;

namespace SlipperIS.UI.Views;

public class StockReportItem
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsLowStock => StockQuantity < MinStockLevel;
    public string StockStatus => IsLowStock ? "⚠️ 库存不足" : "✅ 正常";
    public decimal StockValue => StockQuantity * CostPrice;
}

public partial class StockReportWindow : Window
{
    private List<StockReportItem> _allItems = new();

    public StockReportWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allItems = db.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductCode)
                .Select(p => new StockReportItem
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Category = p.Category,
                    StockQuantity = p.StockQuantity,
                    MinStockLevel = p.MinStockLevel,
                    CostPrice = p.CostPrice
                }).ToList();

            var categories = new List<string> { "全部" };
            categories.AddRange(_allItems.Select(x => x.Category).Distinct().Where(c => !string.IsNullOrEmpty(c)).OrderBy(c => c));
            CategoryFilter.ItemsSource = categories;
            CategoryFilter.SelectedIndex = 0;

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载库存报表失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        var category = CategoryFilter.SelectedItem?.ToString() ?? "全部";
        var lowOnly = LowStockOnly.IsChecked == true;

        var filtered = _allItems.AsEnumerable();
        if (category != "全部") filtered = filtered.Where(x => x.Category == category);
        if (lowOnly) filtered = filtered.Where(x => x.IsLowStock);

        var list = filtered.ToList();
        StockReportGrid.ItemsSource = list;

        TotalProductsText.Text = list.Count.ToString();
        LowStockCountText.Text = list.Count(x => x.IsLowStock).ToString();
        TotalValueText.Text = list.Sum(x => x.StockValue).ToString("F2");
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilter();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }
}
