using YoutubeExplode;
using NAudio.Wave;
using NAudio.Lame;
using YoutubeExplode.Videos.Streams;
using System.Windows.Forms.Design;


namespace MP3DownloaderAPP
{
    public partial class Form1 : Form
    {
        YoutubeClient _youtube = new YoutubeClient();
        AudioHelper _audioHelper = new AudioHelper();
        UrlHelper _urlHelper = new UrlHelper();
        private string _fileName = "";
        private string _downloadFolderPath = "";
        public Form1()
        {
            InitializeComponent();
        }
        #region [Controls]
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            #region 下載驗證
            if (String.IsNullOrWhiteSpace(_downloadFolderPath))
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

            for (int i = listUrl.Items.Count - 1; i >= 0; i--)
            {
                var item = listUrl.Items[i];
                int count = 0;
                bool isDownload = false;
                // 重試邏輯
                while (count < 2)
                {
                    isDownload = await DownloadMP3(item.ToString());
                    if (isDownload)
                    {
                        break;
                    }
                    count++;
                }
                if (isDownload)
                {
                    listUrl.Items.RemoveAt(i); // 移除當前項目
                }
                else
                {
                    listStatus.Items.Add($"{_fileName} 下載失敗！");
                    listStatus.Items.Add("======================================");
                }
            }

            MessageBox.Show($"下載結束，剩餘：{listUrl.Items.Count} 項未下載！");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            #region URL驗證
            if (String.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show("請輸入 URL！");
                return;
            }
            if (!txtUrl.Text.Contains("youtube.com") || !_urlHelper.IsValidUrl(txtUrl.Text))
            {
                MessageBox.Show("URL 格式不正確！必須是 YouTube 影片 URL！");
                return;
            }
            #endregion
            listUrl.Items.Add(txtUrl.Text);
            txtUrl.Text = "";
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            getDownloadPath();
        }
        #endregion

        #region [Functions]
        public async Task<bool> DownloadMP3(string videoUrl)
        {
            try
            {
                // 初始化 YouTube 客戶端
                var video = await _youtube.Videos.GetAsync(videoUrl);
                _fileName = video.Title; // 取得影片標題
                string outputMp3Path = $@"{_downloadFolderPath}\{_fileName}.mp3";


                // 取得影片的音訊流資訊
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                // 暫存的音訊檔案路徑
                string tempAudioPath = "temp_audio.m4a";

                // 下載音訊流
                listStatus.Items.Add($"正在下載 {_fileName} 音訊流...");
                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempAudioPath);

                // 轉換為 MP3
                listStatus.Items.Add($"正在轉換為 {_fileName} MP3...");
                _audioHelper.ConvertToMp3(tempAudioPath, outputMp3Path);

                // 刪除暫存檔案
                File.Delete(tempAudioPath);

                listStatus.Items.Add($"下載完成！MP3 檔案位置：{outputMp3Path}");
                listStatus.Items.Add("======================================");
                if (listUrl.Items.Count > 15)
                {
                    await Task.Delay(3000);
                }
                else
                {
                    await Task.Delay(5000);
                }
                return true;
            }
            catch (Exception ex)
            {
                listStatus.Items.Add($"{_fileName}發生錯誤：" + ex.Message);
                listStatus.Items.Add("再重新下載一次");
                listStatus.Items.Add("======================================");
                return false;
            }
        }
        public void getDownloadPath()
        {
            // 讓用戶選擇下載資料夾
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "選擇下載資料夾";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    _downloadFolderPath = folderDialog.SelectedPath;
                    lblPath.Text = _downloadFolderPath;
                }
                else
                {
                    return;
                }
            }
        }
        #endregion
    }
}
