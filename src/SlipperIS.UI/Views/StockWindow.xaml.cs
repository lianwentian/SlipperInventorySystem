using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 库存管理窗口 - 显示库存状态、库存预警和调整功能
/// </summary>
public partial class StockWindow : Window
{
    private SlipperDbContext _db;
    private List<StockViewModel> _allItems = new();
    private int _selectedProductId = 0;

    public StockWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadStock();
    }

    private void LoadStock()
    {
        var products = _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductCode)
            .ToList();

        _allItems = products.Select(p => new StockViewModel(p)).ToList();
        ApplyFilter();

        var lowCount = _allItems.Count(x => x.IsLowStock);
        StatusText.Text = $"共 {_allItems.Count} 种产品，其中 {lowCount} 种库存不足";
    }

    private void ApplyFilter()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var lowOnly = ChkLowStock.IsChecked == true;

        var filtered = _allItems.AsEnumerable();
        if (!string.IsNullOrEmpty(keyword))
            filtered = filtered.Where(x => x.ProductCode.ToLower().Contains(keyword)
                                        || x.ProductName.ToLower().Contains(keyword));
        if (lowOnly)
            filtered = filtered.Where(x => x.IsLowStock);

        StockGrid.ItemsSource = filtered.ToList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void FilterChanged(object sender, RoutedEventArgs e) => ApplyFilter();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadStock();
    }

    private void AdjustStock_Click(object sender, RoutedEventArgs e)
    {
        if (StockGrid.SelectedItem is not StockViewModel selected)
        {
            MessageBox.Show("请先选择一条库存记录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _selectedProductId = selected.ProductId;
        TxtAdjProduct.Text = $"{selected.ProductCode} - {selected.ProductName}";
        TxtAdjQty.Text = "0";
        TxtAdjReason.Text = string.Empty;
        AdjustPanel.Visibility = Visibility.Visible;
    }

    private void ConfirmAdjust_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtAdjQty.Text, out var qty) || qty == 0)
        {
            MessageBox.Show("请输入有效的调整数量（不能为0）。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var typeItem = CmbAdjType.SelectedItem as ComboBoxItem;
        var typeStr = typeItem?.Content?.ToString() ?? string.Empty;
        var parts = typeStr.Split(' ');
        if (parts.Length < 2)
        {
            MessageBox.Show("请选择有效的调整类型。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var typePart = parts[1].TrimStart('(').TrimEnd(')');

        var product = _db.Products.Find(_selectedProductId);
        if (product == null) return;

        int newQty;
        try
        {
            newQty = typePart switch
            {
                "In" => product.StockQuantity + qty,
                "Out" => product.StockQuantity - qty,
                "Adjustment" => qty, // 直接设置库存数量
                _ => throw new InvalidOperationException($"未知的调整类型：{typePart}")
            };
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newQty < 0)
        {
            MessageBox.Show("出库数量不能超过当前库存。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 创建库存记录
        var record = new StockRecord
        {
            ProductId = _selectedProductId,
            Quantity = qty,
            Type = typePart,
            Reason = TxtAdjReason.Text.Trim(),
            CreatedByUserId = CurrentSession.UserId,
            CreatedAt = DateTime.Now
        };
        _db.StockRecords.Add(record);

        // 更新产品库存
        product.StockQuantity = newQty;
        product.UpdatedAt = DateTime.Now;

        _db.SaveChanges();
        AdjustPanel.Visibility = Visibility.Collapsed;
        LoadStock();
        StatusText.Text = $"库存已调整：{product.ProductName} 当前库存 {newQty}";
    }

    private void CancelAdjust_Click(object sender, RoutedEventArgs e)
    {
        AdjustPanel.Visibility = Visibility.Collapsed;
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}

/// <summary>
/// 库存视图模型 - 用于 DataGrid 显示
/// </summary>
public class StockViewModel
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Specification { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsLowStock => StockQuantity < MinStockLevel;
    public string StockStatus => IsLowStock ? "⚠️ 库存不足" : "✅ 正常";
    public decimal StockValue => StockQuantity * CostPrice;

    public StockViewModel(Product p)
    {
        ProductId = p.Id;
        ProductCode = p.ProductCode;
        ProductName = p.ProductName;
        Category = p.Category;
        Specification = p.Specification;
        StockQuantity = p.StockQuantity;
        MinStockLevel = p.MinStockLevel;
        CostPrice = p.CostPrice;
    }
}
