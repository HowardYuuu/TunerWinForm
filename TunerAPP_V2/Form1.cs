using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TunerAPP_V2
{
    public partial class Form1 : Form
    {
        private WaveInEvent _waveIn; // 音訊錄製設備 (麥克風)
        private BufferedWaveProvider _bufferedWaveProvider; // 音訊緩衝處理
        private float[] _previousBuffer; // 儲存先前的音訊緩衝數據
        private const int _sampleRate = 44100; // 音訊採樣率 (44.1kHz)
        private const int _minimumFrequency = 27;  // 最低偵測頻率 (27Hz)
        private const int _maximumFrequency = 4200; // 最高偵測頻率 (4.2kHz)
        private string tuningIndicator;
        private float df;

        // 音名與頻率對照表 (基準音頻)
        private static readonly Dictionary<string, float> _noteFrequencies = new Dictionary<string, float>
        {
            { "C", (float) 16.352 }, { "C#", (float) 17.324 }, { "D", (float) 18.354 },
            { "Eb", (float) 19.445 },{ "E", (float) 20.602 }, { "F", (float) 21.827 },
            { "F#", (float) 23.125 }, { "G", (float) 24.500 },{ "G#", (float) 25.957 },
            { "A", (float) 27.500 }, { "Bb", (float) 29.135 }, { "B", (float) 30.867 }
        };

        private Queue<float> _pitchHistory = new Queue<float>(); // 儲存音高歷史
        private const int _maxHistoryLength = 300; // 波形圖歷史點數限制

        public Form1()
        {
            InitializeComponent(); // 初始化視窗元件
            InitializeAudioDevices(); // 初始化音訊錄製設備
        }

        // 初始化音訊設備 (麥克風清單)
        private void InitializeAudioDevices()
        {
            cbxMachine.Items.Clear();
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                cbxMachine.Items.Add(WaveInEvent.GetCapabilities(i).ProductName); // 添加可用音訊設備
            }

            if (cbxMachine.Items.Count > 0)
            {
                cbxMachine.SelectedIndex = 0;
            }
        }

        //開始偵測音訊頻率
        private void StartDetection(int deviceIndex)
        {
            //建立收音物件
            _waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex, //選擇音訊設備
                WaveFormat = new WaveFormat(_sampleRate, 1) //設定採樣率，單聲道
            };

            //觸發事件
            _waveIn.DataAvailable += OnDataAvailable;

            //建立緩衝區物件
            _bufferedWaveProvider = new BufferedWaveProvider(_waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            _waveIn.StartRecording();
        }

        // 停止偵測音訊頻率
        private void StopDetection()
        {
            _waveIn?.StopRecording(); // 停止錄音
            _waveIn?.Dispose(); // 釋放資源
            _waveIn = null;
        }

        // 音訊數據可用時的處理方法
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded); // 添加數據到緩衝區

            float[] floatBuffer = ConvertToFloatArray(e.Buffer, e.BytesRecorded); // 將位元組數據轉為浮點數
            float detectedFrequency = DetectFrequency(floatBuffer, _sampleRate); // 偵測音訊頻率

            // 確保頻率在設定的範圍內
            if (detectedFrequency >= _minimumFrequency && detectedFrequency <= _maximumFrequency)
            {
                string note = GetNoteNameByCentBaseFreqFirst(detectedFrequency);
                DisplayFrequency(detectedFrequency, note, tuningIndicator, df);

                UpdatePitchHistory(detectedFrequency);
                DrawWaveform();
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
            if (_previousBuffer == null)
            {
                _previousBuffer = new float[buffer.Length];
            }

            int minLag = sampleRate / _maximumFrequency; // 最小延遲值
            int maxLag = sampleRate / _minimumFrequency; // 最大延遲值

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

            return 0.0f;
        }

        /// <summary>
        /// 根據頻率及音分判斷音名(先遊歷每個八度的第一個音)
        /// </summary>
        /// <param name = "frequency" > 傳入頻率(Hz) </ param >
        /// <returns></returns>
        private string GetNoteNameByCentOctaveFirst(float frequency)
        {
            foreach (var note in _noteFrequencies)
            {
                float baseFrequency = note.Value;
                for (int octave = 0; octave < 9; octave++)
                {
                    df = 1200 * (float)Math.Log(frequency / baseFrequency, 2);

                    if (df >= -50 && df <= 50)
                    {
                        switch (df)
                        {
                            case > (float)5:
                                tuningIndicator = "↗";
                                break;
                            case < (float)-5:
                                tuningIndicator = "↘";
                                break;
                            default:
                                tuningIndicator = "●";
                                break;
                        }
                        return $"{note.Key}{octave}";
                    }
                    baseFrequency *= 2;
                }
            }
            return "超出範圍";
        }

        /// <summary>
        /// 根據頻率及音分判斷音名(先遊歷第一個八度)
        /// </summary>
        /// <param name="frequency"> 傳入頻率(Hz) </param>
        /// <returns></returns>
        private string GetNoteNameByCentBaseFreqFirst(float frequency)
        {
            for (int octave = 0; octave < 9; octave++)
            {
                foreach (var note in _noteFrequencies)
                {
                    float baseFrequency = note.Value * (float)Math.Pow(2, octave);
                    df = 1200 * (float)Math.Log(frequency / baseFrequency, 2);

                    if (df >= -50 && df <= 50)
                    {
                        switch (df)
                        {
                            case > (float)5:
                                tuningIndicator = "↗";
                                break;
                            case < (float)-5:
                                tuningIndicator = "↘";
                                break;
                            default:
                                tuningIndicator = "●";
                                break;
                        }
                        return $"{note.Key}{octave}";
                    }
                }
            }
            return "超出範圍";
        }

        // 顯示頻率與音高資訊
        private void DisplayFrequency(float frequency, string note, string tuningIndicator, float diff)
        {
            string displayMessage = $"頻率:  {frequency:F3} Hz   音高:  {note} {tuningIndicator}   音分差： {diff:F3}\r\n";

            lblPitch.Invoke((Action)(() => lblPitch.Text = displayMessage));
            txtPitch.Invoke((Action)(() =>
            {
                txtPitch.AppendText(displayMessage);
                txtPitch.ScrollToCaret();
            }));
        }

        #region 繪製波形圖
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

                int xStep = pictureBox1.Width / _maxHistoryLength; // 每個時間片的水平間隔

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
                float[] pitches = _pitchHistory.ToArray();
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
            if (_pitchHistory.Count >= _maxHistoryLength)
            {
                _pitchHistory.Dequeue(); // 移除最舊的音高
            }
            _pitchHistory.Enqueue(frequency);
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
        #endregion

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
            _pitchHistory.Clear();
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
