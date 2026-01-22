using JY_Print;
using JY_Print.model;
using Newtonsoft.Json;
using NJCH_Station;
using Seagull.BarTender.Print;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FJY_Print
{
    public partial class Main : Form
    {
        private const string PrinterName = "ZDesigner ZE511 LH-300dpi ZPL";
        private const int MonitoringInterval = 500;
        private readonly S7NetConnect _s7net = new S7NetConnect();
        private CancellationTokenSource _monitoringTokenSource;
        private bool _isPrinting = false;
        private bool res = false;

        public Main()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                // 初始化PLC配置（首次运行时创建配置文件）
                Config.InitializePLCConfig();

                await InitializePLCConnectionAsync();
                StartMonitoring();
            }
            catch (Exception ex)
            {
                AppendLog($"初始化失败: {ex.Message}");
            }
        }

        private async Task InitializePLCConnectionAsync()
        {
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    AppendLog($"尝试连接PLC（第 {retryCount + 1} 次）...");

                    string plcIP = Config.GetPLCIP();
                    int plcPort = Config.GetPLCPort();
                    //await _plcConn.ConnectAsync(plcIP, plcPort);
                    bool res = _s7net.Connect(plcIP, plcPort).IsSuccess;
                    //bool res = await _plcConn.ConnectAsync("127.0.0.1", 502);
                    if (res)
                    {
                        AppendLog("PLC连接成功");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"连接失败: {ex.Message}");
                }
                retryCount++;
                await Task.Delay(2000); // 等待2秒后重试
            }

          
        }

        private void StartMonitoring()
        {
            _monitoringTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorPLCAsync(_monitoringTokenSource.Token), _monitoringTokenSource.Token);
        }

        private async Task MonitorPLCAsync(CancellationToken token)
        {
            bool wasConnected = _s7net.IsConnect;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 自动重连逻辑
                    if (!_s7net.IsConnect)
                    {
                        AppendLog("PLC未连接，尝试重连...");

                        // 如果之前是连接状态，现在断开了，提示用户
                        if (wasConnected)
                        {
                            ShowPLCDisconnectedAlert();
                            wasConnected = false;
                        }

                        await InitializePLCConnectionAsync();
                        await Task.Delay(5000, token); // 等待5秒后重试
                        continue;
                    }
                    else
                    {
                        // 如果刚刚恢复连接
                        if (!wasConnected)
                        {
                            AppendLog("PLC重新连接成功");
                            ShowPLCReconnectedAlert();
                            wasConnected = true;
                        }
                    }

                    // 正常监控逻辑
                    if (!_isPrinting)
                    {
                        var req = _s7net.ReadBool("DB4010.7.5").Content;
                        var respon = _s7net.ReadBool("DB4010.14.0").Content;

                        if (req && !respon)
                        {
                            //通过id获取缓存码数据
                            var res = GetBoxCacheCodeById(1);
                            if (res.Success)
                            {
                                //打印标签
                                await PrintLabelAsync(res.Data);
                                _s7net.Write("DB4010.0.6", true);
                            }
                            else
                            {
                                AppendLog($"获取缓存码失败: {res.Messages}");
                                _s7net.Write("DB4010.0.7", true);
                            }
                        }
                        else
                        {
                            _s7net.Write("DB4010.14.0", false);
                            AppendLog("请求打印结束！");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"监控异常: {ex.Message}");
                    _s7net.Disconnect(); // 发生异常时强制断开
                }

                await Task.Delay(MonitoringInterval, token);
            }
        }

        private void ShowPLCDisconnectedAlert()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowPLCDisconnectedAlert);
                return;
            }

            MessageBox.Show(this, "PLC连接已断开，请检查网络或设备！", "PLC断开", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowPLCReconnectedAlert()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowPLCReconnectedAlert);
                return;
            }

            MessageBox.Show(this, "PLC已重新连接。", "PLC恢复", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private async Task PrintLabelAsync(BoxCodeCache codeCache)
        {
            if (codeCache == null)
            {
                AppendLog("错误: 型号名称为空");
                return;
            }

            _isPrinting = true;
            try
            {
                UpdateLabelText(codeCache.CacheBoxCode);
                AppendLog($"开始打印 {codeCache.CacheBoxCode}");

                var result = await Task.Run(() =>
                {
                    using (var engine = new Engine())
                    {
                        engine.Start();
                        var format = engine.Documents.Open(_selectedLabelPath); // 使用用户选择的路径
                        try
                        {
                            format.SubStrings["SN"].Value = codeCache.CacheBoxCode;
                            format.SubStrings["CN"].Value = codeCache.CustomerPartNumber;
                            format.SubStrings["QU"].Value = codeCache.Quantity;
                            format.SubStrings["DF"].Value = codeCache.CreateTime.ToString("yyyy/MM/dd HH:mm:ss");
                            format.SubStrings["SH"].Value = codeCache.Shift;
                            format.SubStrings["DS"].Value = codeCache.CreateTime.ToString("yyyyMMdd") + codeCache.SerialNumber;
                            format.PrintSetup.PrinterName = PrinterName;
                            format.PrintSetup.IdenticalCopiesOfLabel = 1;
                            return format.Print();
                        }
                        finally
                        {
                            format.Close(SaveOptions.DoNotSaveChanges);
                        }
                    }
                });

                AppendLog($"打印结果:{codeCache.CacheBoxCode}， {result}");
            }
            catch (Exception ex)
            {
                AppendLog($"打印异常: {ex.Message}");
            }
            finally
            {
                _isPrinting = false;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string sn = txtManualSN.Text.Trim();
                if (string.IsNullOrWhiteSpace(sn))
                {
                    AppendLog("错误: 请输入SN码");
                    return;
                }

                AppendLog($"开始手动打印，SN码: {sn}");

                var result = await Task.Run(() =>
                {
                    Engine engine = null;
                    LabelFormatDocument format = null;
                    try
                    {
                        engine = new Engine();
                        engine.Start();
                        format = engine.Documents.Open(_selectedLabelPath);
                        format.SubStrings["SN"].Value = sn;  // 使用手动输入的SN码
                        format.PrintSetup.PrinterName = PrinterName;
                        format.PrintSetup.IdenticalCopiesOfLabel = 1;
                        var printResult = format.Print();

                        format.Close(SaveOptions.DoNotSaveChanges);
                        format = null;

                        return printResult;
                    }
                    finally
                    {
                        if (format != null)
                            format.Close(SaveOptions.DoNotSaveChanges);
                        if (engine != null)
                            engine.Stop();
                    }
                });

                AppendLog($"手动打印完成，结果: {result}");
            }
            catch (OperationCanceledException)
            {
                AppendLog("手动打印操作已取消");
            }
            catch (Exception ex)
            {
                AppendLog($"手动打印失败: {ex.Message}");
            }
        }

        private void UpdateLabelText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => label1.Text = text));
            }
            else
            {
                label1.Text = text;
            }
        }

        private string CleanString(string input) =>
            input?.Replace("\0", "").Replace("\r", "").Replace("\n", "").Replace(" ", "");

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                    textBox1.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}   {message}\r\n")));
            }
            else
            {
                textBox1.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}   {message}\r\n");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            AppendLog("正在关闭窗体，释放资源...");
            _monitoringTokenSource?.Cancel();
            _s7net?.Dispose();
            base.OnFormClosing(e);
        }
        private string _selectedLabelPath = @"D:\\Users\\Desktop\\宁波拓普\\printModel.btw";

        // 新增文件选择事件
        private void btnSelectLabelFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "BarTender文件|*.btw";
                openFileDialog.InitialDirectory = Path.GetDirectoryName(_selectedLabelPath);
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _selectedLabelPath = openFileDialog.FileName;
                    UpdateLabelFileDisplay(_selectedLabelPath);
                }
            }
        }

        private void UpdateLabelFileDisplay(string path)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => lblSelectedFile.Text = $"当前标签文件: {path}"));
            }
            else
            {
                lblSelectedFile.Text = $"当前标签文件: {path}";
            }
        }

        // 新增PLC配置入口事件
        private void btnPLCConfig_Click(object sender, EventArgs e)
        {
            using (var configForm = new PLCConfigForm())
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // 保存配置并尝试重连
                    _s7net.Disconnect();
                    _ = InitializePLCConnectionAsync();
                }
            }
        }


        /// <summary> 
        /// 通过get请求获取缓存cacheCode码的内容 
        /// </summary> 
        /// <param name="url"></param> 
        /// <returns></returns> 
        private RespDto GetCacheCode(string url)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    string response = client.DownloadString(url);
                    return JsonConvert.DeserializeObject<RespDto>(response);
                }
            }
            catch (Exception ex)
            {
                AppendLog($": {ex.Message}");
                return null;
            }
        }

        /// <summary> 
        /// 获取指定ID的箱体缓存码数据 
        /// </summary> 
        /// <param name="id">箱体ID</param> 
        /// <returns>箱体缓存码数据</returns> 
        private RespDto GetBoxCacheCodeById(int id)
        {
            string baseUrl = "http://localhost:8809/saomaget/GetBoxCacheCode";
            string url = $"{baseUrl}?id={id}";
            return GetCacheCode(url);
        }
    }
}