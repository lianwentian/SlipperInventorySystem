using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 产品管理窗口 - 支持产品的增删改查和搜索
/// </summary>
public partial class ProductWindow : Window
{
    private SlipperDbContext _db;
    private List<Product> _allProducts = new();
    private int _editingId = 0; // 0 = new, >0 = editing existing

    public ProductWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadProducts();
    }

    private void LoadProducts()
    {
        _allProducts = _db.Products.AsNoTracking().OrderBy(p => p.ProductCode).ToList();
        ApplySearch();
        StatusText.Text = $"共 {_allProducts.Count} 条记录";
    }

    private void ApplySearch()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(keyword))
        {
            ProductGrid.ItemsSource = _allProducts;
        }
        else
        {
            ProductGrid.ItemsSource = _allProducts
                .Where(p => p.ProductCode.ToLower().Contains(keyword)
                         || p.ProductName.ToLower().Contains(keyword)
                         || p.Category.ToLower().Contains(keyword))
                .ToList();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplySearch();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadProducts();
    }

    private void ProductGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        _editingId = 0;
        EditTitle.Text = "添加产品";
        ClearForm();
        EditPanel.Visibility = Visibility.Visible;
    }

    private void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("请先选择一条产品记录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _editingId = selected.Id;
        EditTitle.Text = "编辑产品";
        TxtProductCode.Text = selected.ProductCode;
        TxtProductName.Text = selected.ProductName;
        TxtCategory.Text = selected.Category;
        TxtSpecification.Text = selected.Specification;
        TxtUnit.Text = selected.Unit;
        TxtCostPrice.Text = selected.CostPrice.ToString("F2");
        TxtSalesPrice.Text = selected.SalesPrice.ToString("F2");
        TxtVipPrice.Text = selected.VipPrice.ToString("F2");
        TxtStockQty.Text = selected.StockQuantity.ToString();
        TxtMinStock.Text = selected.MinStockLevel.ToString();
        TxtDescription.Text = selected.Description;
        ChkIsActive.IsChecked = selected.IsActive;
        EditPanel.Visibility = Visibility.Visible;
    }

    private void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("请先选择一条产品记录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"确定要删除产品「{selected.ProductName}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var product = _db.Products.Find(selected.Id);
            if (product != null)
            {
                _db.Products.Remove(product);
                _db.SaveChanges();
                LoadProducts();
                StatusText.Text = "产品已删除";
            }
        }
    }

    private void SaveProduct_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtProductCode.Text) || string.IsNullOrWhiteSpace(TxtProductName.Text))
        {
            MessageBox.Show("产品编码和产品名称为必填项。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtCostPrice.Text, out var cost) ||
            !decimal.TryParse(TxtSalesPrice.Text, out var sales) ||
            !decimal.TryParse(TxtVipPrice.Text, out var vip) ||
            !int.TryParse(TxtStockQty.Text, out var stockQty) ||
            !int.TryParse(TxtMinStock.Text, out var minStock))
        {
            MessageBox.Show("价格和数量必须为有效的数字。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingId == 0)
        {
            // 新增
            var product = new Product
            {
                ProductCode = TxtProductCode.Text.Trim(),
                ProductName = TxtProductName.Text.Trim(),
                Category = TxtCategory.Text.Trim(),
                Specification = TxtSpecification.Text.Trim(),
                Unit = TxtUnit.Text.Trim(),
                CostPrice = cost,
                SalesPrice = sales,
                VipPrice = vip,
                StockQuantity = stockQty,
                MinStockLevel = minStock,
                Description = TxtDescription.Text.Trim(),
                IsActive = ChkIsActive.IsChecked == true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _db.Products.Add(product);
            _db.SaveChanges();
            StatusText.Text = "产品已添加";
        }
        else
        {
            // 编辑
            var product = _db.Products.Find(_editingId);
            if (product != null)
            {
                product.ProductCode = TxtProductCode.Text.Trim();
                product.ProductName = TxtProductName.Text.Trim();
                product.Category = TxtCategory.Text.Trim();
                product.Specification = TxtSpecification.Text.Trim();
                product.Unit = TxtUnit.Text.Trim();
                product.CostPrice = cost;
                product.SalesPrice = sales;
                product.VipPrice = vip;
                product.StockQuantity = stockQty;
                product.MinStockLevel = minStock;
                product.Description = TxtDescription.Text.Trim();
                product.IsActive = ChkIsActive.IsChecked == true;
                product.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                StatusText.Text = "产品已更新";
            }
        }

        EditPanel.Visibility = Visibility.Collapsed;
        LoadProducts();
    }

    private void CancelEdit_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Collapsed;
        ClearForm();
    }

    private void ClearForm()
    {
        TxtProductCode.Text = string.Empty;
        TxtProductName.Text = string.Empty;
        TxtCategory.Text = string.Empty;
        TxtSpecification.Text = string.Empty;
        TxtUnit.Text = "Pair";
        TxtCostPrice.Text = "0";
        TxtSalesPrice.Text = "0";
        TxtVipPrice.Text = "0";
        TxtStockQty.Text = "0";
        TxtMinStock.Text = "10";
        TxtDescription.Text = string.Empty;
        ChkIsActive.IsChecked = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}
