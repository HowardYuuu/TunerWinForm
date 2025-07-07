using NAudio.Wave;
using System.Diagnostics;
using Vosk;
using Whisper.net;

class Program
{
    static void Main(string[] args)
    {
        //using WaveInEvent waveIn = Vosk();

        string outputWavPath = "temp.wav";
        WaveInEvent waveIn;
        WaveFileWriter writer;


        Console.WriteLine("========== Whisper.net 語音辨識開始 ==========");

        // 初始化錄音
        waveIn = new WaveInEvent();
        waveIn.DeviceNumber = 0;
        waveIn.WaveFormat = new WaveFormat(16000, 1);
        writer = new WaveFileWriter(outputWavPath, waveIn.WaveFormat);

        waveIn.DataAvailable += (s, e) =>
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        };

        waveIn.RecordingStopped += async (s, e) =>
        {
            writer.Dispose();
            waveIn.Dispose();
            Console.WriteLine("錄音結束，開始辨識...");

            await RunWhisperAsync(outputWavPath);
        };

        Console.WriteLine("請開始說話（按 Ctrl+C 停止錄音）...");
        waveIn.StartRecording();

        Console.CancelKeyPress += (s, e) =>
        {
            waveIn.StopRecording();
            e.Cancel = true;
        };

        while (true) { } // 保持程式執行
    }

    private static WaveInEvent Vosk()
    {
        global::Vosk.Vosk.SetLogLevel(0);
        //var model = new Model("model-cn");
        var model = new Model("model-en");
        var waveIn = new WaveInEvent();
        waveIn.DeviceNumber = 0;
        waveIn.WaveFormat = new WaveFormat(16000, 1);
        var recognizer = new VoskRecognizer(model, 16000.0f);

        Console.WriteLine("==========Start===========");

        waveIn.DataAvailable += (s, e) =>
        {
            if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                Console.WriteLine("======Final======：" + recognizer.Result());
            }
            else
            {
                Console.WriteLine("... " + recognizer.PartialResult());
            }
        };

        waveIn.RecordingStopped += (s, e) =>
        {
            Console.WriteLine("錄音結束");
            Console.WriteLine("Final result:");
            Console.WriteLine(recognizer.FinalResult());
        };

        waveIn.StartRecording();

        Console.CancelKeyPress += (s, e) =>
        {
            waveIn.StopRecording();
            e.Cancel = true;
        };

        while (true) { } // 保持執行狀態

        return waveIn;
    }

    static async Task RunWhisperAsync(string wav)
    {
        //string modelPath = "ggml-large-v2-q8_0.bin";
        //if (!File.Exists(modelPath))
        //{
        //    Console.WriteLine($"模型 '{modelPath}' 不存在，請下載後放在執行目錄");
        //    return;
        //}

        Console.WriteLine("載入模型並建立辨識器...");

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        string model = "ggml-large-v2-q8_0.bin";

        await using var processor = WhisperFactory
            .FromPath(model)
            .CreateBuilder()
            .WithLanguage("zh")
            .Build();

        Console.WriteLine("開始辨識...");
        using var fileStream = File.OpenRead(wav);
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            Console.WriteLine($"{result.Start} → {result.End} ：{result.Text}");
        }
        
        Console.WriteLine("辨識完成");
        
        stopWatch.Stop();
        Console.WriteLine($"總耗時:{stopWatch.ElapsedMilliseconds}");


    }
}
