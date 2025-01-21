using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3DownloaderAPP
{
    public class AudioHelper
    {
        // 將 M4A 轉換為 MP3
        public void ConvertToMp3(string inputPath, string outputPath)
        {
            using var reader = new AudioFileReader(inputPath);
            using var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, LAMEPreset.STANDARD);
            reader.CopyTo(writer);
        }
    }
}
