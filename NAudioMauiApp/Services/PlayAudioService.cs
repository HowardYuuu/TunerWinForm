using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NAudioMauiApp.Services
{
    public class PlayAudioService
    {
        private int _octive;
        private IWavePlayer _waveOut;
        private SignalGenerator _signalGenerator;

        public void PlayGame(float frequency)
        {
            _waveOut = new WaveOutEvent
            {
                DeviceNumber = 0,
            };
            _signalGenerator = new SignalGenerator
            {
                Gain = 0.2,
                Frequency = frequency,
                Type = SignalGeneratorType.Sin
            };

            _waveOut.Init(_signalGenerator);
            _waveOut.Play();
        }

        public void Stop()
        {
            _waveOut?.Stop();
        }


    }
}
