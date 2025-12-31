using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MessageBox = HandyControl.Controls.MessageBox;
using DeviceManage.Services.DeviceMagService.Dto;
using DeviceManage.CustomerControl;
using Prism.Commands;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 点位管理（Tag）页面：实现 Tag（点位组）增删改查，支持分页（每页20条）
    /// </summary>
    public class TagViewModel : ViewModelBase
    {
        private readonly ITagService _tagSvc;
        private readonly IPlcDeviceService _plcDeviceSvc;
        private readonly IRecipeService _recipeSvc;
        private readonly ILogger<TagViewModel> _logger;

        // 数据列表（分页后的数据）
        public ReactiveProperty<ObservableCollection<Tag>> Tags { get; }
        public ReactiveProperty<Tag> SelectedTag { get; }
        public ReactiveProperty<Tag> EditingTag { get; }

        // 下拉框数据源
        public ReactiveProperty<ObservableCollection<PlcDevice>> PlcDevices { get; }
        public ReactiveProperty<ObservableCollection<Recipe>> Recipes { get; }

        // 搜索条件
        public ReactiveProperty<string> SearchTagName { get; }
        public ReactiveProperty<string> SearchPLCName { get; }
        public ReactiveProperty<string> SearchRecipeName { get; }

        // 分页相关
        public PageListViewModel PageListViewModel { get; }

        // 编辑对话框
        public ReactiveProperty<bool> IsEditing { get; }
        public ReactiveProperty<bool> IsDialogOpen { get; }
        
        // 表格编辑区（TagDetail 行数据）
        public ReactiveProperty<ObservableCollection<TagDetailRow>> TagDetailRows { get; }
        
        // DataType 下拉枚举数据源
        public ReactiveProperty<ObservableCollection<DataType>> DataTypes { get; }

        public TagViewModel(ITagService tagSvc, IPlcDeviceService plcDeviceSvc, IRecipeService recipeSvc, ILogger<TagViewModel> logger)
        {
            _tagSvc = tagSvc;
            _plcDeviceSvc = plcDeviceSvc;
            _recipeSvc = recipeSvc;
            _logger = logger;

            // 初始化 ReactiveProperty
            Tags = new ReactiveProperty<ObservableCollection<Tag>>(new ObservableCollection<Tag>());
            SelectedTag = new ReactiveProperty<Tag>(new Tag());
            EditingTag = new ReactiveProperty<Tag>(new Tag());
            PlcDevices = new ReactiveProperty<ObservableCollection<PlcDevice>>(new ObservableCollection<PlcDevice>());
            Recipes = new ReactiveProperty<ObservableCollection<Recipe>>(new ObservableCollection<Recipe>());
            
            SearchTagName = new ReactiveProperty<string>(string.Empty);
            SearchPLCName = new ReactiveProperty<string>(string.Empty);
            SearchRecipeName = new ReactiveProperty<string>(string.Empty);
            
            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);
            TagDetailRows = new ReactiveProperty<ObservableCollection<TagDetailRow>>(new ObservableCollection<TagDetailRow>());
            
            // 初始化 DataType 枚举列表
            var dataTypes = new ObservableCollection<DataType>(Enum.GetValues(typeof(DataType)).Cast<DataType>());
            DataTypes = new ReactiveProperty<ObservableCollection<DataType>>(dataTypes);

            // 初始化分页组件
            PageListViewModel = new PageListViewModel
            {
                PageSize = 20,
                PageIndex = 1,
                Total = 0,
                TotalPage = 0
            };
            PageListViewModel.PageChangedCommand = new DelegateCommand(async () => await LoadAsync());
            PageListViewModel.PageSizeChangedCommand = new DelegateCommand(async () => await LoadAsync());

            // 初始化命令
            LoadCommand = new ReactiveCommand().WithSubscribe(async () => await LoadAsync());
            SearchCommand = new ReactiveCommand().WithSubscribe(async () =>
            {
                PageListViewModel.PageIndex = 1;
                await LoadAsync();
            });
            AddCommand = new ReactiveCommand().WithSubscribe(OpenAdd);
            EditCommand = new ReactiveCommand<Tag>().WithSubscribe(OpenEdit);
            DeleteCommand = new ReactiveCommand<Tag>().WithSubscribe(async t => await DeleteAsync(t));
            AddDetailCommand = new ReactiveCommand().WithSubscribe(AddDetailRow);
            RemoveDetailCommand = new ReactiveCommand<TagDetailRow>().WithSubscribe(RemoveDetailRow);
            SaveCommand = new ReactiveCommand().WithSubscribe(async () => await SaveAsync());
            CancelCommand = new ReactiveCommand().WithSubscribe(Close);

            // 初始化加载（在后台线程加载数据，然后在 UI 线程更新）
            _ = Task.Run(async () =>
            {
                await LoadDropdownDataAsync();
                await LoadAsync();
            });
        }

        public ReactiveCommand LoadCommand { get; }
        public ReactiveCommand SearchCommand { get; }
        public ReactiveCommand AddCommand { get; }
        public ReactiveCommand<Tag> EditCommand { get; }
        public ReactiveCommand<Tag> DeleteCommand { get; }
        public ReactiveCommand AddDetailCommand { get; }
        public ReactiveCommand<TagDetailRow> RemoveDetailCommand { get; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand CancelCommand { get; }

        /// <summary>
        /// 加载下拉框数据（PLC设备和配方）
        /// </summary>
        private async Task LoadDropdownDataAsync()
        {
            try
            {
                var plcDevices = await _plcDeviceSvc.GetAllPlcDevicesAsync();
                var recipes = await _recipeSvc.GetAllRecipesAsync();

                // 在 UI 线程上更新 ObservableCollection
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    PlcDevices.Value = new ObservableCollection<PlcDevice>(plcDevices);
                    Recipes.Value = new ObservableCollection<Recipe>(recipes);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载下拉框数据失败");
            }
        }

        /// <summary>
        /// 加载分页数据
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                var searchDto = new TagSearchDto
                {
                    current = PageListViewModel.PageIndex,
                    pageSize = PageListViewModel.PageSize,
                    PlcTagName = SearchTagName.Value?.Trim() ?? string.Empty,
                    PLCName = SearchPLCName.Value?.Trim() ?? string.Empty,
                    RecipeName = SearchRecipeName.Value?.Trim() ?? string.Empty
                };

                var paginatedList = await _tagSvc.GetAllTagsAsync(searchDto);
                
                // 在 UI 线程上更新 ObservableCollection 和分页信息
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Tags.Value = new ObservableCollection<Tag>(paginatedList);
                    
                    // 更新分页信息
                    PageListViewModel.Total = paginatedList.TotalCounts;
                    PageListViewModel.TotalPage = paginatedList.TotalPages;
                    PageListViewModel.Init();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载点位失败");
                
                // 错误提示也需要在 UI 线程上显示
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"加载数据失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        /// <summary>
        /// 打开新增对话框
        /// </summary>
        private async void OpenAdd()
        {
            // 确保下拉框数据已加载
            if (PlcDevices.Value.Count == 0 || Recipes.Value.Count == 0)
            {
                await LoadDropdownDataAsync();
            }

            EditingTag.Value = new Tag
            {
                PlcTagName = string.Empty,
                PlcDeviceId = PlcDevices.Value.Count > 0 ? PlcDevices.Value[0].Id : 0,
                RecipeId = Recipes.Value.Count > 0 ? Recipes.Value[0].RecipeId : 0,
                Remarks = string.Empty,
                TagDetailDataArray = new List<TagDetail>()
            };
            
            TagDetailRows.Value = new ObservableCollection<TagDetailRow>();
            IsEditing.Value = false;
            IsDialogOpen.Value = true;
        }

        /// <summary>
        /// 打开编辑对话框
        /// </summary>
        private async void OpenEdit(Tag tag)
        {
            if (tag == null) return;

            // 确保下拉框数据已加载
            if (PlcDevices.Value.Count == 0 || Recipes.Value.Count == 0)
            {
                await LoadDropdownDataAsync();
            }

            try
            {
                // 重新加载完整数据（包含导航属性）
                var fullTag = await _tagSvc.GetTagByIdAsync(tag.Id);
                if (fullTag == null)
                {
                    MessageBox.Show("未找到该点位数据", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                EditingTag.Value = new Tag
                {
                    Id = fullTag.Id,
                    PlcTagName = fullTag.PlcTagName ?? string.Empty,
                    PlcDeviceId = fullTag.PlcDeviceId,
                    RecipeId = fullTag.RecipeId,
                    Remarks = fullTag.Remarks ?? string.Empty,
                    TagDetailDataArray = fullTag.TagDetailDataArray ?? new List<TagDetail>()
                };

                // 将 TagDetailDataArray 转换为 TagDetailRows
                TagDetailRows.Value = new ObservableCollection<TagDetailRow>(
                    (EditingTag.Value.TagDetailDataArray ?? new List<TagDetail>())
                        .Select(d => new TagDetailRow(d))
                );

                IsEditing.Value = true;
                IsDialogOpen.Value = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打开编辑对话框失败");
                MessageBox.Show($"打开编辑对话框失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 保存数据（新增或更新）
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                var entity = EditingTag.Value;

                // 验证必填字段
                if (string.IsNullOrWhiteSpace(entity.PlcTagName))
                {
                    MessageBox.Show("请输入点位名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (entity.PlcDeviceId <= 0)
                {
                    MessageBox.Show("请选择 PLC 设备", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 验证 PlcDeviceId 是否存在
                var plcDevice = await _plcDeviceSvc.GetPlcDeviceByIdAsync(entity.PlcDeviceId);
                if (plcDevice == null)
                {
                    MessageBox.Show($"PLC 设备 ID={entity.PlcDeviceId} 不存在，请先创建该 PLC 设备", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 验证 RecipeId 是否存在
                if (entity.RecipeId > 0)
                {
                    var recipe = await _recipeSvc.GetRecipeByIdAsync(entity.RecipeId);
                    if (recipe == null)
                    {
                        MessageBox.Show($"配方 ID={entity.RecipeId} 不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // 从 TagDetailRows 转换为 TagDetailDataArray
                var rows = TagDetailRows.Value ?? new ObservableCollection<TagDetailRow>();
                var validRows = rows.Where(r => 
                    r != null && 
                    !string.IsNullOrWhiteSpace(r.TagName.Value) && 
                    !string.IsNullOrWhiteSpace(r.Address.Value)
                ).ToList();

                if (validRows.Count == 0)
                {
                    MessageBox.Show("请至少新增一条有效的点位明细（TagName 和 Address 不能为空）", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查 Address 是否重复
                var duplicateAddresses = validRows
                    .GroupBy(r => r.Address.Value?.Trim())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                
                if (duplicateAddresses.Any())
                {
                    MessageBox.Show($"Address 不能重复：{string.Join(", ", duplicateAddresses)}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 转换为实体
                entity.TagDetailDataArray = validRows.Select(r => r.ToEntity()).ToList();

                // 保存数据
                if (entity.Id == 0)
                {
                    await _tagSvc.AddTagAsync(entity);
                    MessageBox.Show("新增成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _tagSvc.UpdateTagAsync(entity);
                    MessageBox.Show("更新成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadAsync();
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存点位失败");
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n\n内部异常：{ex.InnerException.Message}";
                }
                MessageBox.Show($"保存点位失败：{errorMsg}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        private async Task DeleteAsync(Tag tag)
        {
            if (tag == null) return;
            
            var result = MessageBox.Show(
                $"确定删除点位组 ID={tag.Id} (名称: {tag.PlcTagName}) 吗？",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _tagSvc.DeleteTagAsync(tag.Id);
                MessageBox.Show("删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除点位失败");
                MessageBox.Show($"删除点位失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 添加点位明细行
        /// </summary>
        private void AddDetailRow()
        {
            TagDetailRows.Value.Add(new TagDetailRow());
        }

        /// <summary>
        /// 删除点位明细行
        /// </summary>
        private void RemoveDetailRow(TagDetailRow row)
        {
            if (row == null) return;
            TagDetailRows.Value.Remove(row);
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        private void Close()
        {
            IsDialogOpen.Value = false;
            EditingTag.Value = new Tag();
            TagDetailRows.Value = new ObservableCollection<TagDetailRow>();
        }
    }
}
