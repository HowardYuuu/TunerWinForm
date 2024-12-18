using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TunerAPP_V2
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn; // ���T���s�]�� (���J��)
        private BufferedWaveProvider bufferedWaveProvider; // ���T�w�ĳB�z
        private float[] previousBuffer; // �x�s���e�����T�w�ļƾ�
        private const int SampleRate = 44100; // ���T�ļ˲v (44.1kHz)
        private const int MinimumFrequency = 27;  // �̧C�����W�v (27Hz)
        private const int MaximumFrequency = 4200; // �̰������W�v (4.2kHz)

        // ���W�P�W�v��Ӫ� (��ǭ����W�v)
        private static readonly Dictionary<string, float> NoteFrequencies = new Dictionary<string, float>
        {
            { "C", 16.35f }, { "C#", 17.32f }, { "D", 18.35f }, { "Eb", 19.45f },
            { "E", 20.60f }, { "F", 21.83f }, { "F#", 23.12f }, { "G", 24.50f },
            { "G#", 25.96f }, { "A", 27.50f }, { "Bb", 29.14f }, { "B", 30.87f }
        };

        private Queue<float> pitchHistory = new Queue<float>(); // �x�s�������v
        private const int MaxHistoryLength = 300; // �i�ιϾ��v�I�ƭ���

        public Form1()
        {
            InitializeComponent(); // ��l�Ƶ�������
            InitializeAudioDevices(); // ��l�ƭ��T���s�]��
        }

        // ��l�ƭ��T�]�� (���J���M��)
        private void InitializeAudioDevices()
        {
            cbxMachine.Items.Clear(); // �M�ŤU�Կ��
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                cbxMachine.Items.Add(WaveInEvent.GetCapabilities(i).ProductName); // �K�[�i�έ��T�]��
            }

            if (cbxMachine.Items.Count > 0)
            {
                cbxMachine.SelectedIndex = 0; // �w�]��ܲĤ@�ӳ]��
            }
        }

        // �}�l�������T�W�v
        private void StartDetection(int deviceIndex)
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex, // ��ܭ��T�]��
                WaveFormat = new WaveFormat(SampleRate, 1) // �]�w�ļ˲v�� 44.1kHz�A���n�D
            };

            waveIn.DataAvailable += OnDataAvailable; // �����T�ƾڮ�Ĳ�o�ƥ�
            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true // �p�G�w�İϷ��X�A�h���ƾ�
            };

            waveIn.StartRecording(); // �}�l����
        }

        // ��������T�W�v
        private void StopDetection()
        {
            waveIn?.StopRecording(); // �������
            waveIn?.Dispose(); // ����귽
            waveIn = null;
        }

        // ���T�ƾڥi�ήɪ��B�z��k
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded); // �K�[�ƾڨ�w�İ�

            float[] floatBuffer = ConvertToFloatArray(e.Buffer, e.BytesRecorded); // �N�줸�ռƾ��ର�B�I��
            float detectedFrequency = DetectFrequency(floatBuffer, SampleRate); // �������T�W�v

            // �T�O�W�v�b�]�w���d��
            if (detectedFrequency >= MinimumFrequency && detectedFrequency <= MaximumFrequency)
            {
                string tuningIndicator; // �խ����ܾ� (���B���B��)
                string note = GetNoteNameByCent(detectedFrequency, out tuningIndicator); // ���o���W�P�խ����ܾ�
                DisplayFrequency(detectedFrequency, note, tuningIndicator); // ����W�v�P������T

                UpdatePitchHistory(detectedFrequency); // ��s�������v
                DrawWaveform(); // ø�s�i�ι�
            }
        }

        // �N�줸�ռƾ��ഫ���B�I�ư}�C
        private float[] ConvertToFloatArray(byte[] buffer, int bytesRecorded)
        {
            int samples = bytesRecorded / sizeof(short); // �p����˼�
            float[] floatBuffer = new float[samples]; // �إ߯B�I�ư}�C

            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * sizeof(short)); // Ū�� 16 �줸���T�ƾ�
                floatBuffer[i] = sample / 32768f; // �ഫ���B�I�� (�зǤƽd��: -1 �� 1)
            }

            return floatBuffer;
        }

        // �������T�W�v (�۬�����ƪk)
        private float DetectFrequency(float[] buffer, int sampleRate)
        {
            if (previousBuffer == null)
            {
                previousBuffer = new float[buffer.Length];
            }

            int minLag = sampleRate / MaximumFrequency; // �̤p�����
            int maxLag = sampleRate / MinimumFrequency; // �̤j�����

            float[] autocorrelation = new float[maxLag]; // �s��۬����ƭ�

            // �p��۬�����
            for (int lag = minLag; lag < maxLag; lag++)
            {
                float correlation = 0;

                for (int i = 0; i < buffer.Length - lag; i++)
                {
                    correlation += buffer[i] * buffer[i + lag]; // �p��۬���
                }

                autocorrelation[lag] = correlation;
            }

            int bestLag = Array.IndexOf(autocorrelation, autocorrelation.Max()); // ��X�̤j�۬����Ȫ�����

            if (autocorrelation[bestLag] > 0.01f) // �]�w�������H�ȡA�L�C�h����
            {
                return (float)sampleRate / bestLag; // �ھک���p���W�v
            }

            return 0.0f; // �W�v�������Ѯɦ^�� 0
        }

        // �ھ��W�v�έ����P�_���W
        private string GetNoteNameByCent(float frequency, out string tuningIndicator)
        {
            tuningIndicator = "��"; // �w�]�����T����
            foreach (var note in NoteFrequencies)
            {
                float baseFrequency = note.Value; // ���W������W�v
                for (int octave = 0; octave < 9; octave++)
                {
                    float diff = 1200 * (float)Math.Log(frequency / baseFrequency, 2); // �p���W�v�t�� (cent)
                    if (Math.Abs(diff) < 50) // �Y�W�v�t���p�� 50 cent
                    {
                        tuningIndicator = diff > 0 ? "��" : (diff < 0 ? "��" : "��"); // �P�_���������ΰ��C
                        return $"{note.Key}{octave}"; // �^�ǭ��W�P�K�׼�
                    }
                    baseFrequency *= 2; // ���W½�� (�U�@�ӤK��)
                }
            }
            return "�W�X�d��"; // �W�v�W�X�i�����d��
        }

        // ����W�v�P������T
        private void DisplayFrequency(float frequency, string note, string tuningIndicator)
        {
            string displayMessage = $"�W�v:   {frequency:F2} Hz    ����:   {note}   {tuningIndicator}\r\n";

            lblPitch.Invoke((Action)(() => lblPitch.Text = displayMessage));
            txtPitch.Invoke((Action)(() =>
            {
                txtPitch.AppendText(displayMessage);
                txtPitch.ScrollToCaret();
            }));
        }

        // ø�s�i�ι�
        private void DrawWaveform()
        {
            if (pictureBox1.Image == null)
            {
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            }

            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                // �M�ŵe���A�]�m�I�����զ�
                g.Clear(Color.White);

                // �]�w�Ѽ�
                int totalNotes = 88; // A0 (21) �� C8 (108)�A�@ 88 ��
                float yStep = pictureBox1.Height / (float)totalNotes; // �C�ӭ��W�����j

                int xStep = pictureBox1.Width / MaxHistoryLength; // �C�Ӯɶ������������j

                // ø�s Y �b���� (���W)
                using (Pen gridPen = new Pen(Color.LightGray, 1)) // ��׽u���L�Ǧ�
                using (Brush textBrush = new SolidBrush(Color.Black)) // ���Ҥ�r���¦�
                using (Font font = new Font("Arial", 8))
                {
                    for (int i = 0; i < totalNotes; i++)
                    {
                        string note = GetNoteNameByIndex(i); // �ھگ��ި��o���W
                        float y = pictureBox1.Height - i * yStep;

                        g.DrawLine(gridPen, 0, y, pictureBox1.Width, y); // �e�����u
                        g.DrawString(note, font, textBrush, 0, y - font.Height / 2); // ��ܭ��W����
                    }
                }

                // ø�s�i�Φ��u
                float[] pitches = pitchHistory.ToArray();
                using (Pen waveformPen = new Pen(Color.Green, 2)) // �i�ά����
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

            pictureBox1.Invalidate(); // Ĳ�o���sø�s
        }

        // ��s�����i�ά���
        private void UpdatePitchHistory(float frequency)
        {
            if (pitchHistory.Count >= MaxHistoryLength)
            {
                pitchHistory.Dequeue(); // �������ª�����
            }
            pitchHistory.Enqueue(frequency);
        }

        // �ھگ��ި��o���W (A0 �� C8)
        private string GetNoteNameByIndex(int index)
        {
            string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            int octave = (index + 9) / 12; // �p��K�׼�
            string note = notes[index % 12]; // �p�⭵�W
            return $"{note}{octave}";
        }

        // �ھ��W�v���o Y �b��m (�N�W�v�M�g�쭵�W������ Y �b��m)
        private float GetYPositionByFrequency(float frequency, int height, int totalNotes)
        {
            // �p���W�v������ MIDI ���ů���
            float midiNote = 12 * (float)Math.Log(frequency / 27.5, 2) + 21; // A0 �� 21
            if (midiNote < 21 || midiNote > 108) return -1; // �W�X�d��A��^�L�ĭ�

            // �M�g�� Y �b
            float yStep = height / (float)totalNotes;
            return height - (midiNote - 21) * yStep;
        }

        #region WinForm�ƥ�
        // �}�l���s�ƥ�
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbxMachine.SelectedIndex >= 0)
            {
                StartDetection(cbxMachine.SelectedIndex);
            }
        }

        // ������s�ƥ�
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopDetection();
        }

        // �M�����s�ƥ�
        private void btnClear_Click(object sender, EventArgs e)
        {
            lblPitch.Text = string.Empty;
            txtPitch.Clear();
            pictureBox1.Image = null;
            pitchHistory.Clear();
        }

        // ���T�]�ƭ��s��z���s�ƥ�
        private void cbxMachine_Click(object sender, EventArgs e)
        {
            InitializeAudioDevices();
        }

        // ���������ƥ� (����귽)
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopDetection();
        }
        #endregion
    }
}
