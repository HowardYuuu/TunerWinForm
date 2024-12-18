namespace TunerAPP_V2
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
            btnStart = new Button();
            lblPitch = new Label();
            cbxMachine = new ComboBox();
            txtPitch = new TextBox();
            panel1 = new Panel();
            btnClear = new Button();
            btnStop = new Button();
            pictureBox1 = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Top;
            btnStart.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 136);
            btnStart.Location = new Point(939, 19);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(167, 103);
            btnStart.TabIndex = 0;
            btnStart.Text = "開始檢測";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // lblPitch
            // 
            lblPitch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPitch.BackColor = SystemColors.ButtonHighlight;
            lblPitch.Font = new Font("Consolas", 19.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblPitch.Location = new Point(82, 135);
            lblPitch.Name = "lblPitch";
            lblPitch.Size = new Size(1725, 101);
            lblPitch.TabIndex = 1;
            // 
            // cbxMachine
            // 
            cbxMachine.Anchor = AnchorStyles.Top;
            cbxMachine.Font = new Font("Microsoft JhengHei UI", 13.875F, FontStyle.Regular, GraphicsUnit.Point, 136);
            cbxMachine.FormattingEnabled = true;
            cbxMachine.Location = new Point(327, 39);
            cbxMachine.Name = "cbxMachine";
            cbxMachine.Size = new Size(565, 55);
            cbxMachine.TabIndex = 2;
            cbxMachine.Text = "請選擇音訊設備";
            cbxMachine.Click += cbxMachine_Click;
            // 
            // txtPitch
            // 
            txtPitch.Dock = DockStyle.Fill;
            txtPitch.Font = new Font("Consolas", 13.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPitch.Location = new Point(0, 0);
            txtPitch.Multiline = true;
            txtPitch.Name = "txtPitch";
            txtPitch.ScrollBars = ScrollBars.Both;
            txtPitch.Size = new Size(1762, 366);
            txtPitch.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(txtPitch);
            panel1.Location = new Point(82, 754);
            panel1.Name = "panel1";
            panel1.Size = new Size(1762, 366);
            panel1.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top;
            btnClear.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 136);
            btnClear.Location = new Point(1304, 19);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(183, 103);
            btnClear.TabIndex = 5;
            btnClear.Text = "清空結果";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnStop
            // 
            btnStop.Anchor = AnchorStyles.Top;
            btnStop.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 136);
            btnStop.Location = new Point(1136, 19);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(145, 103);
            btnStop.TabIndex = 6;
            btnStop.Text = "停止";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackColor = SystemColors.ButtonHighlight;
            pictureBox1.Location = new Point(82, 250);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1725, 490);
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1927, 1143);
            Controls.Add(pictureBox1);
            Controls.Add(btnStop);
            Controls.Add(btnClear);
            Controls.Add(panel1);
            Controls.Add(cbxMachine);
            Controls.Add(lblPitch);
            Controls.Add(btnStart);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnStart;
        private Label lblPitch;
        private ComboBox cbxMachine;
        private TextBox txtPitch;
        private Panel panel1;
        private Button btnClear;
        private Button btnStop;
        private PictureBox pictureBox1;
    }
}
