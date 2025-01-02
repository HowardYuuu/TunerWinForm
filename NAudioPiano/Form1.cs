using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Timer = System.Windows.Forms.Timer;

namespace NAudioPiano
{
    public partial class Form1 : Form
    {

        private int _octive;
        private IWavePlayer _waveOut;
        private SignalGenerator _signalGenerator;

        public Form1()
        {
            InitializeComponent();
            InitializeAudioDevices();
            InitKeys();
        }


        /// <summary>
        /// 重置琴鍵音高顯示
        /// </summary>
        private void InitKeys()
        {
            button1.Text = $"C{numericUpDown1.Value}";
            button3.Text = $"D{numericUpDown1.Value}";
            button5.Text = $"E{numericUpDown1.Value}";
            button6.Text = $"F{numericUpDown1.Value}";
            button8.Text = $"G{numericUpDown1.Value}";
            button10.Text = $"A{numericUpDown1.Value}";
            button12.Text = $"B{numericUpDown1.Value}";
        }

        /// <summary>
        /// 執行音訊播放
        /// </summary>
        /// <param name="frequency">播放頻率(Hz)</param>
        private void PlayPiano(float frequency)
        {
            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Stop();
            }

            // 初始化音頻生成器
            _signalGenerator = new SignalGenerator
            {
                Gain = 0.2, // 音量
                Frequency = frequency,
                Type = SignalGeneratorType.Sin
            };
            _waveOut.Init(_signalGenerator);
            _waveOut.Play();
            textBox1.Text = frequency.ToString() + " Hz";

            Timer timer = new Timer { Interval = 3000 };
            timer.Tick += (s, args) =>
            {
                _waveOut.Stop();
                textBox1.Text = "";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        /// <summary>
        /// 初始化音訊設備
        /// </summary>
        private void InitializeAudioDevices()
        {
            cbxMachine.Items.Clear();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                cbxMachine.Items.Add(WaveOut.GetCapabilities(i).ProductName);
            }

            if (cbxMachine.Items.Count > 0)
            {
                cbxMachine.SelectedIndex = 0;

                _waveOut = new WaveOutEvent()
                {
                    DeviceNumber = cbxMachine.SelectedIndex,
                };
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float frequency = (float)16.352 *(float)Math.Pow(2,_octive);
            PlayPiano(frequency);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            float frequency = (float)17.324 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float frequency = (float)18.354 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            float frequency = (float)19.445 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            float frequency = (float)20.602 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            float frequency = (float)21.827 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            float frequency = (float)23.125 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            float frequency = (float)24.500 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            float frequency = (float)25.957 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            float frequency = (float)27.500 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            float frequency = (float)29.135 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            float frequency = (float)30.868 * (float)Math.Pow(2, _octive);
            PlayPiano(frequency);
        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _octive = Convert.ToInt32(numericUpDown1.Value);
            InitKeys();
        }

        private void cbxMachine_SelectedIndexChanged(object sender, EventArgs e)
        {
            _waveOut = new WaveOutEvent()
            {
                DeviceNumber = cbxMachine.SelectedIndex,
            };
        }
    }
}
