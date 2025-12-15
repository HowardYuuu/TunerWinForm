using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using NAudio.Wave;
using NAudio.Lame;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        // 要下載的 YouTube 影片 URL
        string videoUrl = "https://www.youtube.com/watch?v=K9CbHZLXPIA";
        string fileName = "";
        // 輸出 MP3 檔案名稱
        string outputMp3Path = $@"D:\Downloads\{fileName}.mp3";

        try
        {
            // 初始化 YouTube 客戶端
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);
            fileName = video.Title; // 取得影片標題

            // 取得影片的音訊流資訊
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // 暫存的音訊檔案路徑
            string tempAudioPath = "temp_audio.m4a";

            // 下載音訊流
            Console.WriteLine("正在下載音訊流...");
            await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempAudioPath);

            // 轉換為 MP3
            Console.WriteLine("正在轉換為 MP3...");
            ConvertToMp3(tempAudioPath, outputMp3Path);

            // 刪除暫存檔案
            File.Delete(tempAudioPath);

            Console.WriteLine($"下載完成！MP3 檔案位置：{outputMp3Path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("發生錯誤：" + ex.Message);
        }
    }

    // 將 M4A 轉換為 MP3
    private static void ConvertToMp3(string inputPath, string outputPath)
    {
        using var reader = new AudioFileReader(inputPath);
        using var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, LAMEPreset.STANDARD);
        reader.CopyTo(writer);
    }
}
