using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public partial class ProductImageWindow : Window
{
    private List<Product> _products = new();
    private Product? _selectedProduct;

    public ProductImageWindow()
    {
        InitializeComponent();
        LoadProducts();
    }

    private void LoadProducts()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _products = db.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToList();
            ProductList.ItemsSource = _products;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载产品失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedProduct = ProductList.SelectedItem as Product;
        if (_selectedProduct == null) return;
        LoadImages(_selectedProduct.Id);
    }

    private void LoadImages(int productId)
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            var images = db.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .ToList();
            ImageList.ItemsSource = images;
            PreviewImage.Source = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载图片失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ImageList.SelectedItem is not ProductImage image) return;
        try
        {
            if (File.Exists(image.ImagePath))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(image.ImagePath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                PreviewImage.Source = bmp;
            }
            else
            {
                PreviewImage.Source = null;
                MessageBox.Show("图片文件不存在。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch
        {
            PreviewImage.Source = null;
        }
    }

    private void AddImage_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedProduct == null)
        {
            MessageBox.Show("请先从左侧选择一个产品。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new OpenFileDialog
        {
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
            Multiselect = true,
            Title = "选择产品图片"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var maxOrder = db.ProductImages
                .Where(i => i.ProductId == _selectedProduct.Id)
                .Select(i => (int?)i.DisplayOrder)
                .Max() ?? 0;

            foreach (var filePath in dlg.FileNames)
            {
                maxOrder++;
                db.ProductImages.Add(new ProductImage
                {
                    ProductId = _selectedProduct.Id,
                    ImagePath = filePath,
                    ImageName = Path.GetFileNameWithoutExtension(filePath),
                    DisplayOrder = maxOrder,
                    IsMainImage = maxOrder == 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            db.SaveChanges();
            LoadImages(_selectedProduct.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"添加图片失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteImage_Click(object sender, RoutedEventArgs e)
    {
        if (ImageList.SelectedItem is not ProductImage image)
        {
            MessageBox.Show("请先选择一张图片。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"确定删除图片「{image.ImageName}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var img = db.ProductImages.Find(image.Id);
            if (img != null)
            {
                db.ProductImages.Remove(img);
                db.SaveChanges();
            }
            if (_selectedProduct != null)
                LoadImages(_selectedProduct.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
