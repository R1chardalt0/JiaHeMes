using HslCommunication.Profinet.Siemens.S7PlusHelper;
using JY_Print;
using JY_Print.model;
using Newtonsoft.Json;
using NJCH_Station;
using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FJY_Print
{
    public partial class Main : Form
    {
        private const string PrinterName = "ZT411 300 (dpi)";
        private const int MonitoringInterval = 500;
        private readonly S7NetConnect _s7net = new S7NetConnect();
        private CancellationTokenSource _monitoringTokenSource;
        private bool _isPrinting = false;
        private DateTime _currentDate = DateTime.Today;
        private int _serialNumber = 1;


        public Main()
        {
            InitializeComponent();
            InitializeLabelPathDisplay();
            InitializeSNTextBoxEvents();
            InitializeAsync();
        }

        /// <summary>
        /// 初始化标签路径显示
        /// </summary>
        private void InitializeLabelPathDisplay()
        {
            UpdateLabelFileDisplay(_selectedLabelPath);
        }

        /// <summary>
        /// 初始化SN码文本框回车键事件
        /// </summary>
        private void InitializeSNTextBoxEvents()
        {
            // 为所有30个SN码文本框添加回车键事件处理
            for (int i = 1; i <= 30; i++)
            {
                string controlName = $"sncode{i}";
                var textBox = this.GetType().GetField(controlName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(this) as TextBox;

                if (textBox != null)
                {
                    textBox.KeyDown += SNTextBox_KeyDown;
                }
            }
        }

        /// <summary>
        /// SN码文本框回车键事件处理
        /// </summary>
        private void SNTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true; // 防止发出系统提示音
                e.SuppressKeyPress = true;

                // 根据当前文本框决定下一个文本框
                TextBox currentTextBox = (TextBox)sender;
                TextBox nextTextBox = GetNextSNTextBox(currentTextBox);

                if (nextTextBox != null)
                {
                    nextTextBox.Focus();
                }
            }
        }

        /// <summary>
        /// 获取下一个SN码文本框
        /// </summary>
        private TextBox GetNextSNTextBox(TextBox currentTextBox)
        {
            // 根据文本框名称获取下一个文本框（支持30个条码）
            string currentName = currentTextBox.Name;
            int currentNumber = int.Parse(currentName.Replace("sncode", ""));

            if (currentNumber < 30)
            {
                int nextNumber = currentNumber + 1;
                string nextControlName = $"sncode{nextNumber}";

                // 使用反射获取控件
                var nextControl = this.GetType().GetField(nextControlName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(this) as TextBox;

                return nextControl;
            }

            return null; // 最后一个文本框，没有下一个
        }

        #region PLC连接与监控
        private async void InitializeAsync()
        {
            try
            {
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
                    bool res = _s7net.Connect(plcIP, plcPort).IsSuccess;
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

        private void StartMonitoring()
        {
            _monitoringTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorPLCAsync(_monitoringTokenSource.Token), _monitoringTokenSource.Token);
        }

        #endregion

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
                        var responOK = _s7net.ReadBool("DB4010.0.6").Content;
                        var responNG = _s7net.ReadBool("DB4010.0.7").Content;
                        if (req && !respon)
                        {
                            //todo 拿到plc发送条码数据
                            var sn1 = _s7net.ReadString("DB4018.160", 100);
                            var sn2 = _s7net.ReadString("DB4018.262", 100);
                            var sn3 = _s7net.ReadString("DB4018.364", 100);
                            var sn4 = _s7net.ReadString("DB4018.466", 100);
                            var sn5 = _s7net.ReadString("DB4018.568", 100);
                            var sn6 = _s7net.ReadString("DB4018.670", 100);
                            var sn7 = _s7net.ReadString("DB4018.772", 100);
                            var sn8 = _s7net.ReadString("DB4018.874", 100);
                            var sn9 = _s7net.ReadString("DB4018.976", 100);
                            var sn10 = _s7net.ReadString("DB4018.1078", 100);
                            var sn11 = _s7net.ReadString("DB4018.1180", 100);
                            var sn12 = _s7net.ReadString("DB4018.1282", 100);
                            var sn13 = _s7net.ReadString("DB4018.1384", 100);
                            var sn14 = _s7net.ReadString("DB4018.1486", 100);
                            var sn15 = _s7net.ReadString("DB4018.1588", 100);
                            var sn16 = _s7net.ReadString("DB4018.1690", 100);
                            var sn17 = _s7net.ReadString("DB4018.1792", 100);
                            var sn18 = _s7net.ReadString("DB4018.1894", 100);
                            var sn19 = _s7net.ReadString("DB4018.1996", 100);
                            var sn20 = _s7net.ReadString("DB4018.2098", 100);
                            var sn21 = _s7net.ReadString("DB4018.2200", 100);
                            var sn22 = _s7net.ReadString("DB4018.2302", 100);
                            var sn23 = _s7net.ReadString("DB4018.2404", 100);
                            var sn24 = _s7net.ReadString("DB4018.2506", 100);
                            var sn25 = _s7net.ReadString("DB4018.2608", 100);
                            var sn26 = _s7net.ReadString("DB4018.2710", 100);
                            var sn27 = _s7net.ReadString("DB4018.2812", 100);
                            var sn28 = _s7net.ReadString("DB4018.2914", 100);
                            var sn29 = _s7net.ReadString("DB4018.3016", 100);
                            var sn30 = _s7net.ReadString("DB4018.3118", 100);


                            var snList = BuildSnList(sn1, sn2, sn3, sn4, sn5, sn6, sn7, sn8, sn9, sn10, sn11, sn12, sn13, sn14, sn15, sn16, sn17, sn18, sn19, sn20, sn21, sn22, sn23, sn24, sn25, sn26, sn27, sn28, sn29, sn30);


                            if (string.IsNullOrWhiteSpace(snList.Item1))
                            {
                                AppendLog("错误: 请至少输入一个产品码");
                                _s7net.Write("DB4010.14.0", true);
                                _s7net.Write("DB4010.0.7", true);
                                return;
                            }

                            // 检查是否有重复的SN码
                            var duplicateResult = CheckDuplicateSN(sn1, sn2, sn3, sn4, sn5, sn6, sn7, sn8, sn9, sn10, sn11, sn12, sn13, sn14, sn15, sn16, sn17, sn18, sn19, sn20, sn21, sn22, sn23, sn24, sn25, sn26, sn27, sn28, sn29, sn30);
                            if (duplicateResult.HasDuplicates)
                            {
                                AppendLog($"错误: 检测到重复的SN码 - {duplicateResult.DuplicateMessage}");
                                _s7net.Write("DB4010.14.0", true);
                                _s7net.Write("DB4010.0.7", true);
                                return;
                            }

                            //生成箱标签
                            var boxCode = await GenerateBoxLabelsAsync(snList.Item2);

                            //打印条码
                            await PrintLabelAsync(boxCode.Item1, snList.Item2, boxCode.Item2, boxCode.Item3,boxCode.Item4);

                            //上传数据
                            var requestData = new ReqDto
                            {
                                snList = snList.Item1,
                                innerBox = boxCode.Item1,
                                resource = "Resource1",
                                stationCode = "ST001",
                                workOrderCode = "WO123456"
                            };

                            RespDto response = await UploadPackingAsync(requestData);

                            if (response.code == 0)
                            {
                                AppendLog("上传包装数据成功");
                                _s7net.Write("DB4010.14.0", true);
                                _s7net.Write("DB4010.0.6", true);

                            }
                            else
                            {
                                AppendLog($"上传包装数据失败: {response.message}");
                                _s7net.Write("DB4010.14.0", true);
                                _s7net.Write("DB4010.0.7", true);
                            }
                        }
                        else if (!req && respon)
                        {
                            _s7net.Write("DB4010.14.0", false);
                            _s7net.Write("DB4010.0.6", false);
                            _s7net.Write("DB4010.0.7", false);
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


        /// <summary>
        /// 打印标签
        /// </summary>
        /// <param name="codeCache"></param>
        /// <returns></returns>
        private async Task PrintLabelAsync(string boxCode, int count, string productNum, string createTime, string serialNum)
        {
            if (boxCode == null)
            {
                AppendLog("错误: 型号名称为空");
                return;
            }

            _isPrinting = true;
            try
            {
                UpdateLabelText(boxCode);
                AppendLog($"开始打印 {boxCode}");

                var result = await Task.Run(() =>
                {
                    using (var engine = new Engine())
                    {
                        engine.Start();
                        var format = engine.Documents.Open(_selectedLabelPath); // 使用用户选择的路径
                        try
                        {
                            format.SubStrings["Boxcode"].Value = boxCode;
                            format.SubStrings["Count"].Value = count.ToString();
                            format.SubStrings["SerialNum"].Value = serialNum;
                            format.SubStrings["ProductNum"].Value = productNum;
                            format.SubStrings["CreateTime"].Value = createTime;
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

                AppendLog($"打印结果:{boxCode}， {result}");
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

        /// <summary>
        /// 手动触发打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Manual_rigger_Click(object sender, EventArgs e)
        {
            try
            {
                string boxcode = txtManualSN.Text.Trim();
                if (string.IsNullOrWhiteSpace(boxcode))
                {
                    AppendLog("错误: 请输入barcode码");
                    return;
                }

                AppendLog($"开始手动打印，barcode码: {boxcode}");
                var count = boxCount.Text.Trim();

                var result = await Task.Run(() =>
                {
                    Engine engine = null;
                    LabelFormatDocument format = null;
                    try
                    {
                        engine = new Engine();
                        engine.Start();
                        format = engine.Documents.Open(_selectedLabelPath);
                        format.SubStrings["Boxcode"].Value = boxcode;  // 使用手动输入的SN码
                        format.SubStrings["ProductNum"].Value = productCode.Text.Trim();
                        format.SubStrings["CreateTime"].Value = DateTime.Now.ToString("yyyyMMdd");
                        format.SubStrings["SerialNum"].Value = serialNum.Text.Trim();
                        format.SubStrings["Count"].Value = count;
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

        #region UI更新方法
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
            // 显示确认关闭对话框
            var result = MessageBox.Show(this, "确定要关闭程序吗?", "确认关闭", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                // 如果用户点击"否"，则阻止窗口关闭
                e.Cancel = true;
                return;
            }

            // 用户确认关闭，执行清理操作
            AppendLog("正在关闭窗体，释放资源...");
            _monitoringTokenSource?.Cancel();
            _s7net?.Dispose();
            base.OnFormClosing(e);
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

        #endregion

        #region 标签选择与PLC配置
        private string _selectedLabelPath = @"E:\\home\\a.btw";

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

        #endregion


        /// <summary> 
        /// 上传包装数据 
        /// </summary> 
        /// <param name="requestData">请求数据</param> 
        /// <returns>响应结果</returns> 
        private RespDto UploadPacking(ReqDto requestData)
        {
            try
            {
                string baseUrl = "http://localhost:5000/api/CommonInterfase/UploadPacking";
                string jsonData = JsonConvert.SerializeObject(requestData);

                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
                    string response = client.UploadString(baseUrl, "POST", jsonData);
                    return JsonConvert.DeserializeObject<RespDto>(response);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"UploadPacking接口调用失败: {ex.Message}");
                return new RespDto { code = -1, message = ex.Message };
            }
        }

        /// <summary> 
        /// 上传包装数据 
        /// </summary> 
        /// <param name="requestData">请求数据</param> 
        /// <returns>响应结果</returns> 
        private async Task<RespDto> UploadPackingAsync(ReqDto requestData)
        {
            try
            {
                string baseUrl = "http://localhost:5000/api/CommonInterfase/UploadPacking";
                string jsonData = JsonConvert.SerializeObject(requestData);

                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";

                    string response = await Task.Run(() => client.UploadString(baseUrl, "POST", jsonData));
                    return JsonConvert.DeserializeObject<RespDto>(response);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"UploadPacking接口调用失败: {ex.Message}");
                return new RespDto { code = -1, message = ex.Message };
            }
        }

        #region 标签生成方法
        /// <summary>
        ///  生成箱标签
        /// </summary>
        /// <param name="count"></param>
        /// <returns>itme1(boxCode),itme3(productcode),itme3(creteTime),itme4(serialNum)</returns>
        private async Task<(string, string, string, string)> GenerateBoxLabelsAsync(int count)
        {
            return await Task.Run(() =>
            {

                var productcode = productCode.Text.Trim();
                var creteTime = DateTime.Now.ToString("yyyyMMdd");
                var serialNum = GenerateSerialNumAsyc();
                var boxCode = productcode + creteTime + serialNum;
                return (boxCode, productcode, creteTime, serialNum);
            });
        }

        /// <summary>
        /// 生成流水号
        /// </summary>
        /// <returns></returns>
        private string GenerateSerialNumAsyc()
        {
            // 检查日期是否需要重置
            if (DateTime.Today != _currentDate)
            {
                _currentDate = DateTime.Today;
                _serialNumber = 1;
            }

            var serial = serialNum.Text.Trim();
            if (string.IsNullOrWhiteSpace(serial))
            {
                AppendLog("错误: 流水号为空");
                return string.Empty;
            }

            try
            {
                // 如果是当天第一次使用，从输入值开始
                if (_serialNumber == 1)
                {
                    // 尝试解析输入的流水号
                    if (int.TryParse(serial, out int startSerial))
                    {
                        _serialNumber = startSerial;
                    }
                    else
                    {
                        AppendLog("错误: 流水号格式不正确，应为数字");
                        return string.Empty;
                    }
                }

                // 生成当前流水号
                string currentSerial = _serialNumber.ToString("D4");

                // 递增流水号用于下次调用
                _serialNumber++;

                // 更新UI中的起始值为下一个流水号
                UpdateLastsFourInUI(_serialNumber.ToString());

                return currentSerial;
            }
            catch (Exception ex)
            {
                AppendLog($"生成流水号错误: {ex.Message}");
                return string.Empty;
            }
        }

        // 更新UI中的末四位起始值
        private void UpdateLastsFourInUI(string serial)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => serialNum.Text = serial));
            }
            else
            {
                serialNum.Text = serial;
            }
        }

        #endregion

        /// <summary>
        /// 检查SN码是否有重复
        /// </summary>
        /// <param name="sn1">SN1</param>
        /// <param name="sn2">SN2</param>
        /// <param name="sn3">SN3</param>
        /// <param name="sn4">SN4</param>
        /// <param name="sn5">SN5</param>
        /// <param name="sn6">SN6</param>
        /// <param name="sn7">SN7</param>
        /// <param name="sn8">SN8</param>
        /// <param name="sn9">SN9</param>
        /// <param name="sn10">SN10</param>
        /// <param name="sn11">SN11</param>
        /// <param name="sn12">SN12</param>
        /// <returns>去重检查结果</returns>
        /// <summary>
        /// 检查SN码是否重复（支持30个条码）
        /// </summary>
        /// <param name="snValues">SN码数组</param>
        /// <returns>去重检查结果</returns>
        private DuplicateCheckResult CheckDuplicateSN(params string[] snValues)
        {
            // 收集所有非空的SN码及其位置
            var snWithIndex = new List<(string sn, int index)>();

            for (int i = 0; i < snValues.Length; i++)
            {
                var sn = snValues[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(sn))
                {
                    snWithIndex.Add((sn, i + 1)); // 位置从1开始（SN1, SN2...）
                }
            }

            // 检查重复
            var duplicateGroups = snWithIndex
                .GroupBy(x => x.sn)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicateGroups.Any())
            {
                var duplicateMessages = new List<string>();
                foreach (var group in duplicateGroups)
                {
                    var positions = string.Join("、", group.Select(x => $"SN{x.index}"));
                    duplicateMessages.Add($"SN码 \"{group.Key}\" 在 {positions} 中重复");
                }

                return new DuplicateCheckResult
                {
                    HasDuplicates = true,
                    DuplicateMessage = string.Join("\n", duplicateMessages)
                };
            }

            return new DuplicateCheckResult
            {
                HasDuplicates = false,
                DuplicateMessage = string.Empty
            };
        }

        /// <summary>
        /// 去重检查结果
        /// </summary>
        private class DuplicateCheckResult
        {
            public bool HasDuplicates { get; set; }
            public string DuplicateMessage { get; set; }
        }

        /// <summary>
        /// 从sn1-sn30中构建SN列表，过滤空值并用逗号连接
        /// </summary>
        /// <param name="snValues">SN值数组</param>
        /// <returns>非空SN值用逗号连接的字符串</returns>
        private (string, int) BuildSnList(params string[] snValues)
        {
            var snList = new List<string>();

            // 添加非空的SN值
            foreach (var sn in snValues)
            {
                if (!string.IsNullOrEmpty(sn?.Trim()))
                {
                    snList.Add(sn.Trim());
                }
            }

            return (string.Join(",", snList), snList.Count);
        }

        private void UploadAndPrint_Click(object sender, EventArgs e)
        {
            // 获取所有30个SN码
            var snValues = new string[30];
            for (int i = 1; i <= 30; i++)
            {
                string controlName = $"sncode{i}";
                var textBox = this.GetType().GetField(controlName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(this) as TextBox;

                snValues[i - 1] = textBox?.Text?.Trim() ?? string.Empty;
            }

            Task.Run(async () =>
            {
                
                var snList = BuildSnList(snValues);
               
                if (string.IsNullOrWhiteSpace(snList.Item1))
                {
                    AppendLog("错误: 请至少输入一个产品码");
                    MessageBox.Show("请至少输入一个产品码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查是否有重复的SN码
                var duplicateResult = CheckDuplicateSN(snValues);
                if (duplicateResult.HasDuplicates)
                {
                    AppendLog($"错误: 检测到重复的SN码 - {duplicateResult.DuplicateMessage}");
                    MessageBox.Show($"检测到重复的SN码：\n{duplicateResult.DuplicateMessage}\n\n请修正后重试！", "重复SN码警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //生成箱标签
                var boxCode = await GenerateBoxLabelsAsync(snList.Item2);
                AppendLog($"生成箱标签:{boxCode},件数：{snList.Item2}");

                //打印条码
                await PrintLabelAsync(boxCode.Item1, snList.Item2, boxCode.Item2, boxCode.Item3, boxCode.Item4);
                AppendLog($"打印条码:{boxCode}");

                //上传数据
                var requestData = new ReqDto
                {
                    snList = snList.Item1,
                    innerBox = boxCode.Item1,
                    resource = "Resource1",
                    stationCode = "ST001",
                    workOrderCode = "WO123456"
                };
                RespDto response = await UploadPackingAsync(requestData);
                if (response.code == 0)
                {
                    AppendLog("上传包装数据成功");

                }
                else
                {
                    AppendLog($"上传包装数据失败: {response.message}");
                }
            });
        }

        /// <summary>
        /// 清空输入框产品码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearingAll_Click(object sender, EventArgs e)
        {
            // 清空所有30个SN码文本框内容
            for (int i = 1; i <= 30; i++)
            {
                string controlName = $"sncode{i}";
                var textBox = this.GetType().GetField(controlName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(this) as TextBox;

                if (textBox != null)
                {
                    textBox.Clear();
                }
            }

            // 将焦点设置到第一个文本框
            sncode1.Focus();

            AppendLog("已清空所有SN码输入框");
        }
    }
}