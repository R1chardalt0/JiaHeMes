using DeviceManage.Commands;
using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using HandyControl.Controls;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeviceManage.ViewModels
{
    public class PlcDeviceViewModel : ViewModelBase
    {
        public readonly IPlcDeviceService _plcDeviceService;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<PlcDeviceViewModel> _logger;

        /// <summary>
        /// PLC 设备列表
        /// </summary>
        public ReactiveProperty<ObservableCollection<PlcDevice>> PlcDevices { get; }

        /// <summary>
        /// 当前选中的行（仅用于高亮）
        /// </summary>
        public ReactiveProperty<PlcDevice> SelectedPlcDevice { get; }

        /// <summary>
        /// 弹出框中正在编辑的实体（新增或编辑）
        /// </summary>
        public ReactiveProperty<PlcDevice> EditingPlcDevice { get; }

        /// <summary>
        /// 是否处于编辑模式（true: 编辑现有记录；false: 新增）
        /// </summary>
        public ReactiveProperty<bool> IsEditing { get; }

        /// <summary>
        /// 是否显示弹出框
        /// </summary>
        public ReactiveProperty<bool> IsDialogOpen { get; }

        public PlcDeviceViewModel(IPlcDeviceService plcDeviceService, IRecipeService recipeService, ILogger<PlcDeviceViewModel> logger)
        {
            this._plcDeviceService = plcDeviceService;
            this._recipeService = recipeService;
            this._logger = logger;

            // 初始化ReactiveProperty
            PlcDevices = new ReactiveProperty<ObservableCollection<PlcDevice>>(new ObservableCollection<PlcDevice>());
            SelectedPlcDevice = new ReactiveProperty<PlcDevice>(new PlcDevice());
            EditingPlcDevice = new ReactiveProperty<PlcDevice>(new PlcDevice());
            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);

            // 初始化命令
            LoadPlcDevicesCommand = new ReactiveCommand().WithSubscribe(async () => await LoadPlcDevicesAsync());
            AddPlcDeviceCommand = new ReactiveCommand();
            AddPlcDeviceCommand.Subscribe(_ => OpenAddDialog());
            UpdatePlcDeviceCommand = new ReactiveCommand().WithSubscribe(async () => await SavePlcDeviceAsync());
            EditCommand = new ReactiveCommand<PlcDevice>().WithSubscribe(device => OpenEditDialog(device));
            DeletePlcDeviceCommand = new ReactiveCommand<PlcDevice>().WithSubscribe(async device => await DeletePlcDeviceAsync(device));
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(_ => CloseDialog());

            Task.Run(async () => await LoadPlcDevicesAsync());
        }



        public ReactiveCommand LoadPlcDevicesCommand { get; }
        public ReactiveCommand AddPlcDeviceCommand { get; }
        public ReactiveCommand UpdatePlcDeviceCommand { get; }
        public ReactiveCommand<PlcDevice> DeletePlcDeviceCommand { get; }
        public ReactiveCommand<PlcDevice> EditCommand { get; }
        public ReactiveCommand CancelCommand { get; }

        private async Task LoadPlcDevicesAsync()
        {
            try
            {
                var devices = await _plcDeviceService.GetAllPlcDevicesAsync();
                PlcDevices.Value = new ObservableCollection<PlcDevice>(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError($"加载PLC设备失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存弹出框中的 PLC 设备（新增或更新）
        /// </summary>
        private async Task SavePlcDeviceAsync()
        {
            try
            {
                if (EditingPlcDevice.Value == null)
                {
                    _logger.LogWarning("保存PLC设备失败：编辑设备对象为空。");
                    return;
                }

                // 获取当前编辑的设备对象
                var currentDevice = EditingPlcDevice.Value;

                // 基础校验，防止必填字段为空导致接口或数据库异常
                if (string.IsNullOrWhiteSpace(currentDevice.PLCName) ||
                    string.IsNullOrWhiteSpace(currentDevice.IPAddress))
                {
                    _logger.LogWarning("保存PLC设备失败：PLC名称或IP地址为空。");
                    return;
                }

                // 记录保存前的数据，用于调试
                _logger.LogInformation($"准备保存PLC设备 - ID: {currentDevice.Id}, 名称: {currentDevice.PLCName}, IP: {currentDevice.IPAddress}, 端口: {currentDevice.Port}, 协议: {currentDevice.Protocolc}, 型号: {currentDevice.Model}, 描述: {currentDevice.Remarks}");

                if (currentDevice.Id == 0)
                {
                    var newDeviceData = new PlcDevice
                    {
                        Id = 0,
                        PLCName = currentDevice.PLCName?.Trim() ?? string.Empty,
                        IPAddress = currentDevice.IPAddress?.Trim() ?? string.Empty,
                        Port = currentDevice.Port,
                        Protocolc = currentDevice.Protocolc?.Trim() ?? string.Empty,
                        Model = currentDevice.Model?.Trim() ?? string.Empty,
                        Remarks = currentDevice.Remarks?.Trim() ?? string.Empty
                    };

                    var newDevice = await _plcDeviceService.AddPlcDeviceAsync(newDeviceData);
                    _logger.LogInformation($"成功新增PLC设备 - ID: {newDevice.Id}, 名称: {newDevice.PLCName}");
                }
                else
                {
                    var deviceToUpdate = new PlcDevice
                    {
                        Id = currentDevice.Id,
                        PLCName = currentDevice.PLCName?.Trim() ?? string.Empty,
                        IPAddress = currentDevice.IPAddress?.Trim() ?? string.Empty,
                        Port = currentDevice.Port,
                        Protocolc = currentDevice.Protocolc?.Trim() ?? string.Empty,
                        Model = currentDevice.Model?.Trim() ?? string.Empty,
                        Remarks = currentDevice.Remarks?.Trim() ?? string.Empty
                    };

                    var updatedDevice = await _plcDeviceService.UpdatePlcDeviceAsync(deviceToUpdate);
                    _logger.LogInformation($"成功更新PLC设备 - ID: {updatedDevice.Id}, 名称: {updatedDevice.PLCName}, IP: {updatedDevice.IPAddress}, 端口: {updatedDevice.Port}, 协议: {updatedDevice.Protocolc}, 型号: {updatedDevice.Model}, 描述: {updatedDevice.Remarks}");
                }

                await LoadPlcDevicesAsync();
                SelectedPlcDevice.Value = new PlcDevice();
                CloseDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存PLC设备失败: {ex.Message}");
            }
        }

        private async Task DeletePlcDeviceAsync(PlcDevice device)
        {
            if (device == null)
            {
                _logger.LogWarning("删除PLC设备失败：设备对象为空。");
                return;
            }

            if (device.Id <= 0)
            {
                _logger.LogWarning($"删除PLC设备失败：设备ID无效 (ID: {device.Id})。");
                return;
            }

            // 显示确认删除对话框
            var deviceName = string.IsNullOrWhiteSpace(device.PLCName) ? $"ID: {device.Id}" : device.PLCName;
            var result = MessageBox.Show(
                $"确定要删除PLC设备 \"{deviceName}\" 吗？\n\n删除后无法恢复！",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            // 用户取消删除
            if (result != MessageBoxResult.Yes)
            {
                _logger.LogInformation($"用户取消删除PLC设备 (ID: {device.Id}, 名称: {deviceName})");
                return;
            }

            try
            {
                await _plcDeviceService.DeletePlcDeviceAsync(device.Id);
                _logger.LogInformation($"成功删除PLC设备 (ID: {device.Id}, 名称: {deviceName})");

                await LoadPlcDevicesAsync();
                if (SelectedPlcDevice.Value != null && SelectedPlcDevice.Value.Id == device.Id)
                {
                    SelectedPlcDevice.Value = new PlcDevice();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除PLC设备失败 (ID: {device.Id}): {ex.Message}");
                MessageBox.Show($"删除PLC设备失败：{ex.Message}", "删除失败",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开“新增”弹出框
        /// </summary>
        private void OpenAddDialog()
        {
            EditingPlcDevice.Value = new PlcDevice();
            IsEditing.Value = false;
            IsDialogOpen.Value = true;
        }

        /// <summary>
        /// 打开"编辑"弹出框
        /// </summary>
        private void OpenEditDialog(PlcDevice device)
        {
            if (device == null)
            {
                _logger.LogWarning("打开编辑对话框失败：设备对象为空。");
                return;
            }

            if (device.Id <= 0)
            {
                _logger.LogWarning($"打开编辑对话框失败：设备ID无效 (ID: {device.Id})。");
                return;
            }

            try
            {
                // 创建副本，避免直接修改列表中的对象
                EditingPlcDevice.Value = new PlcDevice
                {
                    Id = device.Id,
                    PLCName = device.PLCName ?? string.Empty,
                    IPAddress = device.IPAddress ?? string.Empty,
                    Port = device.Port,
                    Protocolc = device.Protocolc ?? string.Empty,
                    Model = device.Model ?? string.Empty,
                    Remarks = device.Remarks ?? string.Empty
                };

                IsEditing.Value = true;
                IsDialogOpen.Value = true;
                _logger.LogInformation($"打开编辑对话框 (ID: {device.Id}, 名称: {device.PLCName})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"打开编辑对话框失败 (ID: {device?.Id}): {ex.Message}");
            }
        }

        /// <summary>
        /// 关闭弹出框并丢弃未保存更改
        /// </summary>
        private void CloseDialog()
        {
            IsDialogOpen.Value = false;
            EditingPlcDevice.Value = new PlcDevice();
        }
        
        /// <summary>
        /// 根据配方ID获取标签列表
        /// </summary>
        public async Task<List<DeviceManage.Models.Tag>> GetTagListByRecipeIdAsync(int id)
        {
            try
            {
                return await _recipeService.GetTagListByRecipeIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据配方ID获取标签列表失败 (RecipeId: {id}): {ex.Message}");
                throw;
            }
        }
    }
}
