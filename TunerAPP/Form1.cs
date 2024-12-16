using System;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;

namespace TunerAPP
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn;              // 用於錄音
        private BufferedWaveProvider waveProvider; // 用於音頻數據緩存
        private BiQuadFilter lowPassFilter;
        private const int sampleRate = 44100;     // 采樣率
        private const int fftSize = 8192;         // FFT大小
        private const float volumeThreshold = 0.001f; // 可根據需要調整閾值
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private const double A4Frequency = 440.0; // A4的頻率

        // 設定每個音階的基頻
        private Dictionary<string, float> KeyNote = new Dictionary<string, float>
        {
            {"C",(float)16.352 },
            {"C#/Db",(float)17.324 },
            {"D",(float)18.354 },
            {"D#/Eb",(float)19.445 },
            {"E",(float)20.602 },
            {"F",(float)21.827 },
            {"F#/Gb",(float)23.125 },
            {"G",(float)24.500 },
            {"G#/Ab",(float)25.957 },
            {"A",(float)27.500 },
            {"A#/Bb",(float)29.135 },
            {"B",(float)30.868 },
        };

        public Form1()
        {
            InitializeComponent();
            InitializeAudioCapture();
        }

        // 初始化音頻捕獲
        private void InitializeAudioCapture()
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = 0, // 使用預設音訊設備
                WaveFormat = new WaveFormat(sampleRate, 1) // 單聲道, 44.1kHz
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(1),
                DiscardOnBufferOverflow = true
            };
            waveIn.StartRecording();
            // 設置低通濾波器，過濾掉1000 Hz以上的頻率
            lowPassFilter = BiQuadFilter.LowPassFilter(44100, 1000, 0.707f);
        }

        // 處理捕獲的音頻數據
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            // 將音頻數據添加到緩衝區
            waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

            // 使用 byte[] 讀取數據
            byte[] byteBuffer = new byte[fftSize * 2]; // 16-bit 音頻數據，1個樣本 2個字節
            int samplesRead = waveProvider.Read(byteBuffer, 0, byteBuffer.Length);

            // 如果沒有讀取到足夠的數據則返回
            if (samplesRead < byteBuffer.Length) return;

            // 將 byte[] 轉換為 float[]，這裡假設是 16-bit PCM 音頻數據
            float[] samples = new float[fftSize];
            for (int i = 0; i < samples.Length; i++)
            {
                // 假設每個音頻樣本是 16-bit，將 byte 轉換為 float
                short sample = BitConverter.ToInt16(byteBuffer, i * 2);  // 16-bit 音頻，每個樣本 2 個字節
                samples[i] = sample / 32768f; // 將範圍從 [-32768, 32767] 映射到 [-1, 1]
            }

            // 濾波處理：通過低通濾波器處理音頻數據
            for (int i = 0; i < samples.Length; i++)
            {
                // 如果音量小於閾值，則將樣本設為 0 或跳過
                if (Math.Abs(samples[i]) < volumeThreshold)
                {
                    samples[i] = 0; // 或者跳過此樣本，根據需要調整
                }
                else
                {
                    // 使用低通濾波器對音頻數據進行過濾處理
                    samples[i] = lowPassFilter.Transform(samples[i]);
                }
            }

            // 使用FFT分析音頻信號的頻率
            var fftBuffer = new Complex[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                fftBuffer[i] = new Complex { X = samples[i], Y = 0 };
            }

            // 計算FFT
            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2), fftBuffer);

            // 獲取頻率分量並找出最大頻率
            var magnitudes = fftBuffer.Select(c => Math.Sqrt(c.X * c.X + c.Y * c.Y)).ToArray();
            int maxIndex = magnitudes.Skip(1).ToList().IndexOf(magnitudes.Skip(1).Max()) + 1;
            double frequency = maxIndex * (sampleRate / (double)fftSize);

            // 限制音高範圍在C0到B8
            if (frequency < 16.35 || frequency > 7902.13) return; // C0 = 16.35 Hz, B8 = 7902.13 Hz

            // 顯示音高（頻率）及對應音符
            string note = FrequencyToNote(frequency, out double deviation);
            Invoke(new Action(() =>
            {
                labelFrequency.Text = $"Frequency: {frequency:F2} Hz";
                labelNote.Text = $"Note: {note} ({deviation:F2} Hz deviation)";
            }));
        }

        // 根據頻率計算音符名稱和偏差
        private string FrequencyToNote(double frequency, out double deviation)
        {
            const double A4Frequency = 440.0; // A4 基準頻率
            if (frequency <= 0)
            {
                deviation = 0;
                return "N/A"; // 無效頻率
            }

            // 計算相對於 A4 的半音數
            double semitoneOffset = 12 * Math.Log2(frequency / A4Frequency);
            int closestNoteIndex = (int)Math.Round(semitoneOffset) % 12;
            closestNoteIndex = (closestNoteIndex + 12) % 12; // 確保結果在 0-11 範圍內

            // 計算音符的八度
            int octave = (int)Math.Floor((semitoneOffset + 9) / 12) + 4;

            // 計算最近音符的頻率
            double closestSemitoneOffset = Math.Round(semitoneOffset); // 取整後的半音數
            double closestFrequency = A4Frequency * Math.Pow(2, closestSemitoneOffset / 12.0);

            // 計算頻率與最近音符的偏差
            deviation = frequency - closestFrequency;

            // 計算音符名稱及八度
            string noteName = NoteNames[closestNoteIndex];
            return $"{noteName}{octave}";
        }


        // 當窗體關閉時停止錄音
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            base.OnFormClosing(e);
        }
    }
}