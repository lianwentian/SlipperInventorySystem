using System.Windows;
using SlipperIS.Core.Models;
using SlipperIS.Core.Services;

namespace SlipperIS.UI.Views;

public partial class UserEditDialog : Window
{
    private readonly User? _user;

    public UserEditDialog(User? user)
    {
        InitializeComponent();
        _user = user;

        using var db = AppDbContextFactory.Create();
        var roles = db.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToList();
        RoleCombo.ItemsSource = roles;

        if (user != null)
        {
            Title = "编辑用户";
            UsernameBox.Text = user.Username;
            FullNameBox.Text = user.FullName;
            EmailBox.Text = user.Email;
            PhoneBox.Text = user.PhoneNumber;
            RoleCombo.SelectedValue = user.RoleId;
            ActiveCheck.IsChecked = user.IsActive;

            if (user.Username == "admin")
                UsernameBox.IsReadOnly = true;
        }
        else
        {
            Title = "创建用户";
            if (roles.Count > 0) RoleCombo.SelectedIndex = 0;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameBox.Text))
        {
            MessageBox.Show("请输入用户名。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_user == null && string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            MessageBox.Show("请输入密码。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (RoleCombo.SelectedValue == null)
        {
            MessageBox.Show("请选择角色。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = AppDbContextFactory.Create();
            int roleId = (int)RoleCombo.SelectedValue;

            if (_user == null)
            {
                db.Users.Add(new User
                {
                    Username = UsernameBox.Text.Trim(),
                    PasswordHash = PasswordHasher.HashPassword(PasswordBox.Password),
                    FullName = FullNameBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    PhoneNumber = PhoneBox.Text.Trim(),
                    RoleId = roleId,
                    IsActive = ActiveCheck.IsChecked == true,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                var existing = db.Users.Find(_user.Id);
                if (existing != null)
                {
                    existing.Username = UsernameBox.Text.Trim();
                    existing.FullName = FullNameBox.Text.Trim();
                    existing.Email = EmailBox.Text.Trim();
                    existing.PhoneNumber = PhoneBox.Text.Trim();
                    existing.RoleId = roleId;
                    existing.IsActive = ActiveCheck.IsChecked == true;

                    if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                        existing.PasswordHash = PasswordHasher.HashPassword(PasswordBox.Password);
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
