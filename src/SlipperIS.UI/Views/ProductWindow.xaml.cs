using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public partial class ProductWindow : Window
{
    private List<Product> _allProducts = new();

    public ProductWindow()
    {
        InitializeComponent();
        LoadProducts();
    }

    private void LoadProducts()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allProducts = db.Products.OrderBy(p => p.ProductCode).ToList();
            ProductGrid.ItemsSource = _allProducts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载产品失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(keyword))
        {
            ProductGrid.ItemsSource = _allProducts;
        }
        else
        {
            ProductGrid.ItemsSource = _allProducts.Where(p =>
                p.ProductCode.ToLower().Contains(keyword) ||
                p.ProductName.ToLower().Contains(keyword) ||
                p.Category.ToLower().Contains(keyword) ||
                p.Specification.ToLower().Contains(keyword)).ToList();
        }
    }

    private void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ProductEditDialog(null);
        if (dlg.ShowDialog() == true)
        {
            LoadProducts();
        }
    }

    private void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("请先选择一个产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dlg = new ProductEditDialog(selected);
        if (dlg.ShowDialog() == true)
        {
            LoadProducts();
        }
    }

    private void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("请先选择一个产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"确定要删除产品「{selected.ProductName}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var product = db.Products.Find(selected.Id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
            LoadProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
        LoadProducts();
    }
}
