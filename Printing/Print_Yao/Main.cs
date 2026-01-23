using JY_Print;
using JY_Print.model;
using Newtonsoft.Json;
using NJCH_Station;
using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private bool res = false;

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
            // 为所有SN码文本框添加回车键事件处理
            sncode1.KeyDown += SNTextBox_KeyDown;
            sncode2.KeyDown += SNTextBox_KeyDown;
            sncode3.KeyDown += SNTextBox_KeyDown;
            sncode4.KeyDown += SNTextBox_KeyDown;
            sncode5.KeyDown += SNTextBox_KeyDown;
            sncode6.KeyDown += SNTextBox_KeyDown;
            sncode7.KeyDown += SNTextBox_KeyDown;
            sncode8.KeyDown += SNTextBox_KeyDown;
            sncode9.KeyDown += SNTextBox_KeyDown;
            sncode10.KeyDown += SNTextBox_KeyDown;
            sncode11.KeyDown += SNTextBox_KeyDown;
            sncode12.KeyDown += SNTextBox_KeyDown;
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
            // 根据TabIndex顺序获取下一个文本框
            // TabIndex顺序: sncode1=1, sncode2=3, sncode3=5, sncode4=7, sncode5=9, sncode6=11, 
            //                sncode7=13, sncode8=15, sncode9=17, sncode10=19, sncode11=21, sncode12=23
            
            switch (currentTextBox.Name)
            {
                case "sncode1": return sncode2;
                case "sncode2": return sncode3;
                case "sncode3": return sncode4;
                case "sncode4": return sncode5;
                case "sncode5": return sncode6;
                case "sncode6": return sncode7;
                case "sncode7": return sncode8;
                case "sncode8": return sncode9;
                case "sncode9": return sncode10;
                case "sncode10": return sncode11;
                case "sncode11": return sncode12;
                case "sncode12": return null; // 最后一个文本框，没有下一个
            }
            
            return null;
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

                            //生成箱标签
                            var boxCode = await GenerateBoxLabelsAsync();

                            //打印条码
                            await PrintLabelAsync(boxCode);

                            //上传数据
                            var snList = BuildSnList(sn1, sn2, sn3, sn4, sn5, sn6, sn7, sn8, sn9, sn10, sn11, sn12);
                            
                            var requestData = new ReqDto
                            {
                                snList = snList,
                                innerBox =boxCode,
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
        private async Task PrintLabelAsync(string boxCode)
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
                            //format.SubStrings["CN"].Value = codeCache.CustomerPartNumber;
                            //format.SubStrings["QU"].Value = codeCache.Quantity;
                            //format.SubStrings["DF"].Value = codeCache.CreateTime.ToString("yyyy/MM/dd HH:mm:ss");
                            //format.SubStrings["SH"].Value = codeCache.Shift;
                            //format.SubStrings["DS"].Value = codeCache.CreateTime.ToString("yyyyMMdd") + codeCache.SerialNumber;
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

        /// <summary>
        /// 生成箱标签
        /// </summary>
        /// <returns></returns>
        private async Task<string> GenerateBoxLabelsAsync()
        {
            return await Task.Run(() =>
            {
                // 模拟生成箱标签的过程
                Thread.Sleep(1000); // 模拟耗时操作
                return "BOX1234567890"; // 返回生成的箱标签
            });
        }

        /// <summary>
        /// 从sn1-sn12中构建SN列表，过滤空值并用逗号连接
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
        /// <returns>非空SN值用逗号连接的字符串</returns>
        private string BuildSnList(string sn1, string sn2, string sn3, string sn4, string sn5, string sn6,
            string sn7, string sn8, string sn9, string sn10, string sn11, string sn12)
        {
            var snList = new List<string>();
            
            // 添加非空的SN值
            if (!string.IsNullOrEmpty(sn1?.Trim())) snList.Add(sn1.Trim());
            if (!string.IsNullOrEmpty(sn2?.Trim())) snList.Add(sn2.Trim());
            if (!string.IsNullOrEmpty(sn3?.Trim())) snList.Add(sn3.Trim());
            if (!string.IsNullOrEmpty(sn4?.Trim())) snList.Add(sn4.Trim());
            if (!string.IsNullOrEmpty(sn5?.Trim())) snList.Add(sn5.Trim());
            if (!string.IsNullOrEmpty(sn6?.Trim())) snList.Add(sn6.Trim());
            if (!string.IsNullOrEmpty(sn7?.Trim())) snList.Add(sn7.Trim());
            if (!string.IsNullOrEmpty(sn8?.Trim())) snList.Add(sn8.Trim());
            if (!string.IsNullOrEmpty(sn9?.Trim())) snList.Add(sn9.Trim());
            if (!string.IsNullOrEmpty(sn10?.Trim())) snList.Add(sn10.Trim());
            if (!string.IsNullOrEmpty(sn11?.Trim())) snList.Add(sn11.Trim());
            if (!string.IsNullOrEmpty(sn12?.Trim())) snList.Add(sn12.Trim());
            
            return string.Join(",", snList);
        }

        private void UploadAndPrint_Click(object sender, EventArgs e)
        {
            var sn1=sncode1.Text.Trim();
            var sn2=sncode2.Text.Trim();
            var sn3=sncode3.Text.Trim();
            var sn4=sncode4.Text.Trim();
            var sn5 = sncode5.Text.Trim();
            var sn6 = sncode6.Text.Trim();
            var sn7 = sncode7.Text.Trim();
            var sn8 = sncode8.Text.Trim();
            var sn9 = sncode9.Text.Trim();
            var sn10 = sncode10.Text.Trim();
            var sn11 = sncode11.Text.Trim();
            var sn12 = sncode12.Text.Trim();
            Task.Run(async () =>
            {
                //生成箱标签
                var boxCode = await GenerateBoxLabelsAsync();
                //打印条码
                await PrintLabelAsync(boxCode);
                //上传数据
                var snList = BuildSnList(sn1, sn2, sn3, sn4, sn5, sn6, sn7, sn8, sn9, sn10, sn11, sn12);
                var requestData = new ReqDto
                {
                    snList = snList,
                    innerBox = boxCode,
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

        private void ClearingAll_Click(object sender, EventArgs e)
        {
            // 清空所有SN码文本框内容
            sncode1.Clear();
            sncode2.Clear();
            sncode3.Clear();
            sncode4.Clear();
            sncode5.Clear();
            sncode6.Clear();
            sncode7.Clear();
            sncode8.Clear();
            sncode9.Clear();
            sncode10.Clear();
            sncode11.Clear();
            sncode12.Clear();
            
            // 将焦点设置到第一个文本框
            sncode1.Focus();
            
            AppendLog("已清空所有SN码输入框");
        }
    }
}