namespace CameraApp
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageCalib = new System.Windows.Forms.TabPage();
            this.picBoxCalib = new System.Windows.Forms.PictureBox();
            this.btnSaveData = new System.Windows.Forms.Button();
            this.btnStartCalib = new System.Windows.Forms.Button();
            this.txtCapInterval = new System.Windows.Forms.TextBox();
            this.txtTargetCapCount = new System.Windows.Forms.TextBox();
            this.lblCapInterval = new System.Windows.Forms.Label();
            this.lblCapCount = new System.Windows.Forms.Label();
            this.tabPageMeasure = new System.Windows.Forms.TabPage();
            this.picBoxMeasure = new System.Windows.Forms.PictureBox();
            this.btnMeasure = new System.Windows.Forms.Button();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.txtMeasureDist = new System.Windows.Forms.TextBox();
            this.lblMeasureDist = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabPageCalib.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCalib)).BeginInit();
            this.tabPageMeasure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxMeasure)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageCalib);
            this.tabControl.Controls.Add(this.tabPageMeasure);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(695, 440);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageCalib
            // 
            this.tabPageCalib.Controls.Add(this.picBoxCalib);
            this.tabPageCalib.Controls.Add(this.btnSaveData);
            this.tabPageCalib.Controls.Add(this.btnStartCalib);
            this.tabPageCalib.Controls.Add(this.txtCapInterval);
            this.tabPageCalib.Controls.Add(this.txtTargetCapCount);
            this.tabPageCalib.Controls.Add(this.lblCapInterval);
            this.tabPageCalib.Controls.Add(this.lblCapCount);
            this.tabPageCalib.Location = new System.Drawing.Point(4, 22);
            this.tabPageCalib.Name = "tabPageCalib";
            this.tabPageCalib.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCalib.Size = new System.Drawing.Size(687, 414);
            this.tabPageCalib.TabIndex = 0;
            this.tabPageCalib.Text = "キャリブレーション";
            this.tabPageCalib.UseVisualStyleBackColor = true;
            // 
            // picBoxCalib
            // 
            this.picBoxCalib.Location = new System.Drawing.Point(7, 21);
            this.picBoxCalib.Name = "picBoxCalib";
            this.picBoxCalib.Size = new System.Drawing.Size(527, 392);
            this.picBoxCalib.TabIndex = 6;
            this.picBoxCalib.TabStop = false;
            // 
            // btnSaveData
            // 
            this.btnSaveData.Location = new System.Drawing.Point(564, 317);
            this.btnSaveData.Name = "btnSaveData";
            this.btnSaveData.Size = new System.Drawing.Size(75, 23);
            this.btnSaveData.TabIndex = 5;
            this.btnSaveData.Text = "データ保存";
            this.btnSaveData.UseVisualStyleBackColor = true;
            this.btnSaveData.Click += new System.EventHandler(this.BtnSaveData_Click);
            // 
            // btnStartCalib
            // 
            this.btnStartCalib.Location = new System.Drawing.Point(564, 270);
            this.btnStartCalib.Name = "btnStartCalib";
            this.btnStartCalib.Size = new System.Drawing.Size(75, 23);
            this.btnStartCalib.TabIndex = 4;
            this.btnStartCalib.Text = "開始";
            this.btnStartCalib.UseVisualStyleBackColor = true;
            this.btnStartCalib.Click += new System.EventHandler(this.BtnStartCalib_Click);
            // 
            // txtCapInterval
            // 
            this.txtCapInterval.Location = new System.Drawing.Point(581, 202);
            this.txtCapInterval.Name = "txtCapInterval";
            this.txtCapInterval.Size = new System.Drawing.Size(100, 19);
            this.txtCapInterval.TabIndex = 3;
            this.txtCapInterval.TextChanged += new System.EventHandler(this.txtCapInterval_TextChanged);
            // 
            // txtTargetCapCount
            // 
            this.txtTargetCapCount.Location = new System.Drawing.Point(581, 116);
            this.txtTargetCapCount.Name = "txtTargetCapCount";
            this.txtTargetCapCount.Size = new System.Drawing.Size(100, 19);
            this.txtTargetCapCount.TabIndex = 2;
            // 
            // lblCapInterval
            // 
            this.lblCapInterval.AutoSize = true;
            this.lblCapInterval.Location = new System.Drawing.Point(607, 163);
            this.lblCapInterval.Name = "lblCapInterval";
            this.lblCapInterval.Size = new System.Drawing.Size(53, 12);
            this.lblCapInterval.TabIndex = 1;
            this.lblCapInterval.Text = "撮影間隔";
            // 
            // lblCapCount
            // 
            this.lblCapCount.AutoSize = true;
            this.lblCapCount.Location = new System.Drawing.Point(586, 80);
            this.lblCapCount.Name = "lblCapCount";
            this.lblCapCount.Size = new System.Drawing.Size(53, 12);
            this.lblCapCount.TabIndex = 0;
            this.lblCapCount.Text = "撮影枚数";
            // 
            // tabPageMeasure
            // 
            this.tabPageMeasure.Controls.Add(this.picBoxMeasure);
            this.tabPageMeasure.Controls.Add(this.btnMeasure);
            this.tabPageMeasure.Controls.Add(this.btnLoadData);
            this.tabPageMeasure.Controls.Add(this.txtMeasureDist);
            this.tabPageMeasure.Controls.Add(this.lblMeasureDist);
            this.tabPageMeasure.Location = new System.Drawing.Point(4, 22);
            this.tabPageMeasure.Name = "tabPageMeasure";
            this.tabPageMeasure.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMeasure.Size = new System.Drawing.Size(687, 414);
            this.tabPageMeasure.TabIndex = 1;
            this.tabPageMeasure.Text = "計測";
            this.tabPageMeasure.UseVisualStyleBackColor = true;
            // 
            // picBoxMeasure
            // 
            this.picBoxMeasure.Location = new System.Drawing.Point(6, 17);
            this.picBoxMeasure.Name = "picBoxMeasure";
            this.picBoxMeasure.Size = new System.Drawing.Size(527, 392);
            this.picBoxMeasure.TabIndex = 11;
            this.picBoxMeasure.TabStop = false;
            // 
            // btnMeasure
            // 
            this.btnMeasure.Location = new System.Drawing.Point(563, 307);
            this.btnMeasure.Name = "btnMeasure";
            this.btnMeasure.Size = new System.Drawing.Size(75, 23);
            this.btnMeasure.TabIndex = 10;
            this.btnMeasure.Text = "計測";
            this.btnMeasure.UseVisualStyleBackColor = true;
            this.btnMeasure.Click += new System.EventHandler(this.BtnMeasure_Click);
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(563, 260);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(75, 23);
            this.btnLoadData.TabIndex = 9;
            this.btnLoadData.Text = "データ読み込み";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.Click += new System.EventHandler(this.BtnLoadData_Click);
            // 
            // txtMeasureDist
            // 
            this.txtMeasureDist.Location = new System.Drawing.Point(580, 106);
            this.txtMeasureDist.Name = "txtMeasureDist";
            this.txtMeasureDist.Size = new System.Drawing.Size(100, 19);
            this.txtMeasureDist.TabIndex = 8;
            this.txtMeasureDist.TextChanged += new System.EventHandler(this.txtMeasureDist_TextChanged);
            // 
            // lblMeasureDist
            // 
            this.lblMeasureDist.AutoSize = true;
            this.lblMeasureDist.Location = new System.Drawing.Point(585, 70);
            this.lblMeasureDist.Name = "lblMeasureDist";
            this.lblMeasureDist.Size = new System.Drawing.Size(53, 12);
            this.lblMeasureDist.TabIndex = 7;
            this.lblMeasureDist.Text = "計測距離";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(712, 452);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.tabControl.ResumeLayout(false);
            this.tabPageCalib.ResumeLayout(false);
            this.tabPageCalib.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCalib)).EndInit();
            this.tabPageMeasure.ResumeLayout(false);
            this.tabPageMeasure.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxMeasure)).EndInit();
            this.ResumeLayout(false);

        }





        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageCalib;
        private System.Windows.Forms.TabPage tabPageMeasure;
        private System.Windows.Forms.Label lblCapCount;
        private System.Windows.Forms.Label lblCapInterval;
        private System.Windows.Forms.TextBox txtTargetCapCount;
        private System.Windows.Forms.TextBox txtCapInterval;
        private System.Windows.Forms.Button btnStartCalib;
        private System.Windows.Forms.PictureBox picBoxCalib;
        private System.Windows.Forms.Button btnSaveData;
        private System.Windows.Forms.PictureBox picBoxMeasure;
        private System.Windows.Forms.Button btnMeasure;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.TextBox txtMeasureDist;
        private System.Windows.Forms.Label lblMeasureDist;
    }
}

