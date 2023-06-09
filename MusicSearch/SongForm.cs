using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NAudio.Wave;
using Spectrogram;
using System.Drawing;

namespace MusicSearch
{
    internal class SongForm
    { 
        //private double[]? values;
        private string? file;
        private string? title;
        private string? artist;

        public SongForm(string filename)
        {
            if (File.Exists(@"data\records\" + filename + ".mp3"))
            {
                file = filename;
                var tag = TagLib.File.Create(@"data\records\" + filename + ".mp3").Tag;
                title = tag.Title;
                artist = tag.FirstPerformer;
                //ReadMp3File(file);
            }
        }
        (double[] audio, int sampleRate) ReadWavMono(string filePath, double multiplier = 16_000)
        {
            using var afr = new AudioFileReader(filePath);
            int sampleRate = afr.WaveFormat.SampleRate;
            int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            int sampleCount = (int)(afr.Length / bytesPerSample);
            int channelCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
                audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate);
        }

        private void Mp3ToSpectrogram(string dir)
        {
            string wavFile = file + ".wav";
            //
            var audioLength = 180 * 44100; // длина аудио в сэмплах
            var width = 224; // желаемая ширина спектрограммы в пикселях
            var stepSize = (audioLength - 4096) / (width - 1); // шаг окна в сэмплах
            // Конвертируем mp3 в wav с помощью библиотеки NAudio
            string dir2;
            if (dir == "spectrograms") dir2 = @"data\records\";
            else dir2 = "songs";
            using (var mp3Reader = new Mp3FileReader(dir2 + @"\" + file + ".mp3"))
            using (var wavWriter = new WaveFileWriter(wavFile, mp3Reader.WaveFormat))
            {
                mp3Reader.CopyTo(wavWriter);
            }
            (double[] audio, int sampleRate) = ReadWavMono(file + ".wav");
            var sg = new SpectrogramGenerator(sampleRate, fftSize: 4096, stepSize: stepSize, maxFreq: 3000, fixedWidth: 224);
            sg.Add(audio);
            sg.SaveImage(@"buf\" + file + ".jpg");
            File.Delete(wavFile);
            TreatSpectrogram(dir);
        }
        private void TreatSpectrogram(string dir)
        {
            Bitmap bmp = new Bitmap(@"buf\" + file + ".jpg");
            Bitmap resized = new Bitmap(bmp, new Size(224, 224));
            resized.Save(dir + @"\" + file + ".jpg");
        }
        public bool CreateSpectogram()
        {
            if(file == null) return false;
            Mp3ToSpectrogram("spectro_songs");
            return true;
        }
        public bool CreateSpectrogramOne()
        {
            if (file == null) return false;
            Mp3ToSpectrogram("spectrograms");
            return true;
        }

        public string? Filename { get => file; 
            set { 
                if (value != null && File.Exists(@"songs\" + value + ".mp3"))
                { file = value; 
                    var tag = TagLib.File.Create(@"songs\" + file + ".mp3").Tag;
                    title = tag.Title;artist = tag.FirstPerformer;/*ReadMp3File(file);*/}
                else { file = null; } } }
        public string? Title { get => title; }
        public string? Artist { get => artist; }
    }
}
