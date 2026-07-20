using System.Windows;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class RoleViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
}

public partial class RoleWindow : Window
{
    private List<RoleViewModel> _roles = new();

    public RoleWindow()
    {
        InitializeComponent();
        LoadRoles();
    }

    private void LoadRoles()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _roles = db.Roles
                .Include(r => r.Users)
                .OrderBy(r => r.Name)
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt,
                    UserCount = r.Users.Count
                }).ToList();

            RoleGrid.ItemsSource = _roles;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载角色失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddRole_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new RoleEditDialog(null);
        if (dlg.ShowDialog() == true) LoadRoles();
    }

    private void EditRole_Click(object sender, RoutedEventArgs e)
    {
        if (RoleGrid.SelectedItem is not RoleViewModel selected)
        {
            MessageBox.Show("请先选择一个角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        using var db = AppDbContextFactory.Create();
        var role = db.Roles.Find(selected.Id);
        if (role != null)
        {
            var dlg = new RoleEditDialog(role);
            if (dlg.ShowDialog() == true) LoadRoles();
        }
    }

    private void DeleteRole_Click(object sender, RoutedEventArgs e)
    {
        if (RoleGrid.SelectedItem is not RoleViewModel selected)
        {
            MessageBox.Show("请先选择一个角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (selected.Name == "Admin")
        {
            MessageBox.Show("不能删除系统管理员角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (selected.UserCount > 0)
        {
            MessageBox.Show($"该角色还有 {selected.UserCount} 个用户，无法删除。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show($"确定要删除角色「{selected.Name}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var role = db.Roles.Find(selected.Id);
            if (role != null)
            {
                db.Roles.Remove(role);
                db.SaveChanges();
            }
            LoadRoles();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AssignPermissions_Click(object sender, RoutedEventArgs e)
    {
        if (RoleGrid.SelectedItem is not RoleViewModel selected)
        {
            MessageBox.Show("请先选择一个角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        using var db = AppDbContextFactory.Create();
        var role = db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.Id == selected.Id);
        if (role != null)
        {
            var dlg = new RolePermissionDialog(role);
            if (dlg.ShowDialog() == true) LoadRoles();
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadRoles();
    }
}
