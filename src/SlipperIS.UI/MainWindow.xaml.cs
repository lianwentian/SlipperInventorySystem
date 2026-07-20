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
            OpenWindow<ProductWindow>();
        }

        private void Stock_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<StockWindow>();
        }

        private void SalesOrder_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<SalesOrderWindow>();
        }

        private void Customer_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<CustomerWindow>();
        }

        private void Quotation_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<QuotationWindow>();
        }

        private void SalesReport_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<SalesReportWindow>();
        }

        private void StockReport_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<StockReportWindow>();
        }

        private void ProductImage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("产品图片预览功能即将推出。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<UserWindow>();
        }

        private void Role_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<RoleWindow>();
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

        /// <summary>
        /// 通用方法：打开一个功能窗口，异常时显示错误信息
        /// </summary>
        private static void OpenWindow<T>() where T : Window, new()
        {
            try
            {
                var window = new T();
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开窗口时发生错误：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
