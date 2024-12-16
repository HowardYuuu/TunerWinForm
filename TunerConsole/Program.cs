using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunerConsole
{
    public class Program
    {
        static float[] prevBuffer = null;
        static BufferedWaveProvider bufferedWaveProvider = null;

        static Dictionary<string, float> noteBaseFreqs = new Dictionary<string, float>()
        {
            { "C", 16.35f }, { "C#", 17.32f }, { "D", 18.35f }, { "Eb", 19.45f },
            { "E", 20.60f }, { "F", 21.83f }, { "F#", 23.12f }, { "G", 24.50f },
            { "G#", 25.96f }, { "A", 27.50f }, { "Bb", 29.14f }, { "B", 30.87f }
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Tuner Console App!");

            int inputDevice = SelectInputDevice();
            StartDetect(inputDevice);
        }

        static int SelectInputDevice()
        {
            int inputDevice = 0;
            bool isValidChoice = false;

            do
            {
                Console.Clear();
                Console.WriteLine("請選擇收音設備:");

                for (int i = 0; i < WaveInEvent.DeviceCount; i++)
                {
                    Console.WriteLine($"{i}. {WaveInEvent.GetCapabilities(i).ProductName}");
                }

                Console.WriteLine();

                if (int.TryParse(Console.ReadLine(), out inputDevice) && inputDevice >= 0 && inputDevice < WaveInEvent.DeviceCount)
                {
                    isValidChoice = true;
                    Console.WriteLine($"已選擇 {WaveInEvent.GetCapabilities(inputDevice).ProductName}.\n");
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                }

            } while (!isValidChoice);

            return inputDevice;
        }

        static void StartDetect(int inputDevice)
        {
            WaveInEvent waveIn = new WaveInEvent
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

            IWaveProvider stream = new Wave16ToFloatProvider(bufferedWaveProvider);
            int sampleRate = stream.WaveFormat.SampleRate;

            byte[] buffer = new byte[8192];
            int bytesRead;

            Console.WriteLine("Play or sing a note! Press ESC to exit at any time. \n");

            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                float freq = DetectPitch(buffer, bytesRead / sizeof(float), sampleRate);

                if (freq != 0)
                {
                    Console.WriteLine($"Frequency: {freq:F2} Hz | Notes: {GetNote(freq)}");
                }

            } while (bytesRead != 0 && !(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape));

            waveIn.StopRecording();
            waveIn.Dispose();
        }

        static void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            bufferedWaveProvider?.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        static float DetectPitch(byte[] buffer, int frames, int sampleRate)
        {
            float[] floatBuffer = new WaveBuffer(buffer).FloatBuffer;

            if (prevBuffer == null)
            {
                prevBuffer = new float[frames];
            }

            int minFreq = 75;
            int maxFreq = 335;
            int minOffset = sampleRate / maxFreq;
            int maxOffset = sampleRate / minFreq;

            float secCor = 0;
            int secLag = 0;

            float maxCorr = 0;
            int maxLag = 0;

            for (int lag = maxOffset; lag >= minOffset; lag--)
            {
                float corr = 0;

                for (int i = 0; i < frames; i++)
                {
                    int oldIndex = i - lag;
                    float sample = (oldIndex < 0) ? prevBuffer[frames + oldIndex] : floatBuffer[oldIndex];
                    corr += (sample * floatBuffer[i]);
                }

                if (corr > maxCorr)
                {
                    maxCorr = corr;
                    maxLag = lag;
                }
                if (corr >= 0.9 * maxCorr)
                {
                    secCor = corr;
                    secLag = lag;
                }
            }
            for (int n = 0; n < frames; n++)
            {
                prevBuffer[n] = buffer[n];
            }

            float noiseThreshold = frames / 1000f;

            return (maxCorr < noiseThreshold || maxLag == 0) ? 0.0f : (float)sampleRate / maxLag;
        }

        static string GetNote(float freq)
        {
            foreach (var note in noteBaseFreqs)
            {
                float baseFreq = note.Value;

                for (int i = 0; i < 9; i++)
                {
                    if ((freq >= baseFreq - 0.5) && (freq < baseFreq + 0.5) || (freq == baseFreq))
                    {
                        return $"{note.Key}{i}";
                    }

                    baseFreq *= 2; // 遞增八度
                }
            }

            return null;
        }
    }
}
