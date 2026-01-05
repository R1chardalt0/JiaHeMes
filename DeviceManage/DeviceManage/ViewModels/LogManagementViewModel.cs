using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Dto;
using HandyControl.Controls;
using Prism.Commands;
using Reactive.Bindings;
using DeviceManage.CustomerControl;
using DeviceManage.ViewModels;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 日志管理 ViewModel：按模块/用户/操作类型查询并分页展示
    /// </summary>
    public class LogManagementViewModel : ViewModelBase
    {
        private readonly ILogService _logService;

        // 列表数据
        public ReactiveProperty<ObservableCollection<OperationLog>> Logs { get; } =
            new(new ObservableCollection<OperationLog>());

        // 搜索条件
        public ReactiveProperty<string> SearchModule { get; } = new(string.Empty);
        public ReactiveProperty<string> SearchUsername { get; } = new(string.Empty);
        public ReactiveProperty<string> SearchOperationType { get; } = new(string.Empty); // OperationTypeString

            // 操作类型下拉项
        public ObservableCollection<OperationTypeOption> OperationTypeOptions { get; }

        // 分页
        public PageListViewModel PageVM { get; }

        // Commands
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SearchCommand { get; }
        public DelegateCommand ResetCommand { get; }

        public LogManagementViewModel(ILogService logService)
        {
            _logService = logService;

                        OperationTypeOptions = new ObservableCollection<OperationTypeOption>(
                Enum.GetValues(typeof(OperationType)).Cast<OperationType>()
                    .Select(o => new OperationTypeOption { Display = EnumDescriptionHelper.GetDescription(o), Value = o.ToString() }));

            PageVM = new PageListViewModel
            {
                PageSize = 20,
                PageIndex = 1
            };
            PageVM.PageChangedCommand = new DelegateCommand(async () => await LoadAsync());
            PageVM.PageSizeChangedCommand = new DelegateCommand(async () => await LoadAsync());

            LoadCommand = new DelegateCommand(async () => await LoadAsync());
            SearchCommand = new DelegateCommand(async () =>
            {
                PageVM.PageIndex = 1;
                await LoadAsync();
            });
            ResetCommand = new DelegateCommand(async () =>
            {
                SearchModule.Value = string.Empty;
                SearchUsername.Value = string.Empty;
                SearchOperationType.Value = string.Empty;
                PageVM.PageIndex = 1;
                await LoadAsync();
            });

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var dto = new LogSearchDto
                {
                    current = PageVM.PageIndex,
                    pageSize = PageVM.PageSize,
                    Module = SearchModule.Value.Trim(),
                    Username = SearchUsername.Value.Trim(),
                    OperationType = SearchOperationType.Value.Trim()
                };
                var paged = await _logService.GetLogsAsync(dto);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Logs.Value = new ObservableCollection<OperationLog>(paged);
                    PageVM.Total = paged.TotalCounts;
                    PageVM.TotalPage = paged.TotalPages;
                    PageVM.Init();
                });
            }
            catch (Exception ex)
            {
                Growl.Error($"加载日志失败：{ex.Message}");
            }
        }
    }
}
