using System.Windows;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public partial class CustomerEditDialog : Window
{
    private readonly Customer? _customer;

    public CustomerEditDialog(Customer? customer)
    {
        InitializeComponent();
        _customer = customer;
        if (customer != null)
        {
            Title = "编辑客户";
            CodeBox.Text = customer.CustomerCode;
            NameBox.Text = customer.CustomerName;
            ContactBox.Text = customer.ContactPerson;
            PhoneBox.Text = customer.PhoneNumber;
            EmailBox.Text = customer.Email;
            CityBox.Text = customer.City;
            AddressBox.Text = customer.Address;
            VipCheck.IsChecked = customer.IsVip;
            ActiveCheck.IsChecked = customer.IsActive;
        }
        else
        {
            Title = "添加客户";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeBox.Text))
        {
            MessageBox.Show("请输入客户编码。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("请输入客户名称。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = AppDbContextFactory.Create();
            if (_customer == null)
            {
                db.Customers.Add(new Customer
                {
                    CustomerCode = CodeBox.Text.Trim(),
                    CustomerName = NameBox.Text.Trim(),
                    ContactPerson = ContactBox.Text.Trim(),
                    PhoneNumber = PhoneBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    City = CityBox.Text.Trim(),
                    Address = AddressBox.Text.Trim(),
                    IsVip = VipCheck.IsChecked == true,
                    IsActive = ActiveCheck.IsChecked == true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                var existing = db.Customers.Find(_customer.Id);
                if (existing != null)
                {
                    existing.CustomerCode = CodeBox.Text.Trim();
                    existing.CustomerName = NameBox.Text.Trim();
                    existing.ContactPerson = ContactBox.Text.Trim();
                    existing.PhoneNumber = PhoneBox.Text.Trim();
                    existing.Email = EmailBox.Text.Trim();
                    existing.City = CityBox.Text.Trim();
                    existing.Address = AddressBox.Text.Trim();
                    existing.IsVip = VipCheck.IsChecked == true;
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
