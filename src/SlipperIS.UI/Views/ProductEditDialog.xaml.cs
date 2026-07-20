using System.Windows;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public partial class ProductEditDialog : Window
{
    private readonly Product? _product;

    public ProductEditDialog(Product? product)
    {
        InitializeComponent();
        _product = product;
        if (product != null)
        {
            Title = "编辑产品";
            CodeBox.Text = product.ProductCode;
            NameBox.Text = product.ProductName;
            CategoryBox.Text = product.Category;
            SpecBox.Text = product.Specification;
            UnitBox.Text = product.Unit;
            CostBox.Text = product.CostPrice.ToString("F2");
            SalePriceBox.Text = product.SalesPrice.ToString("F2");
            VipPriceBox.Text = product.VipPrice.ToString("F2");
            MinStockBox.Text = product.MinStockLevel.ToString();
            DescBox.Text = product.Description;
            ActiveCheck.IsChecked = product.IsActive;
        }
        else
        {
            Title = "添加产品";
            UnitBox.Text = "Pair";
            MinStockBox.Text = "10";
            ActiveCheck.IsChecked = true;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeBox.Text))
        {
            MessageBox.Show("请输入产品代码。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("请输入产品名称。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(CostBox.Text, out var cost) || cost < 0)
        {
            MessageBox.Show("请输入有效的成本价。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(SalePriceBox.Text, out var salePrice) || salePrice < 0)
        {
            MessageBox.Show("请输入有效的销售价。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(VipPriceBox.Text.Replace(string.Empty, "0"), out var vipPrice))
            vipPrice = 0;
        if (!int.TryParse(MinStockBox.Text, out var minStock))
            minStock = 10;

        decimal.TryParse(string.IsNullOrWhiteSpace(VipPriceBox.Text) ? "0" : VipPriceBox.Text, out vipPrice);

        try
        {
            using var db = AppDbContextFactory.Create();
            if (_product == null)
            {
                var newProduct = new Product
                {
                    ProductCode = CodeBox.Text.Trim(),
                    ProductName = NameBox.Text.Trim(),
                    Category = CategoryBox.Text.Trim(),
                    Specification = SpecBox.Text.Trim(),
                    Unit = string.IsNullOrWhiteSpace(UnitBox.Text) ? "Pair" : UnitBox.Text.Trim(),
                    CostPrice = cost,
                    SalesPrice = salePrice,
                    VipPrice = vipPrice,
                    MinStockLevel = minStock,
                    Description = DescBox.Text.Trim(),
                    IsActive = ActiveCheck.IsChecked == true,
                    StockQuantity = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.Products.Add(newProduct);
            }
            else
            {
                var existing = db.Products.Find(_product.Id);
                if (existing != null)
                {
                    existing.ProductCode = CodeBox.Text.Trim();
                    existing.ProductName = NameBox.Text.Trim();
                    existing.Category = CategoryBox.Text.Trim();
                    existing.Specification = SpecBox.Text.Trim();
                    existing.Unit = string.IsNullOrWhiteSpace(UnitBox.Text) ? "Pair" : UnitBox.Text.Trim();
                    existing.CostPrice = cost;
                    existing.SalesPrice = salePrice;
                    existing.VipPrice = vipPrice;
                    existing.MinStockLevel = minStock;
                    existing.Description = DescBox.Text.Trim();
                    existing.IsActive = ActiveCheck.IsChecked == true;
                    existing.UpdatedAt = DateTime.Now;
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

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
