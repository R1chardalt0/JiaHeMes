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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.UploadAndPrint = new System.Windows.Forms.Button();
            this.sncode12 = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.sncode11 = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.sncode10 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.sncode9 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.sncode8 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.sncode7 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.sncode6 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.sncode5 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.sncode4 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.sncode3 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.sncode2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.sncode1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ClearingAll = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1018, 41);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "触发打印";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Manual_rigger_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(46, 95);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(1047, 579);
            this.textBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(46, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 16);
            this.label1.TabIndex = 2;
            // 
            // txtManualSN
            // 
            this.txtManualSN.Location = new System.Drawing.Point(750, 41);
            this.txtManualSN.Name = "txtManualSN";
            this.txtManualSN.Size = new System.Drawing.Size(238, 21);
            this.txtManualSN.TabIndex = 3;
            this.txtManualSN.Text = "Box123456789";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(697, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "箱体码:";
            // 
            // btnSelectLabelFile
            // 
            this.btnSelectLabelFile.Location = new System.Drawing.Point(46, 696);
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
            this.lblSelectedFile.Location = new System.Drawing.Point(184, 701);
            this.lblSelectedFile.MaximumSize = new System.Drawing.Size(580, 0);
            this.lblSelectedFile.Name = "lblSelectedFile";
            this.lblSelectedFile.Size = new System.Drawing.Size(0, 12);
            this.lblSelectedFile.TabIndex = 6;
            // 
            // btnPLCConfig
            // 
            this.btnPLCConfig.Location = new System.Drawing.Point(1459, 713);
            this.btnPLCConfig.Name = "btnPLCConfig";
            this.btnPLCConfig.Size = new System.Drawing.Size(100, 23);
            this.btnPLCConfig.TabIndex = 7;
            this.btnPLCConfig.Text = "配置PLC参数";
            this.btnPLCConfig.UseVisualStyleBackColor = true;
            this.btnPLCConfig.Click += new System.EventHandler(this.btnPLCConfig_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ClearingAll);
            this.groupBox1.Controls.Add(this.UploadAndPrint);
            this.groupBox1.Controls.Add(this.sncode12);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.sncode11);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.sncode10);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.sncode9);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.sncode8);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.sncode7);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.sncode6);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.sncode5);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.sncode4);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.sncode3);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.sncode2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.sncode1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(1133, 269);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(425, 405);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "手动入箱";
            // 
            // UploadAndPrint
            // 
            this.UploadAndPrint.Location = new System.Drawing.Point(238, 355);
            this.UploadAndPrint.Name = "UploadAndPrint";
            this.UploadAndPrint.Size = new System.Drawing.Size(137, 41);
            this.UploadAndPrint.TabIndex = 24;
            this.UploadAndPrint.Text = "上传数据并触发打印";
            this.UploadAndPrint.UseVisualStyleBackColor = true;
            this.UploadAndPrint.Click += new System.EventHandler(this.UploadAndPrint_Click);
            // 
            // sncode12
            // 
            this.sncode12.Location = new System.Drawing.Point(86, 316);
            this.sncode12.Name = "sncode12";
            this.sncode12.Size = new System.Drawing.Size(289, 21);
            this.sncode12.TabIndex = 23;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(47, 322);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 12);
            this.label14.TabIndex = 22;
            this.label14.Text = "SN12:";
            // 
            // sncode11
            // 
            this.sncode11.Location = new System.Drawing.Point(86, 289);
            this.sncode11.Name = "sncode11";
            this.sncode11.Size = new System.Drawing.Size(289, 21);
            this.sncode11.TabIndex = 21;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(47, 295);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(35, 12);
            this.label13.TabIndex = 20;
            this.label13.Text = "SN11:";
            // 
            // sncode10
            // 
            this.sncode10.Location = new System.Drawing.Point(86, 262);
            this.sncode10.Name = "sncode10";
            this.sncode10.Size = new System.Drawing.Size(289, 21);
            this.sncode10.TabIndex = 19;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(47, 268);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 12);
            this.label12.TabIndex = 18;
            this.label12.Text = "SN10:";
            // 
            // sncode9
            // 
            this.sncode9.Location = new System.Drawing.Point(86, 235);
            this.sncode9.Name = "sncode9";
            this.sncode9.Size = new System.Drawing.Size(289, 21);
            this.sncode9.TabIndex = 17;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(47, 241);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 12);
            this.label11.TabIndex = 16;
            this.label11.Text = "SN9:";
            // 
            // sncode8
            // 
            this.sncode8.Location = new System.Drawing.Point(86, 208);
            this.sncode8.Name = "sncode8";
            this.sncode8.Size = new System.Drawing.Size(289, 21);
            this.sncode8.TabIndex = 15;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(47, 214);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 12);
            this.label10.TabIndex = 14;
            this.label10.Text = "SN8:";
            // 
            // sncode7
            // 
            this.sncode7.Location = new System.Drawing.Point(86, 181);
            this.sncode7.Name = "sncode7";
            this.sncode7.Size = new System.Drawing.Size(289, 21);
            this.sncode7.TabIndex = 13;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(47, 187);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 12;
            this.label9.Text = "SN7:";
            // 
            // sncode6
            // 
            this.sncode6.Location = new System.Drawing.Point(86, 154);
            this.sncode6.Name = "sncode6";
            this.sncode6.Size = new System.Drawing.Size(289, 21);
            this.sncode6.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(47, 160);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "SN6:";
            // 
            // sncode5
            // 
            this.sncode5.Location = new System.Drawing.Point(86, 127);
            this.sncode5.Name = "sncode5";
            this.sncode5.Size = new System.Drawing.Size(289, 21);
            this.sncode5.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 133);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 8;
            this.label7.Text = "SN5:";
            // 
            // sncode4
            // 
            this.sncode4.Location = new System.Drawing.Point(86, 100);
            this.sncode4.Name = "sncode4";
            this.sncode4.Size = new System.Drawing.Size(289, 21);
            this.sncode4.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(47, 106);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "SN4:";
            // 
            // sncode3
            // 
            this.sncode3.Location = new System.Drawing.Point(86, 73);
            this.sncode3.Name = "sncode3";
            this.sncode3.Size = new System.Drawing.Size(289, 21);
            this.sncode3.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "SN3:";
            // 
            // sncode2
            // 
            this.sncode2.Location = new System.Drawing.Point(86, 46);
            this.sncode2.Name = "sncode2";
            this.sncode2.Size = new System.Drawing.Size(289, 21);
            this.sncode2.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "SN2:";
            // 
            // sncode1
            // 
            this.sncode1.Location = new System.Drawing.Point(86, 19);
            this.sncode1.Name = "sncode1";
            this.sncode1.Size = new System.Drawing.Size(289, 21);
            this.sncode1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "SN1:";
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(1133, 95);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(425, 160);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "箱体码规则";
            // 
            // ClearingAll
            // 
            this.ClearingAll.Location = new System.Drawing.Point(86, 355);
            this.ClearingAll.Name = "ClearingAll";
            this.ClearingAll.Size = new System.Drawing.Size(81, 41);
            this.ClearingAll.TabIndex = 25;
            this.ClearingAll.Text = "一键清空";
            this.ClearingAll.UseVisualStyleBackColor = true;
            this.ClearingAll.Click += new System.EventHandler(this.ClearingAll_Click);
            // 
            // Main
            // 
            this.ClientSize = new System.Drawing.Size(1590, 748);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
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
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox sncode1;
        private System.Windows.Forms.TextBox sncode12;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox sncode11;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox sncode10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox sncode9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox sncode8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox sncode7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox sncode6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox sncode5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox sncode4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox sncode3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox sncode2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button UploadAndPrint;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ClearingAll;
    }
}