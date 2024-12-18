using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TunerAPP_V2
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn; // 音訊錄製設備 (麥克風)
        private BufferedWaveProvider bufferedWaveProvider; // 音訊緩衝處理
        private float[] previousBuffer; // 儲存先前的音訊緩衝數據
        private const int SampleRate = 44100; // 音訊採樣率 (44.1kHz)
        private const int MinimumFrequency = 27;  // 最低偵測頻率 (27Hz)
        private const int MaximumFrequency = 4200; // 最高偵測頻率 (4.2kHz)

        // 音名與頻率對照表 (基準音的頻率)
        private static readonly Dictionary<string, float> NoteFrequencies = new Dictionary<string, float>
        {
            { "C", 16.35f }, { "C#", 17.32f }, { "D", 18.35f }, { "Eb", 19.45f },
            { "E", 20.60f }, { "F", 21.83f }, { "F#", 23.12f }, { "G", 24.50f },
            { "G#", 25.96f }, { "A", 27.50f }, { "Bb", 29.14f }, { "B", 30.87f }
        };

        private Queue<float> pitchHistory = new Queue<float>(); // 儲存音高歷史
        private const int MaxHistoryLength = 300; // 波形圖歷史點數限制

        public Form1()
        {
            InitializeComponent(); // 初始化視窗元件
            InitializeAudioDevices(); // 初始化音訊錄製設備
        }

        // 初始化音訊設備 (麥克風清單)
        private void InitializeAudioDevices()
        {
            cbxMachine.Items.Clear(); // 清空下拉選單
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                cbxMachine.Items.Add(WaveInEvent.GetCapabilities(i).ProductName); // 添加可用音訊設備
            }

            if (cbxMachine.Items.Count > 0)
            {
                cbxMachine.SelectedIndex = 0; // 預設選擇第一個設備
            }
        }

        // 開始偵測音訊頻率
        private void StartDetection(int deviceIndex)
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex, // 選擇音訊設備
                WaveFormat = new WaveFormat(SampleRate, 1) // 設定採樣率為 44.1kHz，單聲道
            };

            waveIn.DataAvailable += OnDataAvailable; // 當有音訊數據時觸發事件
            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true // 如果緩衝區溢出，則丟棄數據
            };

            waveIn.StartRecording(); // 開始錄音
        }

        // 停止偵測音訊頻率
        private void StopDetection()
        {
            waveIn?.StopRecording(); // 停止錄音
            waveIn?.Dispose(); // 釋放資源
            waveIn = null;
        }

        // 音訊數據可用時的處理方法
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded); // 添加數據到緩衝區

            float[] floatBuffer = ConvertToFloatArray(e.Buffer, e.BytesRecorded); // 將位元組數據轉為浮點數
            float detectedFrequency = DetectFrequency(floatBuffer, SampleRate); // 偵測音訊頻率

            // 確保頻率在設定的範圍內
            if (detectedFrequency >= MinimumFrequency && detectedFrequency <= MaximumFrequency)
            {
                string tuningIndicator; // 調音指示器 (↗、↘、●)
                string note = GetNoteNameByCent(detectedFrequency, out tuningIndicator); // 取得音名與調音指示器
                DisplayFrequency(detectedFrequency, note, tuningIndicator); // 顯示頻率與音高資訊

                UpdatePitchHistory(detectedFrequency); // 更新音高歷史
                DrawWaveform(); // 繪製波形圖
            }
        }

        // 將位元組數據轉換為浮點數陣列
        private float[] ConvertToFloatArray(byte[] buffer, int bytesRecorded)
        {
            int samples = bytesRecorded / sizeof(short); // 計算取樣數
            float[] floatBuffer = new float[samples]; // 建立浮點數陣列

            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * sizeof(short)); // 讀取 16 位元音訊數據
                floatBuffer[i] = sample / 32768f; // 轉換為浮點數 (標準化範圍: -1 到 1)
            }

            return floatBuffer;
        }

        // 偵測音訊頻率 (自相關函數法)
        private float DetectFrequency(float[] buffer, int sampleRate)
        {
            if (previousBuffer == null)
            {
                previousBuffer = new float[buffer.Length];
            }

            int minLag = sampleRate / MaximumFrequency; // 最小延遲值
            int maxLag = sampleRate / MinimumFrequency; // 最大延遲值

            float[] autocorrelation = new float[maxLag]; // 存放自相關數值

            // 計算自相關值
            for (int lag = minLag; lag < maxLag; lag++)
            {
                float correlation = 0;

                for (int i = 0; i < buffer.Length - lag; i++)
                {
                    correlation += buffer[i] * buffer[i + lag]; // 計算自相關
                }

                autocorrelation[lag] = correlation;
            }

            int bestLag = Array.IndexOf(autocorrelation, autocorrelation.Max()); // 找出最大自相關值的延遲

            if (autocorrelation[bestLag] > 0.01f) // 設定相關性閾值，過低則忽略
            {
                return (float)sampleRate / bestLag; // 根據延遲計算頻率
            }

            return 0.0f; // 頻率偵測失敗時回傳 0
        }

        // 根據頻率及音分判斷音名
        private string GetNoteNameByCent(float frequency, out string tuningIndicator)
        {
            tuningIndicator = "●"; // 預設為正確音高
            foreach (var note in NoteFrequencies)
            {
                float baseFrequency = note.Value; // 音名的基準頻率
                for (int octave = 0; octave < 9; octave++)
                {
                    float diff = 1200 * (float)Math.Log(frequency / baseFrequency, 2); // 計算頻率差異 (cent)
                    if (Math.Abs(diff) < 50) // 若頻率差異小於 50 cent
                    {
                        tuningIndicator = diff > 0 ? "↗" : (diff < 0 ? "↘" : "●"); // 判斷音高偏高或偏低
                        return $"{note.Key}{octave}"; // 回傳音名與八度數
                    }
                    baseFrequency *= 2; // 基頻翻倍 (下一個八度)
                }
            }
            return "超出範圍"; // 頻率超出可偵測範圍
        }

        // 顯示頻率與音高資訊
        private void DisplayFrequency(float frequency, string note, string tuningIndicator)
        {
            string displayMessage = $"頻率:   {frequency:F2} Hz    音高:   {note}   {tuningIndicator}\r\n";

            lblPitch.Invoke((Action)(() => lblPitch.Text = displayMessage));
            txtPitch.Invoke((Action)(() =>
            {
                txtPitch.AppendText(displayMessage);
                txtPitch.ScrollToCaret();
            }));
        }

        // 繪製波形圖
        private void DrawWaveform()
        {
            if (pictureBox1.Image == null)
            {
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            }

            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                // 清空畫布，設置背景為白色
                g.Clear(Color.White);

                // 設定參數
                int totalNotes = 88; // A0 (21) 到 C8 (108)，共 88 鍵
                float yStep = pictureBox1.Height / (float)totalNotes; // 每個音名的間隔

                int xStep = pictureBox1.Width / MaxHistoryLength; // 每個時間片的水平間隔

                // 繪製 Y 軸標籤 (音名)
                using (Pen gridPen = new Pen(Color.LightGray, 1)) // 刻度線為淺灰色
                using (Brush textBrush = new SolidBrush(Color.Black)) // 標籤文字為黑色
                using (Font font = new Font("Arial", 8))
                {
                    for (int i = 0; i < totalNotes; i++)
                    {
                        string note = GetNoteNameByIndex(i); // 根據索引取得音名
                        float y = pictureBox1.Height - i * yStep;

                        g.DrawLine(gridPen, 0, y, pictureBox1.Width, y); // 畫水平線
                        g.DrawString(note, font, textBrush, 0, y - font.Height / 2); // 顯示音名標籤
                    }
                }

                // 繪製波形曲線
                float[] pitches = pitchHistory.ToArray();
                using (Pen waveformPen = new Pen(Color.Green, 2)) // 波形為綠色
                {
                    for (int i = 0; i < pitches.Length - 1; i++)
                    {
                        float x1 = 40 + i * xStep;
                        float x2 = 40 + (i + 1) * xStep;

                        float y1 = GetYPositionByFrequency(pitches[i], pictureBox1.Height, totalNotes);
                        float y2 = GetYPositionByFrequency(pitches[i + 1], pictureBox1.Height, totalNotes);

                        if (y1 >= 0 && y1 <= pictureBox1.Height && y2 >= 0 && y2 <= pictureBox1.Height)
                        {
                            g.DrawLine(waveformPen, x1, y1, x2, y2);
                        }
                    }
                }
            }

            pictureBox1.Invalidate(); // 觸發重新繪製
        }

        // 更新音高波形紀錄
        private void UpdatePitchHistory(float frequency)
        {
            if (pitchHistory.Count >= MaxHistoryLength)
            {
                pitchHistory.Dequeue(); // 移除最舊的音高
            }
            pitchHistory.Enqueue(frequency);
        }

        // 根據索引取得音名 (A0 到 C8)
        private string GetNoteNameByIndex(int index)
        {
            string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            int octave = (index + 9) / 12; // 計算八度數
            string note = notes[index % 12]; // 計算音名
            return $"{note}{octave}";
        }

        // 根據頻率取得 Y 軸位置 (將頻率映射到音名對應的 Y 軸位置)
        private float GetYPositionByFrequency(float frequency, int height, int totalNotes)
        {
            // 計算頻率對應的 MIDI 音符索引
            float midiNote = 12 * (float)Math.Log(frequency / 27.5, 2) + 21; // A0 為 21
            if (midiNote < 21 || midiNote > 108) return -1; // 超出範圍，返回無效值

            // 映射到 Y 軸
            float yStep = height / (float)totalNotes;
            return height - (midiNote - 21) * yStep;
        }

        #region WinForm事件
        // 開始按鈕事件
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbxMachine.SelectedIndex >= 0)
            {
                StartDetection(cbxMachine.SelectedIndex);
            }
        }

        // 停止按鈕事件
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopDetection();
        }

        // 清除按鈕事件
        private void btnClear_Click(object sender, EventArgs e)
        {
            lblPitch.Text = string.Empty;
            txtPitch.Clear();
            pictureBox1.Image = null;
            pitchHistory.Clear();
        }

        // 音訊設備重新整理按鈕事件
        private void cbxMachine_Click(object sender, EventArgs e)
        {
            InitializeAudioDevices();
        }

        // 視窗關閉事件 (釋放資源)
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopDetection();
        }
        #endregion
    }
}
