using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 客户管理窗口 - 支持客户的增删改查和搜索
/// </summary>
public partial class CustomerWindow : Window
{
    private SlipperDbContext _db;
    private List<Customer> _allCustomers = new();
    private int _editingId = 0;

    public CustomerWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadCustomers();
    }

    private void LoadCustomers()
    {
        _allCustomers = _db.Customers.AsNoTracking().OrderBy(c => c.CustomerCode).ToList();
        ApplySearch();
        StatusText.Text = $"共 {_allCustomers.Count} 条记录";
    }

    private void ApplySearch()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(keyword))
            CustomerGrid.ItemsSource = _allCustomers;
        else
            CustomerGrid.ItemsSource = _allCustomers
                .Where(c => c.CustomerCode.ToLower().Contains(keyword)
                         || c.CustomerName.ToLower().Contains(keyword)
                         || c.ContactPerson.ToLower().Contains(keyword)
                         || c.City.ToLower().Contains(keyword))
                .ToList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplySearch();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadCustomers();
    }

    private void AddCustomer_Click(object sender, RoutedEventArgs e)
    {
        _editingId = 0;
        EditTitle.Text = "添加客户";
        ClearForm();
        EditPanel.Visibility = Visibility.Visible;
    }

    private void EditCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is not Customer selected)
        {
            MessageBox.Show("请先选择一条客户记录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _editingId = selected.Id;
        EditTitle.Text = "编辑客户";
        TxtCode.Text = selected.CustomerCode;
        TxtName.Text = selected.CustomerName;
        TxtContact.Text = selected.ContactPerson;
        TxtPhone.Text = selected.PhoneNumber;
        TxtEmail.Text = selected.Email;
        TxtCity.Text = selected.City;
        TxtAddress.Text = selected.Address;
        ChkVip.IsChecked = selected.IsVip;
        ChkActive.IsChecked = selected.IsActive;
        EditPanel.Visibility = Visibility.Visible;
    }

    private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is not Customer selected)
        {
            MessageBox.Show("请先选择一条客户记录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"确定要删除客户「{selected.CustomerName}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var customer = _db.Customers.Find(selected.Id);
            if (customer != null)
            {
                _db.Customers.Remove(customer);
                _db.SaveChanges();
                LoadCustomers();
                StatusText.Text = "客户已删除";
            }
        }
    }

    private void SaveCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtCode.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("客户编码和客户名称为必填项。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingId == 0)
        {
            var customer = new Customer
            {
                CustomerCode = TxtCode.Text.Trim(),
                CustomerName = TxtName.Text.Trim(),
                ContactPerson = TxtContact.Text.Trim(),
                PhoneNumber = TxtPhone.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                City = TxtCity.Text.Trim(),
                Address = TxtAddress.Text.Trim(),
                IsVip = ChkVip.IsChecked == true,
                IsActive = ChkActive.IsChecked == true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _db.Customers.Add(customer);
            _db.SaveChanges();
            StatusText.Text = "客户已添加";
        }
        else
        {
            var customer = _db.Customers.Find(_editingId);
            if (customer != null)
            {
                customer.CustomerCode = TxtCode.Text.Trim();
                customer.CustomerName = TxtName.Text.Trim();
                customer.ContactPerson = TxtContact.Text.Trim();
                customer.PhoneNumber = TxtPhone.Text.Trim();
                customer.Email = TxtEmail.Text.Trim();
                customer.City = TxtCity.Text.Trim();
                customer.Address = TxtAddress.Text.Trim();
                customer.IsVip = ChkVip.IsChecked == true;
                customer.IsActive = ChkActive.IsChecked == true;
                customer.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                StatusText.Text = "客户已更新";
            }
        }

        EditPanel.Visibility = Visibility.Collapsed;
        LoadCustomers();
    }

    private void CancelEdit_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Collapsed;
    }

    private void ClearForm()
    {
        TxtCode.Text = string.Empty;
        TxtName.Text = string.Empty;
        TxtContact.Text = string.Empty;
        TxtPhone.Text = string.Empty;
        TxtEmail.Text = string.Empty;
        TxtCity.Text = string.Empty;
        TxtAddress.Text = string.Empty;
        ChkVip.IsChecked = false;
        ChkActive.IsChecked = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}
