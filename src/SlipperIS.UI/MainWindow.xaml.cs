using System.Windows;
using SlipperIS.UI.Views;

namespace SlipperIS.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            new ProductWindow { Owner = this }.ShowDialog();
        }

        private void Stock_Click(object sender, RoutedEventArgs e)
        {
            new StockWindow { Owner = this }.ShowDialog();
        }

        private void SalesOrder_Click(object sender, RoutedEventArgs e)
        {
            new SalesOrderWindow { Owner = this }.ShowDialog();
        }

        private void Customer_Click(object sender, RoutedEventArgs e)
        {
            new CustomerWindow { Owner = this }.ShowDialog();
        }

        private void Quotation_Click(object sender, RoutedEventArgs e)
        {
            new QuotationWindow { Owner = this }.ShowDialog();
        }

        private void SalesReport_Click(object sender, RoutedEventArgs e)
        {
            new SalesReportWindow { Owner = this }.ShowDialog();
        }

        private void StockReport_Click(object sender, RoutedEventArgs e)
        {
            new StockReportWindow { Owner = this }.ShowDialog();
        }

        private void ProductImage_Click(object sender, RoutedEventArgs e)
        {
            new ProductImageWindow { Owner = this }.ShowDialog();
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            new UserWindow { Owner = this }.ShowDialog();
        }

        private void Role_Click(object sender, RoutedEventArgs e)
        {
            new RoleWindow { Owner = this }.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("拖鞋库存管理系统 v1.0\n\n" +
                          "功能特性：\n" +
                          "✓ 产品库存管理\n" +
                          "✓ 销售订单管理\n" +
                          "✓ 报价单生成\n" +
                          "✓ 产品图片预览\n" +
                          "✓ 报表分析\n" +
                          "✓ 权限管理\n\n" +
                          "版本: 1.0.0\n" +
                          "2026年7月", "关于系统");
        }
    }
}
