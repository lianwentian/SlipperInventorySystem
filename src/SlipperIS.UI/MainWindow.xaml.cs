using System.Windows;

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
            MessageBox.Show("产品管理功能", "提示");
        }

        private void Stock_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("库存管理功能", "提示");
        }

        private void SalesOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("销售订单功能", "提示");
        }

        private void Customer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("客户管理功能", "提示");
        }

        private void Quotation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("报价单功能", "提示");
        }

        private void SalesReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("销售报表功能", "提示");
        }

        private void StockReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("库存报表功能", "提示");
        }

        private void ProductImage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("产品图片预览功能", "提示");
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("用户管理功能", "提示");
        }

        private void Role_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("角色权限功能", "提示");
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
