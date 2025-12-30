using DeviceManage.Commands;
using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceManage.ViewModels
{
    public class PlcDeviceViewModel : ViewModelBase
    {
        public readonly IPlcDeviceService _plcDeviceService;
        private ObservableCollection<PlcDevice> _plcDevices;
        private PlcDevice _selectedPlcDevice;
        private bool _isEditing;
        private readonly ILogger<PlcDeviceViewModel> _logger;

        public PlcDeviceViewModel(IPlcDeviceService plcDeviceService, ILogger<PlcDeviceViewModel> logger)
        {
            this._plcDeviceService = plcDeviceService;
            _plcDevices = new ObservableCollection<PlcDevice>();
            _selectedPlcDevice = new PlcDevice();
            LoadPlcDevicesCommand = new RelayCommand(async () => await LoadPlcDevicesAsync());
            AddPlcDeviceCommand = new RelayCommand(async () => await AddPlcDeviceAsync(), () => !IsEditing);
            UpdatePlcDeviceCommand = new RelayCommand(async () => await UpdatePlcDeviceAsync(), () => SelectedPlcDevice != null && IsEditing);
            DeletePlcDeviceCommand = new RelayCommand(async () => await DeletePlcDeviceAsync(), () => SelectedPlcDevice != null && !IsEditing);
            CancelCommand = new RelayCommand(CancelEdit, () => IsEditing);
            EditCommand = new RelayCommand(EditPlcDevice, () => SelectedPlcDevice != null && !IsEditing);

            // 初始化时加载数据
            Task.Run(async () => await LoadPlcDevicesAsync());
            _logger = logger;
        }

        public ObservableCollection<PlcDevice> PlcDevices
        {
            get => _plcDevices;
            set
            {
                _plcDevices = value;
                OnPropertyChanged();
            }
        }

        public PlcDevice SelectedPlcDevice
        {
            get => _selectedPlcDevice;
            set
            {
                _selectedPlcDevice = value;
                OnPropertyChanged();
                // 更新命令状态
                ((RelayCommand)UpdatePlcDeviceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeletePlcDeviceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                // 更新命令状态
                ((RelayCommand)AddPlcDeviceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdatePlcDeviceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeletePlcDeviceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadPlcDevicesCommand { get; }
        public ICommand AddPlcDeviceCommand { get; }
        public ICommand UpdatePlcDeviceCommand { get; }
        public ICommand DeletePlcDeviceCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }

        private async Task LoadPlcDevicesAsync()
        {
            try
            {
                var devices = await _plcDeviceService.GetAllPlcDevicesAsync();
                PlcDevices = new ObservableCollection<PlcDevice>(devices);
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
                var newDevice = await _plcDeviceService.AddPlcDeviceAsync(SelectedPlcDevice);
                PlcDevices.Add(newDevice);
                SelectedPlcDevice = new PlcDevice(); // 清空当前选中项
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
                var updatedDevice = await _plcDeviceService.UpdatePlcDeviceAsync(SelectedPlcDevice);
                var index = PlcDevices.IndexOf(PlcDevices.First(d => d.Id == SelectedPlcDevice.Id));
                if (index != -1)
                {
                    PlcDevices[index] = updatedDevice;
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
            if (SelectedPlcDevice != null && SelectedPlcDevice.Id > 0)
            {
                try
                {
                    await _plcDeviceService.DeletePlcDeviceAsync(SelectedPlcDevice.Id);
                    PlcDevices.Remove(SelectedPlcDevice);
                    SelectedPlcDevice = new PlcDevice(); // 清空当前选中项
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
            IsEditing = true;
        }

        private void CancelEdit()
        {
            IsEditing = false;
            // 重新加载数据以取消未保存的更改
            Task.Run(async () => await LoadPlcDevicesAsync());
        }
    }
}
