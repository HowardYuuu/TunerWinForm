namespace TunerAPP_V3
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.cbxMachine = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblPitch = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtPitch = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbxMachine
            // 
            this.cbxMachine.FormattingEnabled = true;
            this.cbxMachine.IntegralHeight = false;
            this.cbxMachine.Location = new System.Drawing.Point(97, 96);
            this.cbxMachine.Name = "cbxMachine";
            this.cbxMachine.Size = new System.Drawing.Size(356, 32);
            this.cbxMachine.TabIndex = 0;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(552, 69);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(166, 85);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "開始檢測";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(810, 67);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(190, 87);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "停止";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lblPitch
            // 
            this.lblPitch.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblPitch.Font = new System.Drawing.Font("Consolas", 22.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPitch.Location = new System.Drawing.Point(119, 206);
            this.lblPitch.Name = "lblPitch";
            this.lblPitch.Size = new System.Drawing.Size(815, 94);
            this.lblPitch.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtPitch);
            this.panel1.Location = new System.Drawing.Point(609, 377);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(858, 511);
            this.panel1.TabIndex = 4;
            // 
            // txtPitch
            // 
            this.txtPitch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPitch.Location = new System.Drawing.Point(0, 0);
            this.txtPitch.Multiline = true;
            this.txtPitch.Name = "txtPitch";
            this.txtPitch.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPitch.Size = new System.Drawing.Size(858, 511);
            this.txtPitch.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1615, 1018);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblPitch);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.cbxMachine);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxMachine;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblPitch;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtPitch;
    }
}

