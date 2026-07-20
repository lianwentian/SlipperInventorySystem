using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;
using SlipperIS.Core.Services;
using SlipperIS.Data.DbContext;
using SlipperIS.UI.Helpers;

namespace SlipperIS.UI.Views;

/// <summary>
/// 用户管理窗口 - 支持用户的增删改查
/// </summary>
public partial class UserWindow : Window
{
    private SlipperDbContext _db;
    private List<UserViewModel> _allUsers = new();
    private int _editingId = 0;

    public UserWindow()
    {
        InitializeComponent();
        _db = AppDbContextFactory.CreateContext();
        LoadUsers();
    }

    private void LoadUsers()
    {
        var users = _db.Users.AsNoTracking().Include(u => u.Role)
            .OrderBy(u => u.Username).ToList();
        _allUsers = users.Select(u => new UserViewModel(u)).ToList();
        ApplySearch();
        StatusText.Text = $"共 {_allUsers.Count} 位用户";
    }

    private void ApplySearch()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(keyword))
            UserGrid.ItemsSource = _allUsers;
        else
            UserGrid.ItemsSource = _allUsers
                .Where(u => u.Username.ToLower().Contains(keyword)
                         || u.FullName.ToLower().Contains(keyword)
                         || u.Email.ToLower().Contains(keyword))
                .ToList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplySearch();

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _db.Dispose();
        _db = AppDbContextFactory.CreateContext();
        LoadUsers();
    }

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        _editingId = 0;
        EditTitle.Text = "添加用户";
        ClearForm();
        CmbRole.ItemsSource = _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToList();
        EditPanel.Visibility = Visibility.Visible;
    }

    private void EditUser_Click(object sender, RoutedEventArgs e)
    {
        if (UserGrid.SelectedItem is not UserViewModel selected)
        {
            MessageBox.Show("请先选择一位用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _editingId = selected.Id;
        EditTitle.Text = "编辑用户";
        var roles = _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToList();
        CmbRole.ItemsSource = roles;

        TxtUsername.Text = selected.Username;
        TxtPassword.Password = string.Empty;
        TxtFullName.Text = selected.FullName;
        TxtEmail.Text = selected.Email;
        TxtPhone.Text = selected.PhoneNumber;
        ChkActive.IsChecked = selected.IsActive;

        var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == selected.Id);
        if (user != null)
            CmbRole.SelectedItem = roles.FirstOrDefault(r => r.Id == user.RoleId);

        EditPanel.Visibility = Visibility.Visible;
    }

    private void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (UserGrid.SelectedItem is not UserViewModel selected)
        {
            MessageBox.Show("请先选择一位用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (selected.Id == 1)
        {
            MessageBox.Show("不能删除管理员账户。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"确定要删除用户「{selected.Username}」吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var user = _db.Users.Find(selected.Id);
            if (user != null)
            {
                // 检查该用户是否有关联记录，防止外键约束冲突
                bool hasOrders = _db.SalesOrders.Any(o => o.CreatedByUserId == selected.Id);
                bool hasQuotations = _db.Quotations.Any(q => q.CreatedByUserId == selected.Id);
                bool hasStockRecords = _db.StockRecords.Any(r => r.CreatedByUserId == selected.Id);

                if (hasOrders || hasQuotations || hasStockRecords)
                {
                    MessageBox.Show(
                        $"用户「{selected.Username}」存在关联的订单、报价单或库存记录，无法删除。\n请先转移或删除该用户的关联数据。",
                        "无法删除", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _db.Users.Remove(user);
                _db.SaveChanges();
                LoadUsers();
                StatusText.Text = "用户已删除";
            }
        }
    }

    private void SaveUser_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtUsername.Text))
        {
            MessageBox.Show("用户名为必填项。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CmbRole.SelectedItem is not Role role)
        {
            MessageBox.Show("请选择角色。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingId == 0)
        {
            if (string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                MessageBox.Show("新用户必须设置密码。", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = new User
            {
                Username = TxtUsername.Text.Trim(),
                PasswordHash = PasswordHasher.HashPassword(TxtPassword.Password),
                FullName = TxtFullName.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                PhoneNumber = TxtPhone.Text.Trim(),
                RoleId = role.Id,
                IsActive = ChkActive.IsChecked == true,
                CreatedAt = DateTime.Now
            };
            _db.Users.Add(user);
            _db.SaveChanges();
            StatusText.Text = "用户已添加";
        }
        else
        {
            var user = _db.Users.Find(_editingId);
            if (user != null)
            {
                user.Username = TxtUsername.Text.Trim();
                if (!string.IsNullOrWhiteSpace(TxtPassword.Password))
                    user.PasswordHash = PasswordHasher.HashPassword(TxtPassword.Password);
                user.FullName = TxtFullName.Text.Trim();
                user.Email = TxtEmail.Text.Trim();
                user.PhoneNumber = TxtPhone.Text.Trim();
                user.RoleId = role.Id;
                user.IsActive = ChkActive.IsChecked == true;
                _db.SaveChanges();
                StatusText.Text = "用户已更新";
            }
        }

        EditPanel.Visibility = Visibility.Collapsed;
        LoadUsers();
    }

    private void CancelEdit_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Collapsed;
    }

    private void ClearForm()
    {
        TxtUsername.Text = string.Empty;
        TxtPassword.Password = string.Empty;
        TxtFullName.Text = string.Empty;
        TxtEmail.Text = string.Empty;
        TxtPhone.Text = string.Empty;
        ChkActive.IsChecked = true;
        CmbRole.SelectedIndex = -1;
    }

    protected override void OnClosed(EventArgs e)
    {
        _db.Dispose();
        base.OnClosed(e);
    }
}

/// <summary>
/// 用户视图模型
/// </summary>
public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserViewModel(User u)
    {
        Id = u.Id;
        Username = u.Username;
        FullName = u.FullName;
        Email = u.Email;
        PhoneNumber = u.PhoneNumber;
        RoleName = u.Role?.Name ?? string.Empty;
        IsActive = u.IsActive;
        CreatedAt = u.CreatedAt;
    }
}
