using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using Vosk;
using Whisper.net;

class Program
{
    static async Task Main(string[] args)
    {


        CancellationTokenSource cancellationTokenSource1 = new CancellationTokenSource();
        CancellationTokenSource cancellationTokenSource2 = new CancellationTokenSource();
        CancellationToken cancellationToken1 = cancellationTokenSource1.Token;
        CancellationToken cancellationToken2 = cancellationTokenSource1.Token;
        CancellationTokenSource cancellationTokenNew = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken1, cancellationToken2);
        cancellationToken1.Register(() => Console.WriteLine("工作取消 callback #1."));
        cancellationToken1.Register(() => Console.WriteLine("工作取消 callback #2."));

        //using WaveInEvent waveIn = Vosk();

        #region Whisper.net


        string outputWavPath = "temp.wav";
        WaveInEvent waveIn;
        WaveFileWriter writer;

        Console.WriteLine("========== Whisper.net 語音辨識開始 ==========");

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

            await RunWhisperAsync(outputWavPath, cancellationToken1);
        };

        Console.WriteLine("開始錄音（按 Ctrl+C 停止錄音）...");
        waveIn.StartRecording();
        Console.CancelKeyPress += (s, e) =>
        {
            waveIn.StopRecording();
            e.Cancel = true;
        };

        Console.ReadKey();
        cancellationTokenSource1.Cancel();

        while (true)
        {
        }
        #endregion

        //while (true)
        //{
        //    Console.WriteLine("請輸入文字");
        //    string input = Console.ReadLine();
        //    await GetChat(input);
        //    //Console.ReadKey();
        //    //cancellationTokenSource1.Cancel();
        //}
    }

    private static WaveInEvent Vosk()
    {
        global::Vosk.Vosk.SetLogLevel(0);


        var model = new Model("model-en");
        var waveIn = new WaveInEvent();
        waveIn.DeviceNumber = 0;
        waveIn.WaveFormat = new WaveFormat(16000, 1);
        var voskRecognizer = new VoskRecognizer(model, 16000.0f);

        Console.WriteLine("==========Start===========");

        waveIn.DataAvailable += (s, e) =>
        {
            if (voskRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                Console.WriteLine("======Final======：" + voskRecognizer.Result());
            }
            else
            {
                Console.WriteLine("... " + voskRecognizer.PartialResult());
            }
        };

        waveIn.RecordingStopped += (s, e) =>
        {
            Console.WriteLine("錄音結束");
            Console.WriteLine("Final result:");
            Console.WriteLine(voskRecognizer.FinalResult());
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

    static async Task RunWhisperAsync(string wav, CancellationToken token)
    {
        //string modelPath = "ggml-large-v2-q8_0.bin";
        //if (!File.Exists(modelPath))
        //{
        //    Console.WriteLine($"模型 '{modelPath}' 不存在，請下載後放在執行目錄");
        //    return;
        //}
        try
        {
            Console.WriteLine("載入模型並建立辨識器...");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string model = "ggml-large-v2-q8_0.bin";

            await using WhisperProcessor? processor = WhisperFactory
                .FromPath(model)
                .CreateBuilder()
                .WithLanguage("zh")
                .Build();

            Console.WriteLine("開始辨識...");
            using FileStream? fileStream = File.OpenRead(wav);
            await foreach (var result in processor.ProcessAsync(fileStream, token))
            {
                Console.WriteLine($"{result.Start} → {result.End} ：{result.Text}");
            }

            Console.WriteLine("辨識完成");

            stopWatch.Stop();
            Console.WriteLine($"總耗時:{stopWatch.ElapsedMilliseconds}");
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e.Message);
        }

        //Thread.CurrentThread.Abort();
    }



    static async Task RunGPUWhisperAsync(string wav)
    {
        Console.WriteLine("載入模型並建立辨識器...");

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        string model = "ggml-large-v2-q8_0.bin";

        // ✅ 這裡指定使用 GPU CUDA runtime
        var runtime = new WhisperFactoryOptions()
        {
            UseGpu = true, // 啟用 GPU
            GpuDevice = 0, // 使用第一個 GPU 設備
            DelayInitialization = false // 不延遲初始化
        };
        var factory = WhisperFactory.FromPath(model, runtime);

        await using var processor = factory
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
        Console.WriteLine($"總耗時: {stopWatch.ElapsedMilliseconds} 毫秒");
    }

    static async Task GetChat(string input, CancellationToken token = default)
    {
        try
        {
            string apiKey = "";
            Console.OutputEncoding = Encoding.UTF8;
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var content = new
            {
                messages = new[]
                {
                new { role = "system", content = $"根據用戶輸入的國家三碼來給予該國家語言文字回答，推薦的前三個ASUS電腦的EDM電子單宣傳主旨，要給固定格式：三個主旨用,隔開且用[]包起來，不用其他任何說明"},
                new { role = "user", content = $"國家三碼：{input}" }
            }
            };

            HttpResponseMessage response = await http.PostAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"), token
            );

            string json = await response.Content.ReadAsStringAsync();
            string resContent = ((JObject)JsonConvert.DeserializeObject(json))["choices"][0]["message"]["content"]?.ToString();
            Console.WriteLine(resContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
