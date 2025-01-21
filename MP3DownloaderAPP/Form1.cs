using YoutubeExplode;
using NAudio.Wave;
using NAudio.Lame;
using YoutubeExplode.Videos.Streams;


namespace MP3DownloaderAPP
{
    public partial class Form1 : Form
    {
        YoutubeClient _youtube;
        AudioHelper _audioHelper;
        private string downloadFolderPath = "";
        public Form1()
        {
            InitializeComponent();
        }
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            #region 下載前檢查
            if (String.IsNullOrWhiteSpace(downloadFolderPath))
            {
                MessageBox.Show("尚未選擇下載資料夾");
                return;
            }
            if (listUrl.Items.Count == 0)
            {
                MessageBox.Show("ListUrl 中沒有任何 URL！");
                return;
            }
            if (listStatus.Items.Count > 0)
            {
                listStatus.Items.Clear();
            }
            #endregion
            try
            {
                _youtube = new YoutubeClient();
                _audioHelper = new AudioHelper();
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤：" + ex.Message);
            }
            foreach (var item in listUrl.Items)
            {
                await DownloadMP3(item.ToString());
            }

            MessageBox.Show("全部下載完成！");
            listUrl.Items.Clear();

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show("請輸入 URL！");
                return;
            }
            listUrl.Items.Add(txtUrl.Text);
            txtUrl.Text = "";
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            // 讓用戶選擇下載資料夾
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "選擇下載資料夾";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    downloadFolderPath = folderDialog.SelectedPath;
                    lblPath.Text = downloadFolderPath;
                }
                else
                {
                    MessageBox.Show("未選擇下載資料夾，操作已取消。");
                    return;
                }
            }
        }

        public async Task DownloadMP3(string videoUrl)
        {
            try
            {
                // 初始化 YouTube 客戶端
                var video = await _youtube.Videos.GetAsync(videoUrl);
                string fileName = video.Title; // 取得影片標題
                string outputMp3Path = $@"{downloadFolderPath}\{fileName}.mp3";


                // 取得影片的音訊流資訊
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                // 暫存的音訊檔案路徑
                string tempAudioPath = "temp_audio.m4a";

                // 下載音訊流
                listStatus.Items.Add($"正在下載 {fileName} 音訊流...");
                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempAudioPath);

                // 轉換為 MP3
                listStatus.Items.Add($"正在轉換為 {fileName} MP3...");
                _audioHelper.ConvertToMp3(tempAudioPath, outputMp3Path);

                // 刪除暫存檔案
                File.Delete(tempAudioPath);

                listStatus.Items.Add($"下載完成！MP3 檔案位置：{outputMp3Path}");

                if (listUrl.Items.Count > 15)
                {
                    await Task.Delay(3000);
                }
                else
                {
                    await Task.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤：" + ex.Message);
            }
        }


    }
}
