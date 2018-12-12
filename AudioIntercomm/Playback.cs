using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioIntercomm
{
    class Playback
    {
        RawSourceWaveStream waveStream = null;
        WaveOut waveout = null;
        IWaveProvider provider = null;


        public Playback(MemoryStream stream)
        {
            provider = new RawSourceWaveStream(
                         stream, new WaveFormat(8000, 1));
            byte[] test = stream.ToArray();

            waveout = new WaveOut();
            waveout.Init(provider);
            waveout.PlaybackStopped += waveout_PlaybackStopped;
            waveout.Volume = 1.0F;
            waveout.Play();
        }

        void waveout_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void play()
        {
            if (waveout.PlaybackState == PlaybackState.Stopped)
            {
                waveout.Play();
            }
        }

        public void stop()
        {
            waveout.Stop();
        }
    }
}
