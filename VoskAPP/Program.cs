using NAudio.CoreAudioApi;
using NAudio.Wave;
using Vosk;

class Program
{
    static void Main(string[] args)
    {
        Vosk.Vosk.SetLogLevel(0);

        var model = new Model("model"); // 中文模型
        var recognizer = new VoskRecognizer(model, 16000.0f);

        // 使用 WasapiCapture，支援任意設備格式
        var capture = new WasapiCapture();
        capture.ShareMode = AudioClientShareMode.Shared;
        capture.WaveFormat = capture.WaveFormat; // 自動偵測

        for (int i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var info = WaveInEvent.GetCapabilities(i);
            Console.WriteLine($"Device {i}: {info.ProductName}");
        }

        var buffer = new BufferedWaveProvider(capture.WaveFormat);
        capture.DataAvailable += (s, e) =>
        {
            Console.WriteLine($"錄到 {e.BytesRecorded} bytes");
            buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
        };

        Console.WriteLine("🎙️ 開始錄音中，請開始說話...");

        capture.StartRecording();

        // 設定轉換器：將 48000Hz/2聲道 ➜ 16000Hz/單聲道
        var resampler = new MediaFoundationResampler(buffer, new WaveFormat(16000, 1));
        resampler.ResamplerQuality = 60;

        byte[] audioBuffer = new byte[4096];
        while (true)
        {
            int bytesRead = resampler.Read(audioBuffer, 0, audioBuffer.Length);
            if (bytesRead > 0)
            {
                if (recognizer.AcceptWaveform(audioBuffer, bytesRead))
                {
                    Console.WriteLine("✅ " + recognizer.Result());
                }
                else
                {
                    // 不顯示 Partial 避免洗版
                    // Console.WriteLine("... " + recognizer.PartialResult());
                }
            }
        }
    }
}
