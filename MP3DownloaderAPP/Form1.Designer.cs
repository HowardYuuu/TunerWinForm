namespace MP3DownloaderAPP
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtUrl = new TextBox();
            btnDownload = new Button();
            listUrl = new ListBox();
            btnAdd = new Button();
            listStatus = new ListBox();
            btnPath = new Button();
            lblPath = new Label();
            SuspendLayout();
            // 
            // txtUrl
            // 
            txtUrl.Font = new Font("Microsoft JhengHei UI", 16.125F, FontStyle.Regular, GraphicsUnit.Point, 136);
            txtUrl.Location = new Point(46, 178);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(883, 62);
            txtUrl.TabIndex = 0;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(1206, 79);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(301, 186);
            btnDownload.TabIndex = 1;
            btnDownload.Text = "開始";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // listUrl
            // 
            listUrl.FormattingEnabled = true;
            listUrl.ItemHeight = 30;
            listUrl.Location = new Point(46, 302);
            listUrl.Name = "listUrl";
            listUrl.Size = new Size(1461, 274);
            listUrl.TabIndex = 2;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(949, 178);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(240, 87);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "加入";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // listStatus
            // 
            listStatus.FormattingEnabled = true;
            listStatus.ItemHeight = 30;
            listStatus.Location = new Point(46, 614);
            listStatus.Name = "listStatus";
            listStatus.Size = new Size(1461, 334);
            listStatus.TabIndex = 4;
            // 
            // btnPath
            // 
            btnPath.Location = new Point(949, 79);
            btnPath.Name = "btnPath";
            btnPath.Size = new Size(240, 87);
            btnPath.TabIndex = 6;
            btnPath.Text = "路徑";
            btnPath.UseVisualStyleBackColor = true;
            btnPath.Click += btnPath_Click;
            // 
            // lblPath
            // 
            lblPath.BackColor = SystemColors.ButtonHighlight;
            lblPath.Font = new Font("Microsoft JhengHei UI", 16.125F, FontStyle.Regular, GraphicsUnit.Point, 136);
            lblPath.Location = new Point(46, 88);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(883, 60);
            lblPath.TabIndex = 7;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1557, 1034);
            Controls.Add(lblPath);
            Controls.Add(btnPath);
            Controls.Add(listStatus);
            Controls.Add(btnAdd);
            Controls.Add(listUrl);
            Controls.Add(btnDownload);
            Controls.Add(txtUrl);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUrl;
        private Button btnDownload;
        private ListBox listUrl;
        private Button btnAdd;
        private ListBox listStatus;
        private Button btnPath;
        private Label lblPath;
    }
}
