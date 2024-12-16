using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TunerAPP_V2
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn;
        private BufferedWaveProvider bufferedWaveProvider;
        private float[] previousBuffer;
        private const int SampleRate = 44100;
        private const int MinimumFrequency = 27;
        private const int MaximumFrequency = 4200;

        private static readonly Dictionary<string, float> NoteFrequencies = new Dictionary<string, float>
        {
            { "C", 16.35f }, { "C#", 17.32f }, { "D", 18.35f }, { "Eb", 19.45f },
            { "E", 20.60f }, { "F", 21.83f }, { "F#", 23.12f }, { "G", 24.50f },
            { "G#", 25.96f }, { "A", 27.50f }, { "Bb", 29.14f }, { "B", 30.87f }
        };

        public Form1()
        {
            InitializeComponent();
            InitializeAudioDevices();
        }

        private void InitializeAudioDevices()
        {
            cbxMachine.Items.Clear();
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                cbxMachine.Items.Add(WaveInEvent.GetCapabilities(i).ProductName);
            }

            if (cbxMachine.Items.Count > 0)
            {
                cbxMachine.SelectedIndex = 0;
            }
        }

        private void StartDetection(int deviceIndex)
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex,
                WaveFormat = new WaveFormat(SampleRate, 1)
            };

            waveIn.DataAvailable += OnDataAvailable;
            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            waveIn.StartRecording();
        }

        private void StopDetection()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

            float[] floatBuffer = ConvertToFloatArray(e.Buffer, e.BytesRecorded);
            float detectedFrequency = DetectFrequency(floatBuffer, SampleRate);

            if (detectedFrequency >= MinimumFrequency && detectedFrequency <= MaximumFrequency)
            {
                string tuningIndicator;
                string note = GetNoteName(detectedFrequency, out tuningIndicator);
                DisplayFrequency(detectedFrequency, note, tuningIndicator);
            }
        }

        private float[] ConvertToFloatArray(byte[] buffer, int bytesRecorded)
        {
            int samples = bytesRecorded / sizeof(short);
            float[] floatBuffer = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * sizeof(short));
                floatBuffer[i] = sample / 32768f;
            }

            return floatBuffer;
        }

        private float DetectFrequency(float[] buffer, int sampleRate)
        {
            if (previousBuffer == null)
            {
                previousBuffer = new float[buffer.Length];
            }

            int minLag = sampleRate / MaximumFrequency;
            int maxLag = sampleRate / MinimumFrequency;

            float[] autocorrelation = new float[maxLag];

            for (int lag = minLag; lag < maxLag; lag++)
            {
                float correlation = 0;

                for (int i = 0; i < buffer.Length - lag; i++)
                {
                    correlation += buffer[i] * buffer[i + lag];
                }

                autocorrelation[lag] = correlation;
            }

            int bestLag = Array.IndexOf(autocorrelation, autocorrelation.Max());

            if (autocorrelation[bestLag] > 0.01f)
            {
                return (float)sampleRate / bestLag;
            }

            return 0.0f;
        }

        private string GetNoteName(float frequency, out string tuningIndicator)
        {
            tuningIndicator = "●";
            foreach (var note in NoteFrequencies)
            {
                float baseFrequency = note.Value;
                for (int octave = 0; octave < 9; octave++)
                {
                    float diff = 1200 * (float)Math.Log(frequency / baseFrequency, 2);
                    if (Math.Abs(diff) < 50)
                    {
                        tuningIndicator = diff > 0 ? "↑" : (diff < 0 ? "↓" : "●");
                        return $"{note.Key}{octave}";
                    }
                    baseFrequency *= 2;
                }
            }
            return "超出範圍";
        }

        private void DisplayFrequency(float frequency, string note, string tuningIndicator)
        {
            string displayMessage = $"頻率: {frequency:F2} Hz 音高: {note} {tuningIndicator}\r\n";

            lblPitch.Invoke((Action)(() => lblPitch.Text = displayMessage));
            txtPitch.Invoke((Action)(() =>
            {
                txtPitch.AppendText(displayMessage);
                txtPitch.ScrollToCaret();
            }));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbxMachine.SelectedIndex >= 0)
            {
                StartDetection(cbxMachine.SelectedIndex);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopDetection();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lblPitch.Text = string.Empty;
            txtPitch.Clear();
        }

        private void cbxMachine_Click(object sender, EventArgs e)
        {
            InitializeAudioDevices();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopDetection();
        }
    }
}
