using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 角色权限管理窗口 - 管理角色和分配权限
/// </summary>
public partial class RoleWindow : Window
{
    private SlipperDbContext _db;
    private List<Role> _roles = new();
    private List<PermissionItem> _permissions = new();
    private int _editingRoleId = 0;
    private int _selectedRoleId = 0;

    public RoleWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadRoles();
    }

    private void LoadRoles()
    {
        _roles = _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToList();
        RoleList.ItemsSource = _roles;
    }

    private void RoleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoleList.SelectedItem is not Role role) return;

        _selectedRoleId = role.Id;
        PermTitle.Text = $"权限分配 - {role.Name}";

        var allPermissions = _db.Permissions.AsNoTracking().OrderBy(p => p.Module).ThenBy(p => p.Action).ToList();
        var grantedIds = _db.RolePermissions.AsNoTracking()
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        _permissions = allPermissions.Select(p => new PermissionItem
        {
            Id = p.Id,
            Name = p.Name,
            Module = p.Module,
            Action = p.Action,
            IsGranted = grantedIds.Contains(p.Id)
        }).ToList();

        PermGrid.ItemsSource = _permissions;
        PermStatusText.Text = $"共 {_permissions.Count} 个权限，已授权 {_permissions.Count(x => x.IsGranted)} 个";
    }

    private void AddRole_Click(object sender, RoutedEventArgs e)
    {
        _editingRoleId = 0;
        RoleEditTitle.Text = "添加角色";
        TxtRoleName.Text = string.Empty;
        TxtRoleDesc.Text = string.Empty;
        RoleEditPanel.Visibility = Visibility.Visible;
    }

    private void EditRole_Click(object sender, RoutedEventArgs e)
    {
        if (RoleList.SelectedItem is not Role role)
        {
            MessageBox.Show("请先选择一个角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _editingRoleId = role.Id;
        RoleEditTitle.Text = "编辑角色";
        TxtRoleName.Text = role.Name;
        TxtRoleDesc.Text = role.Description;
        RoleEditPanel.Visibility = Visibility.Visible;
    }

    private void DeleteRole_Click(object sender, RoutedEventArgs e)
    {
        if (RoleList.SelectedItem is not Role role)
        {
            MessageBox.Show("请先选择一个角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (role.Id == 1)
        {
            MessageBox.Show("不能删除管理员角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"确定要删除角色「{role.Name}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var dbRole = _db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.Id == role.Id);
            if (dbRole != null)
            {
                _db.Roles.Remove(dbRole);
                _db.SaveChanges();
                LoadRoles();
                PermGrid.ItemsSource = null;
                PermTitle.Text = "权限分配（选择角色后显示）";
                PermStatusText.Text = string.Empty;
            }
        }
    }

    private void SaveRole_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtRoleName.Text))
        {
            MessageBox.Show("角色名称为必填项。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingRoleId == 0)
        {
            var role = new Role
            {
                Name = TxtRoleName.Text.Trim(),
                Description = TxtRoleDesc.Text.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _db.Roles.Add(role);
            _db.SaveChanges();
        }
        else
        {
            var role = _db.Roles.Find(_editingRoleId);
            if (role != null)
            {
                role.Name = TxtRoleName.Text.Trim();
                role.Description = TxtRoleDesc.Text.Trim();
                _db.SaveChanges();
            }
        }

        RoleEditPanel.Visibility = Visibility.Collapsed;
        LoadRoles();
    }

    private void CancelRoleEdit_Click(object sender, RoutedEventArgs e)
    {
        RoleEditPanel.Visibility = Visibility.Collapsed;
    }

    private void SavePermissions_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRoleId == 0)
        {
            MessageBox.Show("请先选择一个角色。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // 删除当前角色的所有权限再重新添加
        var existingPermissions = _db.RolePermissions
            .Where(rp => rp.RoleId == _selectedRoleId)
            .ToList();
        _db.RolePermissions.RemoveRange(existingPermissions);

        var grantedPermissions = _permissions.Where(p => p.IsGranted).ToList();
        int nextId = (_db.RolePermissions.Any() ? _db.RolePermissions.Max(rp => rp.Id) : 0) + 1;

        foreach (var perm in grantedPermissions)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                Id = nextId++,
                RoleId = _selectedRoleId,
                PermissionId = perm.Id
            });
        }

        _db.SaveChanges();
        PermStatusText.Text = $"权限已保存，已授权 {grantedPermissions.Count} 个权限";
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}

/// <summary>
/// 权限项视图模型（支持双向绑定）
/// </summary>
public class PermissionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}
