using System.Windows;
using System.Windows.Controls;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public partial class CustomerWindow : Window
{
    private List<Customer> _allCustomers = new();

    public CustomerWindow()
    {
        InitializeComponent();
        LoadCustomers();
    }

    private void LoadCustomers()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allCustomers = db.Customers.OrderBy(c => c.CustomerCode).ToList();
            CustomerGrid.ItemsSource = _allCustomers;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载客户失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        CustomerGrid.ItemsSource = string.IsNullOrEmpty(keyword)
            ? _allCustomers
            : _allCustomers.Where(c =>
                c.CustomerCode.ToLower().Contains(keyword) ||
                c.CustomerName.ToLower().Contains(keyword) ||
                c.ContactPerson.ToLower().Contains(keyword) ||
                c.PhoneNumber.ToLower().Contains(keyword)).ToList();
    }

    private void AddCustomer_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CustomerEditDialog(null);
        if (dlg.ShowDialog() == true) LoadCustomers();
    }

    private void EditCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is not Customer selected)
        {
            MessageBox.Show("请先选择一个客户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dlg = new CustomerEditDialog(selected);
        if (dlg.ShowDialog() == true) LoadCustomers();
    }

    private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is not Customer selected)
        {
            MessageBox.Show("请先选择一个客户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"确定要删除客户「{selected.CustomerName}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var customer = db.Customers.Find(selected.Id);
            if (customer != null)
            {
                db.Customers.Remove(customer);
                db.SaveChanges();
            }
            LoadCustomers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
        LoadCustomers();
    }
}
