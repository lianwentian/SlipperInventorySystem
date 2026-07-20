using System.Windows;
using System.Windows.Controls;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class StockProductViewModel
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Specification { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public decimal SalesPrice { get; set; }
    public bool IsLowStock => StockQuantity < MinStockLevel;
    public string StockStatus => IsLowStock ? "⚠️ 库存不足" : "✅ 正常";
}

public partial class StockWindow : Window
{
    private List<StockProductViewModel> _allItems = new();

    public StockWindow()
    {
        InitializeComponent();
        LoadStock();
    }

    private void LoadStock()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allItems = db.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductCode)
                .Select(p => new StockProductViewModel
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Specification = p.Specification,
                    Unit = p.Unit,
                    StockQuantity = p.StockQuantity,
                    MinStockLevel = p.MinStockLevel,
                    SalesPrice = p.SalesPrice
                }).ToList();

            StockGrid.ItemsSource = _allItems;

            var lowCount = _allItems.Count(x => x.IsLowStock);
            if (lowCount > 0)
            {
                LowStockBanner.Visibility = Visibility.Visible;
                LowStockText.Text = $"⚠️ 警告：有 {lowCount} 个产品库存低于最低库存水平！";
            }
            else
            {
                LowStockBanner.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载库存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        StockGrid.ItemsSource = string.IsNullOrEmpty(keyword)
            ? _allItems
            : _allItems.Where(p =>
                p.ProductCode.ToLower().Contains(keyword) ||
                p.ProductName.ToLower().Contains(keyword)).ToList();
    }

    private void StockIn_Click(object sender, RoutedEventArgs e)
    {
        if (StockGrid.SelectedItem is not StockProductViewModel selected)
        {
            MessageBox.Show("请先选择一个产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        AdjustStockDialog("入库", selected, "In");
    }

    private void StockOut_Click(object sender, RoutedEventArgs e)
    {
        if (StockGrid.SelectedItem is not StockProductViewModel selected)
        {
            MessageBox.Show("请先选择一个产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        AdjustStockDialog("出库", selected, "Out");
    }

    private void AdjustStock_Click(object sender, RoutedEventArgs e)
    {
        if (StockGrid.SelectedItem is not StockProductViewModel selected)
        {
            MessageBox.Show("请先选择一个产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        AdjustStockDialog("调整库存", selected, "Adjustment");
    }

    private void AdjustStockDialog(string title, StockProductViewModel item, string type)
    {
        var dlg = new StockAdjustDialog(title, item.ProductName, item.StockQuantity, type);
        if (dlg.ShowDialog() == true)
        {
            try
            {
                using var db = AppDbContextFactory.Create();
                var product = db.Products.Find(item.Id);
                if (product == null) return;

                int qty = dlg.Quantity;
                if (type == "In") product.StockQuantity += qty;
                else if (type == "Out") product.StockQuantity = Math.Max(0, product.StockQuantity - qty);
                else product.StockQuantity = qty; // Adjustment = set to value

                product.UpdatedAt = DateTime.Now;

                db.StockRecords.Add(new StockRecord
                {
                    ProductId = product.Id,
                    Quantity = qty,
                    Type = type,
                    Reason = dlg.Reason,
                    CreatedByUserId = CurrentSession.CurrentUserId,
                    CreatedAt = DateTime.Now
                });

                db.SaveChanges();
                LoadStock();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"库存调整失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
        LoadStock();
    }
}
