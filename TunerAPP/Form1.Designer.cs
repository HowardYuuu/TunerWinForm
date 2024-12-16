namespace TunerAPP
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
            waveViewer1 = new NAudio.Gui.WaveViewer();
            labelFrequency = new Label();
            labelNote = new Label();
            SuspendLayout();
            // 
            // waveViewer1
            // 
            waveViewer1.BorderStyle = BorderStyle.FixedSingle;
            waveViewer1.Location = new Point(96, 588);
            waveViewer1.Name = "waveViewer1";
            waveViewer1.SamplesPerPixel = 128;
            waveViewer1.Size = new Size(1292, 313);
            waveViewer1.StartPosition = 0L;
            waveViewer1.TabIndex = 0;
            waveViewer1.WaveStream = null;
            // 
            // labelFrequency
            // 
            labelFrequency.Location = new Point(156, 106);
            labelFrequency.Name = "labelFrequency";
            labelFrequency.Size = new Size(558, 90);
            labelFrequency.TabIndex = 1;
            labelFrequency.Text = "labelFrequency";
            // 
            // labelNote
            // 
            labelNote.Location = new Point(156, 333);
            labelNote.Name = "labelNote";
            labelNote.Size = new Size(174, 60);
            labelNote.TabIndex = 2;
            labelNote.Text = "labelNote";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1485, 953);
            Controls.Add(labelNote);
            Controls.Add(labelFrequency);
            Controls.Add(waveViewer1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private NAudio.Gui.WaveViewer waveViewer1;
        private Label labelFrequency;
        private Label labelNote;
    }
}
