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
            this.cameraComboBox = new System.Windows.Forms.ComboBox();
            this.cameraButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.selectLabel = new System.Windows.Forms.Label();
            this.cameraPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // cameraComboBox
            // 
            this.cameraComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraComboBox.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cameraComboBox.FormattingEnabled = true;
            this.cameraComboBox.Location = new System.Drawing.Point(531, 122);
            this.cameraComboBox.Name = "cameraComboBox";
            this.cameraComboBox.Size = new System.Drawing.Size(265, 35);
            this.cameraComboBox.TabIndex = 0;
            // 
            // cameraButton
            // 
            this.cameraButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraButton.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cameraButton.Location = new System.Drawing.Point(531, 204);
            this.cameraButton.Name = "cameraButton";
            this.cameraButton.Size = new System.Drawing.Size(265, 46);
            this.cameraButton.TabIndex = 1;
            this.cameraButton.Text = "カメラ接続";
            this.cameraButton.UseVisualStyleBackColor = true;
            this.cameraButton.Click += new System.EventHandler(this.CameraButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.statusLabel.Location = new System.Drawing.Point(26, 25);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(296, 27);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "カメラが接続されていません";
            // 
            // selectLabel
            // 
            this.selectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectLabel.AutoSize = true;
            this.selectLabel.Font = new System.Drawing.Font("07やさしさゴシック", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.selectLabel.Location = new System.Drawing.Point(526, 82);
            this.selectLabel.Name = "selectLabel";
            this.selectLabel.Size = new System.Drawing.Size(274, 27);
            this.selectLabel.TabIndex = 3;
            this.selectLabel.Text = "カメラを選択してください";
            // 
            // cameraPictureBox
            // 
            this.cameraPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraPictureBox.Location = new System.Drawing.Point(18, 63);
            this.cameraPictureBox.Name = "cameraPictureBox";
            this.cameraPictureBox.Size = new System.Drawing.Size(502, 368);
            this.cameraPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cameraPictureBox.TabIndex = 4;
            this.cameraPictureBox.TabStop = false;
            this.cameraPictureBox.Click += new System.EventHandler(this.cameraPictureBox_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(808, 442);
            this.Controls.Add(this.cameraPictureBox);
            this.Controls.Add(this.selectLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.cameraButton);
            this.Controls.Add(this.cameraComboBox);
            this.Name = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.ComboBox cameraComboBox;
        private System.Windows.Forms.Button cameraButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label selectLabel;
        private System.Windows.Forms.PictureBox cameraPictureBox;
    }
}

