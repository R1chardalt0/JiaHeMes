namespace FJY_Print
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtManualSN = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSelectLabelFile = new System.Windows.Forms.Button();
            this.lblSelectedFile = new System.Windows.Forms.Label();
            this.btnPLCConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(46, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "触发打印";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(46, 95);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(1047, 440);
            this.textBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(178, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 16);
            this.label1.TabIndex = 2;
            // 
            // txtManualSN
            // 
            this.txtManualSN.Location = new System.Drawing.Point(855, 41);
            this.txtManualSN.Name = "txtManualSN";
            this.txtManualSN.Size = new System.Drawing.Size(238, 21);
            this.txtManualSN.TabIndex = 3;
            this.txtManualSN.Text = "SN123456789";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(814, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "SN码:";
            // 
            // btnSelectLabelFile
            // 
            this.btnSelectLabelFile.Location = new System.Drawing.Point(828, 553);
            this.btnSelectLabelFile.Name = "btnSelectLabelFile";
            this.btnSelectLabelFile.Size = new System.Drawing.Size(100, 23);
            this.btnSelectLabelFile.TabIndex = 5;
            this.btnSelectLabelFile.Text = "选择标签文件";
            this.btnSelectLabelFile.UseVisualStyleBackColor = true;
            this.btnSelectLabelFile.Click += new System.EventHandler(this.btnSelectLabelFile_Click);
            // 
            // lblSelectedFile
            // 
            this.lblSelectedFile.AutoEllipsis = true;
            this.lblSelectedFile.AutoSize = true;
            this.lblSelectedFile.Location = new System.Drawing.Point(46, 553);
            this.lblSelectedFile.MaximumSize = new System.Drawing.Size(580, 0);
            this.lblSelectedFile.Name = "lblSelectedFile";
            this.lblSelectedFile.Size = new System.Drawing.Size(0, 12);
            this.lblSelectedFile.TabIndex = 6;
            // 
            // btnPLCConfig
            // 
            this.btnPLCConfig.Location = new System.Drawing.Point(974, 553);
            this.btnPLCConfig.Name = "btnPLCConfig";
            this.btnPLCConfig.Size = new System.Drawing.Size(100, 23);
            this.btnPLCConfig.TabIndex = 7;
            this.btnPLCConfig.Text = "配置PLC参数";
            this.btnPLCConfig.UseVisualStyleBackColor = true;
            this.btnPLCConfig.Click += new System.EventHandler(this.btnPLCConfig_Click);
            // 
            // Main
            // 
            this.ClientSize = new System.Drawing.Size(1133, 588);
            this.Controls.Add(this.btnPLCConfig);
            this.Controls.Add(this.lblSelectedFile);
            this.Controls.Add(this.btnSelectLabelFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtManualSN);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Main";
            this.Text = "Print";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtManualSN;
        private System.Windows.Forms.Label label2;
        // 新增控件声明
        private System.Windows.Forms.Button btnSelectLabelFile;
        private System.Windows.Forms.Label lblSelectedFile;
        private System.Windows.Forms.Button btnPLCConfig;
}}