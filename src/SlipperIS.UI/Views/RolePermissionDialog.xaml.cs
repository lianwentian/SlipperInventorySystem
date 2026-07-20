using System.Windows;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class PermissionCheckItem
{
    public int PermissionId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}

public partial class RolePermissionDialog : Window
{
    private readonly Role _role;
    private List<PermissionCheckItem> _items = new();

    public RolePermissionDialog(Role role)
    {
        InitializeComponent();
        _role = role;
        RoleNameText.Text = $"角色：{role.Name} - 权限分配";
        LoadPermissions();
    }

    private void LoadPermissions()
    {
        using var db = AppDbContextFactory.Create();
        var allPermissions = db.Permissions.Where(p => p.IsActive).OrderBy(p => p.Module).ThenBy(p => p.Action).ToList();
        var assignedIds = db.RolePermissions.Where(rp => rp.RoleId == _role.Id).Select(rp => rp.PermissionId).ToHashSet();

        _items = allPermissions.Select(p => new PermissionCheckItem
        {
            PermissionId = p.Id,
            DisplayName = $"{p.Module} - {p.Action}",
            IsAssigned = assignedIds.Contains(p.Id)
        }).ToList();

        PermissionList.ItemsSource = _items;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            var existing = db.RolePermissions.Where(rp => rp.RoleId == _role.Id).ToList();
            db.RolePermissions.RemoveRange(existing);
            db.SaveChanges();

            var maxId = db.RolePermissions.Any() ? db.RolePermissions.Max(rp => rp.Id) : 0;
            int nextId = maxId + 1;

            foreach (var item in _items.Where(x => x.IsAssigned))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    Id = nextId++,
                    RoleId = _role.Id,
                    PermissionId = item.PermissionId
                });
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
