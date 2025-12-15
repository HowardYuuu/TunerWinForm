namespace PitchShiftApp
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
            txtFile = new TextBox();
            btnFile = new Button();
            trbTone = new TrackBar();
            lblTone = new Label();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)trbTone).BeginInit();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(177, 288);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(299, 152);
            btnStart.TabIndex = 0;
            btnStart.Text = "播放";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // txtFile
            // 
            txtFile.Location = new Point(202, 107);
            txtFile.Name = "txtFile";
            txtFile.Size = new Size(416, 38);
            txtFile.TabIndex = 2;
            // 
            // btnFile
            // 
            btnFile.Location = new Point(641, 102);
            btnFile.Name = "btnFile";
            btnFile.Size = new Size(150, 46);
            btnFile.TabIndex = 3;
            btnFile.Text = "選擇檔案";
            btnFile.UseVisualStyleBackColor = true;
            btnFile.Click += btnFile_Click;
            // 
            // trbTone
            // 
            trbTone.Location = new Point(155, 178);
            trbTone.Maximum = 12;
            trbTone.Minimum = -12;
            trbTone.Name = "trbTone";
            trbTone.Size = new Size(540, 90);
            trbTone.TabIndex = 4;
            trbTone.Scroll += trackBar1_Scroll;
            trbTone.MouseUp += trbTone_MouseUp;
            // 
            // lblTone
            // 
            lblTone.BackColor = SystemColors.ButtonHighlight;
            lblTone.Font = new Font("Microsoft JhengHei UI", 13.875F, FontStyle.Regular, GraphicsUnit.Point, 136);
            lblTone.Location = new Point(701, 178);
            lblTone.Name = "lblTone";
            lblTone.Size = new Size(127, 47);
            lblTone.TabIndex = 5;
            lblTone.Text = "0";
            lblTone.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            button1.Location = new Point(492, 288);
            button1.Name = "button1";
            button1.Size = new Size(299, 152);
            button1.TabIndex = 6;
            button1.Text = "停止";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(954, 541);
            Controls.Add(button1);
            Controls.Add(lblTone);
            Controls.Add(trbTone);
            Controls.Add(btnFile);
            Controls.Add(txtFile);
            Controls.Add(btnStart);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)trbTone).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStart;
        private TextBox txtFile;
        private Button btnFile;
        private TrackBar trbTone;
        private Label lblTone;
        private Button button1;
    }
}
