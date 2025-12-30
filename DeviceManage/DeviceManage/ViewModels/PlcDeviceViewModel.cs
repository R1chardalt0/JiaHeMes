using DeviceManage.Commands;
using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace DeviceManage.ViewModels
{
    public class PlcDeviceViewModel : ViewModelBase
    {
        public readonly IPlcDeviceService _plcDeviceService;
        private readonly ILogger<PlcDeviceViewModel> _logger;
        
        public ReactiveProperty<ObservableCollection<PlcDevice>> PlcDevices { get; }
        public ReactiveProperty<PlcDevice> SelectedPlcDevice { get; }
        public ReactiveProperty<bool> IsEditing { get; }

        public PlcDeviceViewModel(IPlcDeviceService plcDeviceService, ILogger<PlcDeviceViewModel> logger)
        {
            this._plcDeviceService = plcDeviceService;
            this._logger = logger;
            
            // 初始化ReactiveProperty
            PlcDevices = new ReactiveProperty<ObservableCollection<PlcDevice>>(new ObservableCollection<PlcDevice>());
            SelectedPlcDevice = new ReactiveProperty<PlcDevice>(new PlcDevice());
            IsEditing = new ReactiveProperty<bool>(false);
            
            // 初始化命令
            LoadPlcDevicesCommand = new ReactiveCommand().WithSubscribe(async () => await LoadPlcDevicesAsync());
            AddPlcDeviceCommand = new ReactiveCommand();
            AddPlcDeviceCommand.Subscribe(async _ => await AddPlcDeviceAsync());
            UpdatePlcDeviceCommand = new ReactiveCommand();
            UpdatePlcDeviceCommand.Subscribe(async _ => await UpdatePlcDeviceAsync());
            DeletePlcDeviceCommand = new ReactiveCommand();
            DeletePlcDeviceCommand.Subscribe(async _ => await DeletePlcDeviceAsync());
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(_ => CancelEdit());
            EditCommand = new ReactiveCommand();
            EditCommand.Subscribe(_ => EditPlcDevice());

            // 初始化时加载数据
            Task.Run(async () => await LoadPlcDevicesAsync());
        }

        

        public ReactiveCommand LoadPlcDevicesCommand { get; }
        public ReactiveCommand AddPlcDeviceCommand { get; }
        public ReactiveCommand UpdatePlcDeviceCommand { get; }
        public ReactiveCommand DeletePlcDeviceCommand { get; }
        public ReactiveCommand EditCommand { get; }
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
                // 可以添加日志记录或错误处理
                _logger.LogError($"加载PLC设备失败: {ex.Message}");
            }
        }

        private async Task AddPlcDeviceAsync()
        {
            try
            {
                var newDevice = await _plcDeviceService.AddPlcDeviceAsync(SelectedPlcDevice.Value);
                PlcDevices.Value.Add(newDevice);
                SelectedPlcDevice.Value = new PlcDevice(); // 清空当前选中项
            }
            catch (Exception ex)
            {
                // 可以添加日志记录或错误处理
                _logger.LogError($"添加PLC设备失败: {ex.Message}");
            }
        }

        private async Task UpdatePlcDeviceAsync()
        {
            try
            {
                var updatedDevice = await _plcDeviceService.UpdatePlcDeviceAsync(SelectedPlcDevice.Value);
                var index = PlcDevices.Value.IndexOf(PlcDevices.Value.First(d => d.Id == SelectedPlcDevice.Value.Id));
                if (index != -1)
                {
                    PlcDevices.Value[index] = updatedDevice;
                }
                CancelEdit(); // 完成编辑后取消编辑状态
            }
            catch (Exception ex)
            {
                // 可以添加日志记录或错误处理
                _logger.LogError($"更新PLC设备失败: {ex.Message}");
            }
        }

        private async Task DeletePlcDeviceAsync()
        {
            if (SelectedPlcDevice.Value != null && SelectedPlcDevice.Value.Id > 0)
            {
                try
                {
                    await _plcDeviceService.DeletePlcDeviceAsync(SelectedPlcDevice.Value.Id);
                    PlcDevices.Value.Remove(SelectedPlcDevice.Value);
                    SelectedPlcDevice.Value = new PlcDevice(); // 清空当前选中项
                }
                catch (Exception ex)
                {
                    // 可以添加日志记录或错误处理
                    _logger.LogError($"删除PLC设备失败: {ex.Message}");
                }
            }
        }

        private void EditPlcDevice()
        {
            IsEditing.Value = true;
        }

        private void CancelEdit()
        {
            IsEditing.Value = false;
            // 重新加载数据以取消未保存的更改
            Task.Run(async () => await LoadPlcDevicesAsync());
        }
    }
}
