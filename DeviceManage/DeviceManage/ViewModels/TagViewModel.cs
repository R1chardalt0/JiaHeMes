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
using MessageBox = HandyControl.Controls.MessageBox;
using DeviceManage.Services.DeviceMagService.Dto;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 点位管理（Tag）页面：实现 Tag（点位组）增删改查。
    /// TagDetailDataArray 通过表格编辑（替代 JSON 文本输入）。
    /// </summary>
    public class TagViewModel : ViewModelBase
    {
        private readonly ITagService _tagSvc;
        private readonly IPlcDeviceService _plcDeviceSvc;
        private readonly ILogger<TagViewModel> _logger;

        public ReactiveProperty<ObservableCollection<Tag>> Tags { get; }
        public ReactiveProperty<Tag> SelectedTag { get; }
        public ReactiveProperty<Tag> EditingTag { get; }

        // PLC 设备列表（用于下拉选择）
        public ReactiveProperty<ObservableCollection<PlcDevice>> PlcDevices { get; }

        // DataType 下拉枚举数据源
        public IReadOnlyList<DataType> DataTypes { get; } = Enum.GetValues(typeof(DataType)).Cast<DataType>().ToList();

        // 表格编辑区
        public ReactiveProperty<ObservableCollection<TagDetailRow>> TagDetailRows { get; }

        public ReactiveProperty<bool> IsEditing { get; }
        public ReactiveProperty<bool> IsDialogOpen { get; }

        public TagViewModel(ITagService tagSvc, IPlcDeviceService plcDeviceSvc, ILogger<TagViewModel> logger)
        {
            _tagSvc = tagSvc;
            _plcDeviceSvc = plcDeviceSvc;
            _logger = logger;

            Tags = new ReactiveProperty<ObservableCollection<Tag>>(new ObservableCollection<Tag>());
            SelectedTag = new ReactiveProperty<Tag>(new Tag());
            EditingTag = new ReactiveProperty<Tag>(new Tag { PlcDeviceId = 1 });
            PlcDevices = new ReactiveProperty<ObservableCollection<PlcDevice>>(new ObservableCollection<PlcDevice>());
            TagDetailRows = new ReactiveProperty<ObservableCollection<TagDetailRow>>(new ObservableCollection<TagDetailRow>());

            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);

            LoadCommand = new ReactiveCommand().WithSubscribe(async () => await LoadAsync());
            AddCommand = new ReactiveCommand().WithSubscribe(OpenAdd);
            EditCommand = new ReactiveCommand<Tag>().WithSubscribe(OpenEdit);
            DeleteCommand = new ReactiveCommand<Tag>().WithSubscribe(async t => await DeleteAsync(t));
            AddDetailCommand = new ReactiveCommand().WithSubscribe(AddDetailRow);
            RemoveDetailCommand = new ReactiveCommand<TagDetailRow>().WithSubscribe(RemoveDetailRow);
            SaveCommand = new ReactiveCommand().WithSubscribe(async () => await SaveAsync());
            CancelCommand = new ReactiveCommand().WithSubscribe(Close);

            Task.Run(async () => await LoadAsync());
        }

        public ReactiveCommand LoadCommand { get; }
        public ReactiveCommand AddCommand { get; }
        public ReactiveCommand<Tag> EditCommand { get; }
        public ReactiveCommand<Tag> DeleteCommand { get; }
        public ReactiveCommand AddDetailCommand { get; }
        public ReactiveCommand<TagDetailRow> RemoveDetailCommand { get; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand CancelCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var list = await _tagSvc.GetAllTagsAsync(new TagSearchDto());
                Tags.Value = new ObservableCollection<Tag>(list);

                // 加载 PLC 设备列表
                var plcDevices = await _plcDeviceSvc.GetAllPlcDevicesAsync();
                PlcDevices.Value = new ObservableCollection<PlcDevice>(plcDevices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载点位失败");
            }
        }

        private async void OpenAdd()
        {
            // 确保已加载 PLC 设备列表
            if (PlcDevices.Value.Count == 0)
            {
                var plcDevices = await _plcDeviceSvc.GetAllPlcDevicesAsync();
                PlcDevices.Value = new ObservableCollection<PlcDevice>(plcDevices);
            }

            // 如果有 PLC 设备，默认选择第一个
            var defaultPlcId = PlcDevices.Value.Count > 0 ? PlcDevices.Value[0].Id : 1;

            EditingTag.Value = new Tag { PlcDeviceId = defaultPlcId, Remarks = string.Empty, TagDetailDataArray = new() };
            TagDetailRows.Value = new ObservableCollection<TagDetailRow>();
            IsEditing.Value = false;
            IsDialogOpen.Value = true;
        }

        private void OpenEdit(Tag tag)
        {
            if (tag == null) return;
            EditingTag.Value = new Tag
            {
                Id = tag.Id,
                PlcDeviceId = tag.PlcDeviceId,
                Remarks = tag.Remarks,
                TagDetailDataArray = tag.TagDetailDataArray
            };

            TagDetailRows.Value = new ObservableCollection<TagDetailRow>((tag.TagDetailDataArray ?? new()).Select(d => new TagDetailRow(d)));

            IsEditing.Value = true;
            IsDialogOpen.Value = true;
        }

        private void AddDetailRow()
        {
            TagDetailRows.Value.Add(new TagDetailRow());
        }

        private void RemoveDetailRow(TagDetailRow row)
        {
            if (row == null) return;
            TagDetailRows.Value.Remove(row);
        }

        private async Task SaveAsync()
        {
            try
            {
                var entity = EditingTag.Value;

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

                // 获取有效的行（过滤掉空行）
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

                // Address 去重
                var dup = validRows.GroupBy(r => r.Address.Value.Trim()).FirstOrDefault(g => g.Count() > 1);
                if (dup != null)
                {
                    MessageBox.Show($"Address 不能重复：{dup.Key}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 转换为实体并保存
                entity.TagDetailDataArray = validRows.Select(r => r.ToEntity()).ToList();

                if (entity.Id == 0)
                {
                    // 新增：确保导航属性为 null，避免 EF 尝试插入关联实体
                    entity.PlcDevice = null;
                    
                    await _tagSvc.AddTagAsync(entity);
                }
                else
                {
                    await _tagSvc.UpdateTagAsync(entity);
                }

                await LoadAsync();
                Close();
                MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async Task DeleteAsync(Tag tag)
        {
            if (tag == null) return;
            var result = MessageBox.Show($"确定删除点位组 ID={tag.Id} 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _tagSvc.DeleteTagAsync(tag.Id);
                await LoadAsync();
                MessageBox.Show("删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除点位失败");
                MessageBox.Show($"删除点位失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close()
        {
            IsDialogOpen.Value = false;
        }
    }
}
