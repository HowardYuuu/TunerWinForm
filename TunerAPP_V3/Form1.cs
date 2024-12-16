using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MathNet.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TunerAPP_V3
{
    public partial class Form1 : Form
    {
        static BufferedWaveProvider bufferedWaveProvider = null;
        static Dictionary<string, float> noteBaseFreqs = new Dictionary<string, float>()
        {
            { "C", 16.35f }, { "C#", 17.32f }, { "D", 18.35f }, { "Eb", 19.45f },
            { "E", 20.60f }, { "F", 21.83f }, { "F#", 23.12f }, { "G", 24.50f },
            { "G#", 25.96f }, { "A", 27.50f }, { "Bb", 29.14f }, { "B", 30.87f }
        };

        private WaveInEvent waveIn;

        public Form1()
        {
            InitializeComponent();
            InitializeDevices();
        }

        // 初始化錄音設備
        private void InitializeDevices()
        {
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                cbxMachine.Items.Add(WaveInEvent.GetCapabilities(i).ProductName);
            }

            if (cbxMachine.Items.Count > 0)
            {
                cbxMachine.SelectedIndex = 0;
            }
        }

        // 開始按鈕事件
        private void btnStart_Click(object sender, EventArgs e)
        {
            int inputDevice = cbxMachine.SelectedIndex;
            StartDetect(inputDevice);
        }

        // 開始錄音與頻率檢測
        private void StartDetect(int inputDevice)
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = inputDevice,
                WaveFormat = new WaveFormat(44100, 1)
            };

            waveIn.DataAvailable += WaveIn_DataAvailable;

            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            waveIn.StartRecording();
        }

        // DataAvailable 處理
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

            Task.Run(() =>
            {
                IWaveProvider stream = new Wave16ToFloatProvider(bufferedWaveProvider);
                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                int sampleRate = stream.WaveFormat.SampleRate;

                if (bytesRead > 0)
                {
                    float pitch = DetectPitchUsingFFT(buffer, bytesRead / sizeof(float), sampleRate);
                    UpdateUI(pitch);
                }
            });
        }

        // 更新 UI 顯示
        private void UpdateUI(float freq)
        {
            if (freq > 20.0f && freq < 20000.0f) // 確保頻率合理
            {
                string tuningIndicator;
                string note = GetNoteByCents(freq, out tuningIndicator);
                string displayMessage = $"頻率: {freq:F2} Hz 音高: {note} {tuningIndicator}\n";

                Invoke((Action)(() =>
                {
                    lblPitch.Text = displayMessage;
                    txtPitch.AppendText(displayMessage);
                }));
            }
        }

        // 使用 FFT 計算音高
        private static float DetectPitchUsingFFT(byte[] buffer, int frames, int sampleRate)
        {
            float[] floatBuffer = new WaveBuffer(buffer).FloatBuffer;

            // 將音頻數據轉換為複數格式
            var complexBuffer = floatBuffer.Select(v => new MathNet.Numerics.Complex32(v, 0)).ToArray();

            // FFT 計算
            MathNet.Numerics.IntegralTransforms.Fourier.Forward(complexBuffer, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

            // 頻域自相關
            for (int i = 0; i < complexBuffer.Length; i++)
            {
                complexBuffer[i] *= MathNet.Numerics.Complex32.Conjugate(complexBuffer[i]);
            }

            // IFFT 還原到時域
            MathNet.Numerics.IntegralTransforms.Fourier.Inverse(complexBuffer, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

            // 找出複數的模長陣列
            float[] magnitudes = complexBuffer.Select(c => c.Magnitude).ToArray();

            // 找到最大峰值對應的頻率
            int maxLag = Array.IndexOf(magnitudes, magnitudes.Max());
            float pitch = (float)sampleRate / maxLag;

            return pitch;
        }

        // 根據音分計算匹配音符
        private static string GetNoteByCents(float freq, out string tuningIndicator)
        {
            tuningIndicator = "";

            foreach (var note in noteBaseFreqs)
            {
                float baseFreq = note.Value;

                for (int i = 0; i < 9; i++)
                {
                    float cents = 1200 * (float)Math.Log(freq / baseFreq);
                    if (Math.Abs(cents) <= 50) // 在 ±50 音分內
                    {
                        tuningIndicator = cents > 0 ? "偏高" : (cents < 0 ? "偏低" : "");
                        return $"{note.Key}{i}";
                    }
                    baseFreq *= 2; // 遞增八度
                }
            }
            return null;
        }

        // 清除 UI
        private void btnClear_Click(object sender, EventArgs e)
        {
            lblPitch.Text = "";
            txtPitch.Text = "";
            waveIn?.StopRecording();
            waveIn?.Dispose();
        }

        // 關閉應用程式時釋放資源
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
        }
    }
}

