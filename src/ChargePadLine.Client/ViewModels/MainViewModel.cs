using System.Windows.Input;
using ChargePadLine.Client.Commands;
using ChargePadLine.Client.Services;

namespace ChargePadLine.Client.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string _statusMessage = "就绪";
    private string _apiResponse = "";

    public MainViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync());
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public string ApiResponse
    {
        get => _apiResponse;
        set
        {
            _apiResponse = value;
            OnPropertyChanged();
        }
    }

    public ICommand TestConnectionCommand { get; }

    private async Task TestConnectionAsync()
    {
        try
        {
            StatusMessage = "正在测试连接...";
            // 这里可以调用一个测试接口，比如健康检查
             var result = await _apiClient.GetAsync<object>("/api/CompanyDashboard/GetOverview");
           
            StatusMessage = "连接成功！";
            ApiResponse = "后端API连接正常";
        }
        catch (Exception ex)
        {
            StatusMessage = $"连接失败: {ex.Message}";
            ApiResponse = ex.ToString();
        }
    }
}

