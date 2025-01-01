namespace ss2409_01
{
    partial class Form1
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
            this.cameraComboBox1 = new System.Windows.Forms.ComboBox();
            this.cameraButton1 = new System.Windows.Forms.Button();
            this.statusLabel1 = new System.Windows.Forms.Label();
            this.selectLabel = new System.Windows.Forms.Label();
            this.cameraPictureBox1 = new System.Windows.Forms.PictureBox();
            this.calibrationButton = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.cameraButton2 = new System.Windows.Forms.Button();
            this.mmTextBox = new System.Windows.Forms.TextBox();
            this.mmLabel = new System.Windows.Forms.Label();
            this.measureLabel = new System.Windows.Forms.Label();
            this.measureButton = new System.Windows.Forms.Button();
            this.statusLabel2 = new System.Windows.Forms.Label();
            this.cameraPictureBox2 = new System.Windows.Forms.PictureBox();
            this.selectLabel2 = new System.Windows.Forms.Label();
            this.loadButton = new System.Windows.Forms.Button();
            this.cameraComboBox2 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox1)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // cameraComboBox1
            // 
            this.cameraComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraComboBox1.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cameraComboBox1.FormattingEnabled = true;
            this.cameraComboBox1.Location = new System.Drawing.Point(753, 77);
            this.cameraComboBox1.Name = "cameraComboBox1";
            this.cameraComboBox1.Size = new System.Drawing.Size(313, 35);
            this.cameraComboBox1.TabIndex = 0;
            // 
            // cameraButton1
            // 
            this.cameraButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraButton1.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cameraButton1.Location = new System.Drawing.Point(753, 136);
            this.cameraButton1.Name = "cameraButton1";
            this.cameraButton1.Size = new System.Drawing.Size(313, 46);
            this.cameraButton1.TabIndex = 1;
            this.cameraButton1.Text = "カメラ接続";
            this.cameraButton1.UseVisualStyleBackColor = true;
            this.cameraButton1.Click += new System.EventHandler(this.CameraButton1_Click);
            // 
            // statusLabel1
            // 
            this.statusLabel1.AutoSize = true;
            this.statusLabel1.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.statusLabel1.Location = new System.Drawing.Point(3, 3);
            this.statusLabel1.Name = "statusLabel1";
            this.statusLabel1.Size = new System.Drawing.Size(296, 27);
            this.statusLabel1.TabIndex = 10;
            this.statusLabel1.Text = "カメラが接続されていません";
            // 
            // selectLabel
            // 
            this.selectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectLabel.AutoSize = true;
            this.selectLabel.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.selectLabel.Location = new System.Drawing.Point(773, 47);
            this.selectLabel.Name = "selectLabel";
            this.selectLabel.Size = new System.Drawing.Size(274, 27);
            this.selectLabel.TabIndex = 3;
            this.selectLabel.Text = "カメラを選択してください";
            // 
            // cameraPictureBox1
            // 
            this.cameraPictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraPictureBox1.Location = new System.Drawing.Point(8, 33);
            this.cameraPictureBox1.Name = "cameraPictureBox1";
            this.cameraPictureBox1.Size = new System.Drawing.Size(730, 456);
            this.cameraPictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cameraPictureBox1.TabIndex = 4;
            this.cameraPictureBox1.TabStop = false;
            // 
            // calibrationButton
            // 
            this.calibrationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.calibrationButton.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.calibrationButton.Location = new System.Drawing.Point(753, 299);
            this.calibrationButton.Name = "calibrationButton";
            this.calibrationButton.Size = new System.Drawing.Size(313, 46);
            this.calibrationButton.TabIndex = 11;
            this.calibrationButton.Text = "キャリブレーション";
            this.calibrationButton.UseVisualStyleBackColor = true;
            this.calibrationButton.Click += new System.EventHandler(this.CalibrationButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tabControl.Location = new System.Drawing.Point(12, 5);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1080, 540);
            this.tabControl.TabIndex = 12;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.statusLabel1);
            this.tabPage1.Controls.Add(this.calibrationButton);
            this.tabPage1.Controls.Add(this.cameraPictureBox1);
            this.tabPage1.Controls.Add(this.selectLabel);
            this.tabPage1.Controls.Add(this.cameraButton1);
            this.tabPage1.Controls.Add(this.cameraComboBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 36);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1072, 500);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "キャリブレーション";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.cameraButton2);
            this.tabPage2.Controls.Add(this.mmTextBox);
            this.tabPage2.Controls.Add(this.mmLabel);
            this.tabPage2.Controls.Add(this.measureLabel);
            this.tabPage2.Controls.Add(this.measureButton);
            this.tabPage2.Controls.Add(this.statusLabel2);
            this.tabPage2.Controls.Add(this.cameraPictureBox2);
            this.tabPage2.Controls.Add(this.selectLabel2);
            this.tabPage2.Controls.Add(this.loadButton);
            this.tabPage2.Controls.Add(this.cameraComboBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 36);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1072, 500);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "計測";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // cameraButton2
            // 
            this.cameraButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraButton2.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cameraButton2.Location = new System.Drawing.Point(753, 188);
            this.cameraButton2.Name = "cameraButton2";
            this.cameraButton2.Size = new System.Drawing.Size(313, 46);
            this.cameraButton2.TabIndex = 23;
            this.cameraButton2.Text = "カメラ接続";
            this.cameraButton2.UseVisualStyleBackColor = true;
            this.cameraButton2.Click += new System.EventHandler(this.CameraButton2_Click);
            // 
            // mmTextBox
            // 
            this.mmTextBox.Location = new System.Drawing.Point(876, 437);
            this.mmTextBox.Name = "mmTextBox";
            this.mmTextBox.Size = new System.Drawing.Size(139, 34);
            this.mmTextBox.TabIndex = 22;
            // 
            // mmLabel
            // 
            this.mmLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mmLabel.AutoSize = true;
            this.mmLabel.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.mmLabel.Location = new System.Drawing.Point(1020, 444);
            this.mmLabel.Name = "mmLabel";
            this.mmLabel.Size = new System.Drawing.Size(46, 27);
            this.mmLabel.TabIndex = 21;
            this.mmLabel.Text = "mm";
            // 
            // measureLabel
            // 
            this.measureLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.measureLabel.AutoSize = true;
            this.measureLabel.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.measureLabel.Location = new System.Drawing.Point(758, 444);
            this.measureLabel.Name = "measureLabel";
            this.measureLabel.Size = new System.Drawing.Size(96, 27);
            this.measureLabel.TabIndex = 20;
            this.measureLabel.Text = "計測距離";
            // 
            // measureButton
            // 
            this.measureButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.measureButton.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.measureButton.Location = new System.Drawing.Point(753, 378);
            this.measureButton.Name = "measureButton";
            this.measureButton.Size = new System.Drawing.Size(313, 46);
            this.measureButton.TabIndex = 19;
            this.measureButton.Text = "計測";
            this.measureButton.UseVisualStyleBackColor = true;
            this.measureButton.Click += new System.EventHandler(this.MeasureButton_Click);
            // 
            // statusLabel2
            // 
            this.statusLabel2.AutoSize = true;
            this.statusLabel2.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.statusLabel2.Location = new System.Drawing.Point(3, 3);
            this.statusLabel2.Name = "statusLabel2";
            this.statusLabel2.Size = new System.Drawing.Size(296, 27);
            this.statusLabel2.TabIndex = 17;
            this.statusLabel2.Text = "カメラが接続されていません";
            // 
            // cameraPictureBox2
            // 
            this.cameraPictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraPictureBox2.Location = new System.Drawing.Point(8, 33);
            this.cameraPictureBox2.Name = "cameraPictureBox2";
            this.cameraPictureBox2.Size = new System.Drawing.Size(730, 456);
            this.cameraPictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cameraPictureBox2.TabIndex = 16;
            this.cameraPictureBox2.TabStop = false;
            // 
            // selectLabel2
            // 
            this.selectLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectLabel2.AutoSize = true;
            this.selectLabel2.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.selectLabel2.Location = new System.Drawing.Point(773, 47);
            this.selectLabel2.Name = "selectLabel2";
            this.selectLabel2.Size = new System.Drawing.Size(274, 27);
            this.selectLabel2.TabIndex = 15;
            this.selectLabel2.Text = "カメラを選択してください";
            // 
            // loadButton
            // 
            this.loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.loadButton.Font = new System.Drawing.Font("07やさしさゴシック", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.loadButton.Location = new System.Drawing.Point(753, 127);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(313, 46);
            this.loadButton.TabIndex = 14;
            this.loadButton.Text = "キャリブレーションデータ読込";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.LoadCalibrationDataButton_Click);
            // 
            // cameraComboBox2
            // 
            this.cameraComboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraComboBox2.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cameraComboBox2.FormattingEnabled = true;
            this.cameraComboBox2.Location = new System.Drawing.Point(753, 77);
            this.cameraComboBox2.Name = "cameraComboBox2";
            this.cameraComboBox2.Size = new System.Drawing.Size(313, 35);
            this.cameraComboBox2.TabIndex = 13;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1104, 550);
            this.Controls.Add(this.tabControl);
            this.Name = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox1)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox2)).EndInit();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.ComboBox cameraComboBox1;
        private System.Windows.Forms.Button cameraButton1;
        private System.Windows.Forms.Label statusLabel1;
        private System.Windows.Forms.Label selectLabel;
        private System.Windows.Forms.Button calibrationButton;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label statusLabel2;
        private System.Windows.Forms.Label selectLabel2;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.ComboBox cameraComboBox2;
        private System.Windows.Forms.Button measureButton;
        private System.Windows.Forms.Label measureLabel;
        private System.Windows.Forms.Label mmLabel;
        private System.Windows.Forms.TextBox mmTextBox;
        private System.Windows.Forms.Button cameraButton2;
        public System.Windows.Forms.PictureBox cameraPictureBox1;
        public System.Windows.Forms.PictureBox cameraPictureBox2;
    }
}

