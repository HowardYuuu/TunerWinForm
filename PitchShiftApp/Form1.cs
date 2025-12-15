using System;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NWaves.Windows;


namespace PitchShiftApp
{
    public partial class Form1 : Form
    {
        private IWavePlayer _waveOut;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtFile.Text))
            {
                MessageBox.Show("尚未選擇音檔");
                return;
            }
            else
            {
                StartPitchShift();
            }
        }

        private async void StartPitchShift()
        {
            try
            {
                string inPath = $@"{txtFile.Text}";
                var semiTone = Math.Pow(2, 1.0 / 12);
                int tone = trbTone.Value;
                await Task.Run(() =>
                {
                    using (var reader = new MediaFoundationReader(inPath))
                    {
                        var pitch = new SmbPitchShiftingSampleProvider(reader.ToSampleProvider());
                        using (_waveOut = new WaveOutEvent())
                        {
                            pitch.PitchFactor = (float)Math.Pow(semiTone, tone);
                            _waveOut.Init(pitch);
                            _waveOut.Play();

                            while (_waveOut.PlaybackState == PlaybackState.Playing)
                            {
                                Thread.Sleep(500);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "音訊檔案 (*.wav;*.mp3)|*.wav;*.mp3|所有檔案 (*.*)|*.*";
                openFileDialog.Title = "選擇音訊檔案";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (txtFile != null)
                    {
                        txtFile.Text = openFileDialog.FileName;
                    }
                }
            }
        }

        private void StopOut()
        {
            if (_waveOut != null)
            {
                if (_waveOut.PlaybackState == PlaybackState.Playing ||
                    _waveOut.PlaybackState == PlaybackState.Paused)
                {
                    _waveOut.Stop();
                }
                _waveOut.Dispose();
                _waveOut = null;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lblTone.Text = trbTone.Value.ToString();
        }

        private void trbTone_MouseUp(object sender, MouseEventArgs e)
        {
            StopOut();
            StartPitchShift();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StopOut();
        }
    }
}
