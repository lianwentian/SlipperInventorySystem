using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.UI.Views;

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public partial class UserWindow : Window
{
    private List<UserViewModel> _allUsers = new();

    public UserWindow()
    {
        InitializeComponent();
        LoadUsers();
    }

    private void LoadUsers()
    {
        try
        {
            using var db = AppDbContextFactory.Create();
            _allUsers = db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Username)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : string.Empty,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt
                }).ToList();

            UserGrid.ItemsSource = _allUsers;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载用户失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        UserGrid.ItemsSource = string.IsNullOrEmpty(keyword)
            ? _allUsers
            : _allUsers.Where(u =>
                u.Username.ToLower().Contains(keyword) ||
                u.FullName.ToLower().Contains(keyword) ||
                u.Email.ToLower().Contains(keyword)).ToList();
    }

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new UserEditDialog(null);
        if (dlg.ShowDialog() == true) LoadUsers();
    }

    private void EditUser_Click(object sender, RoutedEventArgs e)
    {
        if (UserGrid.SelectedItem is not UserViewModel selected)
        {
            MessageBox.Show("请先选择一个用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        using var db = AppDbContextFactory.Create();
        var user = db.Users.Find(selected.Id);
        if (user != null)
        {
            var dlg = new UserEditDialog(user);
            if (dlg.ShowDialog() == true) LoadUsers();
        }
    }

    private void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (UserGrid.SelectedItem is not UserViewModel selected)
        {
            MessageBox.Show("请先选择一个用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (selected.Username == "admin")
        {
            MessageBox.Show("不能删除系统管理员账户。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show($"确定要删除用户「{selected.Username}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = AppDbContextFactory.Create();
            var user = db.Users.Find(selected.Id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
        LoadUsers();
    }
}
