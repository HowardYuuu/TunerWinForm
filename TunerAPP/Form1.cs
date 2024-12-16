using System;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;

namespace TunerAPP
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn;              // �Ω����
        private BufferedWaveProvider waveProvider; // �Ω��W�ƾڽw�s
        private BiQuadFilter lowPassFilter;
        private const int sampleRate = 44100;     // ���˲v
        private const int fftSize = 8192;         // FFT�j�p
        private const float volumeThreshold = 0.001f; // �i�ھڻݭn�վ��H��
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private const double A4Frequency = 440.0; // A4���W�v

        // �]�w�C�ӭ��������W
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

        // ��l�ƭ��W����
        private void InitializeAudioCapture()
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = 0, // �ϥιw�]���T�]��
                WaveFormat = new WaveFormat(sampleRate, 1) // ���n�D, 44.1kHz
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(1),
                DiscardOnBufferOverflow = true
            };
            waveIn.StartRecording();
            // �]�m�C�q�o�i���A�L�o��1000 Hz�H�W���W�v
            lowPassFilter = BiQuadFilter.LowPassFilter(44100, 1000, 0.707f);
        }

        // �B�z���򪺭��W�ƾ�
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            // �N���W�ƾڲK�[��w�İ�
            waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

            // �ϥ� byte[] Ū���ƾ�
            byte[] byteBuffer = new byte[fftSize * 2]; // 16-bit ���W�ƾڡA1�Ӽ˥� 2�Ӧr�`
            int samplesRead = waveProvider.Read(byteBuffer, 0, byteBuffer.Length);

            // �p�G�S��Ū���쨬�����ƾګh��^
            if (samplesRead < byteBuffer.Length) return;

            // �N byte[] �ഫ�� float[]�A�o�̰��]�O 16-bit PCM ���W�ƾ�
            float[] samples = new float[fftSize];
            for (int i = 0; i < samples.Length; i++)
            {
                // ���]�C�ӭ��W�˥��O 16-bit�A�N byte �ഫ�� float
                short sample = BitConverter.ToInt16(byteBuffer, i * 2);  // 16-bit ���W�A�C�Ӽ˥� 2 �Ӧr�`
                samples[i] = sample / 32768f; // �N�d��q [-32768, 32767] �M�g�� [-1, 1]
            }

            // �o�i�B�z�G�q�L�C�q�o�i���B�z���W�ƾ�
            for (int i = 0; i < samples.Length; i++)
            {
                // �p�G���q�p���H�ȡA�h�N�˥��]�� 0 �θ��L
                if (Math.Abs(samples[i]) < volumeThreshold)
                {
                    samples[i] = 0; // �Ϊ̸��L���˥��A�ھڻݭn�վ�
                }
                else
                {
                    // �ϥΧC�q�o�i���ﭵ�W�ƾڶi��L�o�B�z
                    samples[i] = lowPassFilter.Transform(samples[i]);
                }
            }

            // �ϥ�FFT���R���W�H�����W�v
            var fftBuffer = new Complex[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                fftBuffer[i] = new Complex { X = samples[i], Y = 0 };
            }

            // �p��FFT
            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2), fftBuffer);

            // ����W�v���q�ç�X�̤j�W�v
            var magnitudes = fftBuffer.Select(c => Math.Sqrt(c.X * c.X + c.Y * c.Y)).ToArray();
            int maxIndex = magnitudes.Skip(1).ToList().IndexOf(magnitudes.Skip(1).Max()) + 1;
            double frequency = maxIndex * (sampleRate / (double)fftSize);

            // ������d��bC0��B8
            if (frequency < 16.35 || frequency > 7902.13) return; // C0 = 16.35 Hz, B8 = 7902.13 Hz

            // ��ܭ����]�W�v�^�ι�������
            string note = FrequencyToNote(frequency, out double deviation);
            Invoke(new Action(() =>
            {
                labelFrequency.Text = $"Frequency: {frequency:F2} Hz";
                labelNote.Text = $"Note: {note} ({deviation:F2} Hz deviation)";
            }));
        }

        // �ھ��W�v�p�⭵�ŦW�٩M���t
        private string FrequencyToNote(double frequency, out double deviation)
        {
            const double A4Frequency = 440.0; // A4 ����W�v
            if (frequency <= 0)
            {
                deviation = 0;
                return "N/A"; // �L���W�v
            }

            // �p��۹�� A4 ���b����
            double semitoneOffset = 12 * Math.Log2(frequency / A4Frequency);
            int closestNoteIndex = (int)Math.Round(semitoneOffset) % 12;
            closestNoteIndex = (closestNoteIndex + 12) % 12; // �T�O���G�b 0-11 �d��

            // �p�⭵�Ū��K��
            int octave = (int)Math.Floor((semitoneOffset + 9) / 12) + 4;

            // �p��̪񭵲Ū��W�v
            double closestSemitoneOffset = Math.Round(semitoneOffset); // ����᪺�b����
            double closestFrequency = A4Frequency * Math.Pow(2, closestSemitoneOffset / 12.0);

            // �p���W�v�P�̪񭵲Ū����t
            deviation = frequency - closestFrequency;

            // �p�⭵�ŦW�٤ΤK��
            string noteName = NoteNames[closestNoteIndex];
            return $"{noteName}{octave}";
        }


        // ���������ɰ������
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            base.OnFormClosing(e);
        }
    }
}