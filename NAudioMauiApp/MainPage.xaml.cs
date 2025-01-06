using NAudio;
using NAudioMauiApp.Services;

namespace NAudioMauiApp
{
    public partial class MainPage : ContentPage
    {
        int _errorCount = 0;
        int _octave;
        float _freq;
        float _finalFreq;
        PlayAudioService _playAudioService = new PlayAudioService();
        public Command PlayCommand { get; set; }

        public MainPage()
        {
            InitializeComponent();
            PlayCommand = new Command(OnAnswerClicked);
        }



        private void OnStartClicked(object sender, EventArgs e)
        {
            _finalFreq = getRNGFreq();
            _playAudioService.PlayGame(_finalFreq);
        }

        private void OnAnswerClicked(object parameter)
        {
            try
            {
                if (parameter == null)
                {
                    return;
                }
                if ((float)parameter * _octave != _finalFreq)
                {
                    if (_errorCount == 3)
                    {
                        _errorCount = 0;
                        return;
                    }
                    else
                    {
                        _errorCount++;
                        DisplayAlert("答錯", $"剩餘 {3 - _errorCount} 次機會", "OK");
                    }
                }
                if ((float)parameter * _octave == _finalFreq)
                {
                    DisplayAlert("答對","", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("錯誤訊息：", ex.Message, "OK");
            }

        }

        public float getRNGFreq()
        {
            Random rngOctave = new Random();
            Random rngBaseFreq = new Random();

            List<float> baseFreq = new List<float>
            {
                16.352f,17.324f,18.354f,
                19.445f,20.602f,21.827f,
                23.125f,24.500f,25.957f,
                27.500f,29.135f,30.868f
            };
            int baseFreqIndex = rngBaseFreq.Next(baseFreq.Count);
            _octave = rngOctave.Next(0, 8);
            _freq = (float)baseFreq[baseFreqIndex];

            return _freq * _octave;
        }
    }

}
