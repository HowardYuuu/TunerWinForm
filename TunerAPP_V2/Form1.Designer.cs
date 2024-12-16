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
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(677, 41);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(150, 46);
            btnStart.TabIndex = 0;
            btnStart.Text = "開始檢測";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // lblPitch
            // 
            lblPitch.BackColor = SystemColors.ButtonHighlight;
            lblPitch.Font = new Font("Consolas", 19.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPitch.Location = new Point(82, 122);
            lblPitch.Name = "lblPitch";
            lblPitch.Size = new Size(1235, 101);
            lblPitch.TabIndex = 1;
            // 
            // cbxMachine
            // 
            cbxMachine.FormattingEnabled = true;
            cbxMachine.Location = new Point(224, 49);
            cbxMachine.Name = "cbxMachine";
            cbxMachine.Size = new Size(407, 38);
            cbxMachine.TabIndex = 2;
            cbxMachine.Text = "請選擇音訊設備";
            cbxMachine.Click += cbxMachine_Click;
            // 
            // txtPitch
            // 
            txtPitch.Dock = DockStyle.Fill;
            txtPitch.Location = new Point(0, 0);
            txtPitch.Multiline = true;
            txtPitch.Name = "txtPitch";
            txtPitch.ScrollBars = ScrollBars.Both;
            txtPitch.Size = new Size(1268, 527);
            txtPitch.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.Controls.Add(txtPitch);
            panel1.Location = new Point(82, 249);
            panel1.Name = "panel1";
            panel1.Size = new Size(1268, 527);
            panel1.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(1019, 41);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(150, 46);
            btnClear.TabIndex = 5;
            btnClear.Text = "清空結果";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(851, 41);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(150, 46);
            btnStop.TabIndex = 6;
            btnStop.Text = "停止";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1437, 851);
            Controls.Add(btnStop);
            Controls.Add(btnClear);
            Controls.Add(panel1);
            Controls.Add(cbxMachine);
            Controls.Add(lblPitch);
            Controls.Add(btnStart);
            Name = "Form1";
            Text = "Form1";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
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
    }
}
