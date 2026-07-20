using System.Windows;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public partial class RoleEditDialog : Window
{
    private readonly Role? _role;

    public RoleEditDialog(Role? role)
    {
        InitializeComponent();
        _role = role;
        if (role != null)
        {
            Title = "编辑角色";
            NameBox.Text = role.Name;
            DescBox.Text = role.Description;
            ActiveCheck.IsChecked = role.IsActive;

            if (role.Name == "Admin")
                NameBox.IsReadOnly = true;
        }
        else
        {
            Title = "创建角色";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("请输入角色名称。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = AppDbContextFactory.Create();
            if (_role == null)
            {
                db.Roles.Add(new Role
                {
                    Name = NameBox.Text.Trim(),
                    Description = DescBox.Text.Trim(),
                    IsActive = ActiveCheck.IsChecked == true,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                var existing = db.Roles.Find(_role.Id);
                if (existing != null)
                {
                    existing.Name = NameBox.Text.Trim();
                    existing.Description = DescBox.Text.Trim();
                    existing.IsActive = ActiveCheck.IsChecked == true;
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
