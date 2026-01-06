using DeviceManage.Commands;
using DeviceManage.Models;
using DeviceManage.Services;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 用户管理视图模型
    /// </summary>
    public class UserViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserViewModel> _logger;

        /// <summary>
        /// 用户列表
        /// </summary>
        public ReactiveProperty<ObservableCollection<User>> Users { get; }

        /// <summary>
        /// 当前选中的用户
        /// </summary>
        public ReactiveProperty<User> SelectedUser { get; }

        /// <summary>
        /// 弹出框中正在编辑的用户（新增或编辑）
        /// </summary>
        public ReactiveProperty<User> EditingUser { get; }

        /// <summary>
        /// 是否处于编辑模式（true: 编辑现有记录；false: 新增）
        /// </summary>
        public ReactiveProperty<bool> IsEditing { get; }

        /// <summary>
        /// 是否显示弹出框
        /// </summary>
        public ReactiveProperty<bool> IsDialogOpen { get; }

        /// <summary>
        /// 搜索条件
        /// </summary>
        public ReactiveProperty<string> SearchUsername { get; }
        public ReactiveProperty<string> SearchRealName { get; }
        public ReactiveProperty<UserRole?> SearchRole { get; }
        public ReactiveProperty<bool?> SearchIsEnabled { get; }

        /// <summary>
        /// 所有可用角色列表（用于下拉框）
        /// </summary>
        public List<UserRole> AvailableRoles { get; }

        /// <summary>
        /// 所有可用角色描述列表（用于下拉框显示）
        /// </summary>
        public List<string> AvailableRoleDescriptions { get; }

        /// <summary>
        /// 搜索角色列表（包含"全部"选项）
        /// </summary>
        public List<UserRole?> SearchRoleList { get; }

        /// <summary>
        /// 分页信息
        /// </summary>
        public ReactiveProperty<int> CurrentPage { get; }
        public ReactiveProperty<int> PageSize { get; }
        public ReactiveProperty<int> TotalCount { get; }
        public ReactiveProperty<int> TotalPages { get; }
        
        /// <summary>
        /// 是否可以点击上一页
        /// </summary>
        public ReactiveProperty<bool> CanGoToPreviousPage { get; }
        
        /// <summary>
        /// 是否可以点击下一页
        /// </summary>
        public ReactiveProperty<bool> CanGoToNextPage { get; }


        public UserViewModel(IUserService userService, ILogger<UserViewModel> logger)
        {
            _userService = userService;
            _logger = logger;

            // 初始化ReactiveProperty
            Users = new ReactiveProperty<ObservableCollection<User>>(new ObservableCollection<User>());
            SelectedUser = new ReactiveProperty<User>(new User());
            EditingUser = new ReactiveProperty<User>(new User());
            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);

            // 搜索条件
            SearchUsername = new ReactiveProperty<string>(string.Empty);
            SearchRealName = new ReactiveProperty<string>(string.Empty);
            SearchRole = new ReactiveProperty<UserRole?>((UserRole?)null, ReactivePropertyMode.DistinctUntilChanged);
            SearchIsEnabled = new ReactiveProperty<bool?>(default(bool?));

            // 初始化角色列表
            AvailableRoles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>().ToList();
            AvailableRoleDescriptions = AvailableRoles.Select(r => GetRoleDescription(r)).ToList();
            
            // 搜索角色列表（包含null表示"全部"）
            SearchRoleList = new List<UserRole?> { null }.Concat(AvailableRoles.Cast<UserRole?>()).ToList();

            // 分页
            CurrentPage = new ReactiveProperty<int>(1);
            PageSize = new ReactiveProperty<int>(20);
            TotalCount = new ReactiveProperty<int>(0);
            TotalPages = new ReactiveProperty<int>(0);
            CanGoToPreviousPage = new ReactiveProperty<bool>(false);
            CanGoToNextPage = new ReactiveProperty<bool>(false);
            
            // 监听分页变化，更新按钮状态
            CurrentPage.Subscribe(_ => UpdatePaginationButtons());
            TotalPages.Subscribe(_ => UpdatePaginationButtons());

            // 初始化命令
            LoadUsersCommand = new ReactiveCommand().WithSubscribe(async () => await LoadUsersAsync());
            AddUserCommand = new ReactiveCommand();
            AddUserCommand.Subscribe(_ => OpenAddDialog());
            UpdateUserCommand = new ReactiveCommand().WithSubscribe(async () => await SaveUserAsync());
            EditCommand = new ReactiveCommand<User>().WithSubscribe(user => OpenEditDialog(user));
            DeleteUserCommand = new ReactiveCommand<User>().WithSubscribe(async user => await DeleteUserAsync(user));
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(_ => CloseDialog());
            SearchCommand = new ReactiveCommand().WithSubscribe(async () => await LoadUsersAsync());
            ResetSearchCommand = new ReactiveCommand().WithSubscribe(() => ResetSearch());
            PreviousPageCommand = new ReactiveCommand().WithSubscribe(async () => await GoToPreviousPageAsync());
            NextPageCommand = new ReactiveCommand().WithSubscribe(async () => await GoToNextPageAsync());
            GoToPageCommand = new ReactiveCommand<int>().WithSubscribe(async page => await GoToPageAsync(page));

            Task.Run(async () => await LoadUsersAsync());
        }

        #region Commands
        public ReactiveCommand LoadUsersCommand { get; }
        public ReactiveCommand AddUserCommand { get; }
        public ReactiveCommand UpdateUserCommand { get; }
        public ReactiveCommand<User> DeleteUserCommand { get; }
        public ReactiveCommand<User> EditCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand SearchCommand { get; }
        public ReactiveCommand ResetSearchCommand { get; }
        public ReactiveCommand PreviousPageCommand { get; }
        public ReactiveCommand NextPageCommand { get; }
        public ReactiveCommand<int> GoToPageCommand { get; }
        #endregion

        /// <summary>
        /// 加载用户列表（带分页和搜索）
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                var searchDto = new UserSearchDto
                {
                    current = CurrentPage.Value,
                    pageSize = PageSize.Value,
                    Username = SearchUsername.Value ?? string.Empty,
                    RealName = SearchRealName.Value ?? string.Empty,
                    Role = SearchRole.Value.HasValue ? GetRoleDescription(SearchRole.Value.Value) : string.Empty,
                    IsEnabled = SearchIsEnabled.Value
                };

                var paginatedList = await _userService.GetAllUsersAsync(searchDto);
                
                Users.Value = new ObservableCollection<User>(paginatedList);
                TotalCount.Value = paginatedList.TotalCounts;
                TotalPages.Value = paginatedList.TotalPages;
                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载用户列表失败: {ex.Message}");
                MessageBox.Show($"加载用户列表失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 保存用户（新增或更新）
        /// </summary>
        private async Task SaveUserAsync()
        {
            try
            {
                if (EditingUser.Value == null)
                {
                    _logger.LogWarning("保存用户失败：编辑用户对象为空。");
                    return;
                }

                var currentUser = EditingUser.Value;

                // 基础校验
                if (string.IsNullOrWhiteSpace(currentUser.Username))
                {
                    MessageBox.Show("用户名不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 角色验证：确保Role属性已正确设置
                // 如果用户在ComboBox中选择了角色，Role属性会被设置，setter会更新RoleString
                // 但如果RoleString仍然为空，可能是绑定问题，我们强制从Role属性同步一次
                if (string.IsNullOrWhiteSpace(currentUser.RoleString))
                {
                    // 尝试从Role属性获取值（会触发getter，如果RoleString为空会返回默认值OP）
                    var roleValue = currentUser.Role;
                    // 强制同步Role到RoleString（这会触发setter，更新RoleString）
                    currentUser.Role = roleValue;
                }
                
                // 最终验证：确保RoleString不为空
                if (string.IsNullOrWhiteSpace(currentUser.RoleString))
                {
                    MessageBox.Show("角色不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 新增时密码必填
                if (currentUser.Id == 0 && string.IsNullOrWhiteSpace(currentUser.Password))
                {
                    MessageBox.Show("新增用户时密码不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 验证用户名唯一性
                var trimmedUsername = currentUser.Username?.Trim() ?? string.Empty;
                var existingUser = await _userService.GetUserByUsernameAsync(trimmedUsername);
                if (existingUser != null && existingUser.Id != currentUser.Id)
                {
                    MessageBox.Show($"用户名 '{trimmedUsername}' 已存在，请使用其他用户名。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (currentUser.Id == 0)
                {
                    // 新增用户
                    var newUser = new User
                    {
                        Id = 0,
                        Username = currentUser.Username?.Trim() ?? string.Empty,
                        Password = currentUser.Password?.Trim() ?? string.Empty,
                        Role = currentUser.Role, // 使用枚举属性，会自动转换为RoleString
                        RealName = currentUser.RealName?.Trim(),
                        Email = string.IsNullOrWhiteSpace(currentUser.Email) ? null : currentUser.Email?.Trim(),
                        Phone = string.IsNullOrWhiteSpace(currentUser.Phone) ? null : currentUser.Phone?.Trim(),
                        IsEnabled = currentUser.IsEnabled,
                        Remarks = string.IsNullOrWhiteSpace(currentUser.Remarks) ? null : currentUser.Remarks?.Trim(),
                        IsDeleted = false,
                        DeletedAt = null
                    };

                    var addedUser = await _userService.AddUserAsync(newUser);
                    _logger.LogInformation($"成功新增用户 - ID: {addedUser.Id}, 用户名: {addedUser.Username}");
                    MessageBox.Show($"用户 \"{addedUser.Username}\" 已成功新增！", "新增成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 新增后需要重新加载，因为新数据需要添加到列表中
                    await LoadUsersAsync();
                    SelectedUser.Value = new User();
                }
                else
                {
                    // 更新用户
                    var userToUpdate = new User
                    {
                        Id = currentUser.Id,
                        Username = currentUser.Username?.Trim() ?? string.Empty,
                        Role = currentUser.Role, // 使用枚举属性，会自动转换为RoleString
                        RealName = currentUser.RealName?.Trim(),
                        Email = string.IsNullOrWhiteSpace(currentUser.Email) ? null : currentUser.Email?.Trim(),
                        Phone = string.IsNullOrWhiteSpace(currentUser.Phone) ? null : currentUser.Phone?.Trim(),
                        IsEnabled = currentUser.IsEnabled,
                        LastLoginAt = currentUser.LastLoginAt,
                        Remarks = string.IsNullOrWhiteSpace(currentUser.Remarks) ? null : currentUser.Remarks?.Trim()
                    };

                    // 如果密码不为空，则更新密码
                    if (!string.IsNullOrWhiteSpace(currentUser.Password))
                    {
                        userToUpdate.Password = currentUser.Password.Trim();
                    }

                    var updatedUser = await _userService.UpdateUserAsync(userToUpdate);
                    _logger.LogInformation($"成功更新用户 - ID: {updatedUser.Id}, 用户名: {updatedUser.Username}");
                    MessageBox.Show($"用户 \"{updatedUser.Username}\" 已成功更新！", "更新成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 编辑后，只更新列表中的对应项，保持数据在原位置
                    var userInList = Users.Value.FirstOrDefault(u => u.Id == updatedUser.Id);
                    if (userInList != null)
                    {
                        // 保存原位置
                        var index = Users.Value.IndexOf(userInList);
                        
                        // 创建新对象，复制所有更新后的属性
                        var updatedUserInList = new User
                        {
                            Id = updatedUser.Id,
                            Username = updatedUser.Username,
                            RoleString = updatedUser.RoleString,
                            RealName = updatedUser.RealName,
                            Email = updatedUser.Email,
                            Phone = updatedUser.Phone,
                            IsEnabled = updatedUser.IsEnabled,
                            Remarks = updatedUser.Remarks,
                            LastLoginAt = updatedUser.LastLoginAt,
                            CreatedAt = updatedUser.CreatedAt,
                            UpdatedAt = updatedUser.UpdatedAt,
                            IsDeleted = updatedUser.IsDeleted,
                            DeletedAt = updatedUser.DeletedAt
                        };
                        
                        // 替换列表中的对象，触发 ObservableCollection 的更新通知
                        Users.Value[index] = updatedUserInList;
                        
                        // 保持选中状态
                        SelectedUser.Value = updatedUserInList;
                    }
                    else
                    {
                        // 如果找不到（可能在其他页），则重新加载
                        await LoadUsersAsync();
                        SelectedUser.Value = new User();
                    }
                }

                CloseDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存用户失败: {ex.Message}");
                MessageBox.Show($"保存用户失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        private async Task DeleteUserAsync(User user)
        {
            if (user == null)
            {
                _logger.LogWarning("删除用户失败：用户对象为空。");
                return;
            }

            if (user.Id <= 0)
            {
                _logger.LogWarning($"删除用户失败：用户ID无效 (ID: {user.Id})。");
                return;
            }

            var userName = string.IsNullOrWhiteSpace(user.Username) ? $"ID: {user.Id}" : user.Username;
            
            MessageBoxResult result;
            try
            {
                result = MessageBox.Show(
                    $"确定要删除用户 \"{userName}\" 吗？\n\n删除后无法恢复！",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"显示删除确认对话框时发生异常 (ID: {user.Id})");
                return;
            }

            // 处理所有非确认的情况：No、None（关闭按钮）、Cancel等
            // HandyControl MessageBox 关闭按钮可能返回 None 或 No
            if (result == MessageBoxResult.Yes)
            {
                // 用户确认删除，继续执行删除操作
            }
            else
            {
                // 用户取消删除（点击"否"按钮、关闭按钮或其他情况）
                _logger.LogInformation($"用户取消删除用户 (ID: {user.Id}, 用户名: {userName}, 结果: {result})");
                return;
            }

            try
            {
                await _userService.DeleteUserAsync(user.Id);
                _logger.LogInformation($"成功删除用户 (ID: {user.Id}, 用户名: {userName})");

                await LoadUsersAsync();
                if (SelectedUser.Value != null && SelectedUser.Value.Id == user.Id)
                {
                    SelectedUser.Value = new User();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除用户失败 (ID: {user.Id}): {ex.Message}");
                MessageBox.Show($"删除用户失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开"新增"弹出框
        /// </summary>
        private void OpenAddDialog()
        {
            EditingUser.Value = new User
            {
                IsEnabled = true,
                IsDeleted = false,
                Password = string.Empty,
                Role = UserRole.OP // 设置默认角色为操作员
            };
            IsEditing.Value = false;
            IsDialogOpen.Value = true;
        }

        /// <summary>
        /// 打开"编辑"弹出框
        /// </summary>
        private async void OpenEditDialog(User user)
        {
            if (user == null)
            {
                _logger.LogWarning("打开编辑对话框失败：用户对象为空。");
                return;
            }

            if (user.Id <= 0)
            {
                _logger.LogWarning($"打开编辑对话框失败：用户ID无效 (ID: {user.Id})。");
                return;
            }

            try
            {
                // 从服务器获取最新数据
                var userDetail = await _userService.GetUserByIdAsync(user.Id);
                if (userDetail == null)
                {
                    MessageBox.Show("用户不存在或已被删除", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    await LoadUsersAsync();
                    return;
                }

                // 创建副本，避免直接修改列表中的对象
                EditingUser.Value = new User
                {
                    Id = userDetail.Id,
                    Username = userDetail.Username ?? string.Empty,
                    Password = string.Empty, // 编辑时不显示密码
                    Role = userDetail.Role, // Role是枚举类型，直接赋值
                    RealName = userDetail.RealName,
                    Email = userDetail.Email,
                    Phone = userDetail.Phone,
                    IsEnabled = userDetail.IsEnabled,
                    LastLoginAt = userDetail.LastLoginAt,
                    Remarks = userDetail.Remarks,
                    IsDeleted = userDetail.IsDeleted,
                    DeletedAt = userDetail.DeletedAt
                };

                IsEditing.Value = true;
                IsDialogOpen.Value = true;
                _logger.LogInformation($"打开编辑对话框 (ID: {userDetail.Id}, 用户名: {userDetail.Username})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"打开编辑对话框失败 (ID: {user?.Id}): {ex.Message}");
                MessageBox.Show($"加载用户详情失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 关闭弹出框并丢弃未保存更改
        /// </summary>
        private void CloseDialog()
        {
            IsDialogOpen.Value = false;
            EditingUser.Value = new User();
        }

        /// <summary>
        /// 重置搜索条件
        /// </summary>
        private void ResetSearch()
        {
            SearchUsername.Value = string.Empty;
            SearchRealName.Value = string.Empty;
            SearchRole.Value = null;
            SearchIsEnabled.Value = null;
            CurrentPage.Value = 1;
            Task.Run(async () => await LoadUsersAsync());
        }

        /// <summary>
        /// 上一页
        /// </summary>
        private async Task GoToPreviousPageAsync()
        {
            if (CurrentPage.Value > 1)
            {
                CurrentPage.Value--;
                await LoadUsersAsync();
            }
        }

        /// <summary>
        /// 下一页
        /// </summary>
        private async Task GoToNextPageAsync()
        {
            if (CurrentPage.Value < TotalPages.Value)
            {
                CurrentPage.Value++;
                await LoadUsersAsync();
            }
        }

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages.Value)
            {
                CurrentPage.Value = page;
                await LoadUsersAsync();
            }
        }

        /// <summary>
        /// 更新分页按钮状态
        /// </summary>
        private void UpdatePaginationButtons()
        {
            CanGoToPreviousPage.Value = CurrentPage.Value > 1;
            CanGoToNextPage.Value = CurrentPage.Value < TotalPages.Value && TotalPages.Value > 0;
        }

        /// <summary>
        /// 获取角色描述
        /// </summary>
        private string GetRoleDescription(UserRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            if (field != null)
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                return attribute?.Description ?? role.ToString();
            }
            return role.ToString();
        }
    }
}

