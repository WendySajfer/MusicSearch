using System;
using System.Windows;
using System.IO;
using NAudio.Wave;
using NAudio.Lame;
using System.Diagnostics.CodeAnalysis;
using FlexibileMessageBox.FlexibileMessageBox;

namespace MusicSearch
{
    public partial class MainWindow : Window
    {
        WaveIn? waveIn; // объект для получения данных с микрофона
        WaveFileWriter? writer; // объект для записи данных в файл
        WaveOut? waveOut; // объект для воспроизведения звука
        WaveFileReader? reader; // объект для чтения данных из файла
        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists("temp.wav")) DeleteFile("temp.wav");
            //Перевод в спектрограммы необработанных данных песен
            /*string songsPath = @"songs";
            string spectroSongsPath = @"spectro_songs";
            string[] songs = Directory.GetFiles(songsPath, "*.mp3");
            string[] spectroSongs = Directory.GetFiles(spectroSongsPath, "*.*");
            var missingFiles = songs.Except(spectroSongs, new FileNameComparer());
            foreach (var file in missingFiles)
            {
                try
                {
                    SongForm song = new SongForm(System.IO.Path.GetFileNameWithoutExtension(file));
                    if (song.CreateSpectogram() == false) System.Windows.Forms.MessageBox.Show("Ошибка! Спектограмма не была создана.");
                }
                catch(Exception error)
                { 
                    System.Windows.Forms.MessageBox.Show("Ошибка! Спектограмма не была создана или ошибка." + error); }
            }*/
        }
        // Класс для сравнения файлов по имени без учета расширения
        class FileNameComparer : System.Collections.Generic.IEqualityComparer<string>
        {
            public bool Equals([AllowNull] string x, [AllowNull] string y)
            {
                return System.IO.Path.GetFileNameWithoutExtension(x)?.Equals(System.IO.Path.GetFileNameWithoutExtension(y)) ?? false;
            }

            public int GetHashCode(string obj)
            {
                return System.IO.Path.GetFileNameWithoutExtension(obj).GetHashCode();
            }
        }
        private void Exit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        // Обработчик события DataAvailable - вызывается при поступлении данных с микрофона
        public void DeleteFile(string filename)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
                reader?.Close();

            }
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.DataAvailable -= waveIn_DataAvailable;
                waveIn.Dispose();
                waveIn = null;
            }
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
            try
            {
                // Удаляем файл
                File.Delete(filename);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        void waveIn_DataAvailable([AllowNull] object? sender, WaveInEventArgs e)
        {
            writer?.Write(e.Buffer, 0, e.BytesRecorded);
        }
        public void Record(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("temp.wav")) { DeleteFile("temp.wav"); }
                // Создаем объект WaveIn для получения данных с микрофона
                waveIn = new WaveIn();
                // Устанавливаем формат звука -  44.1 кГц, 16 бит, моно
                waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
                // Добавляем обработчик события, возникающего при поступлении данных
                waveIn.DataAvailable += waveIn_DataAvailable;
                // Создаем объект WaveFileWriter для записи данных в файл
                writer = new WaveFileWriter("temp.wav", waveIn.WaveFormat);
                // Начинаем запись
                waveIn.StartRecording();
            }
            catch (Exception error) { System.Windows.MessageBox.Show("Error: " + error); return; }

        }
        public void Stop(object sender, RoutedEventArgs e)
        {
            try
            {
                if (waveIn != null)
                {
                    // Останавливаем запись
                    waveIn.StopRecording();
                    // Удаляем обработчик события
                    waveIn.DataAvailable -= waveIn_DataAvailable;
                    // Освобождаем ресурсы
                    waveIn.Dispose();
                    waveIn = null;
                }
                if (writer != null)
                {
                    // Закрываем файл
                    writer.Close();
                    writer = null;
                }
            }
            catch (Exception error) { System.Windows.MessageBox.Show("Error: " + error); return; }
        }
        public void Save(string filename)
        {
            // Проверяем, есть ли данные для сохранения
            if (File.Exists("temp.wav"))
            {
                // Конвертируем данные из wav в mp3 с помощью библиотеки NAudio.Lame
                var rdr = new WaveFileReader("temp.wav");
                var wtr = new LameMP3FileWriter(filename, rdr.WaveFormat, LAMEPreset.VBR_90);
                rdr.CopyTo(wtr);
                // Освобождаем ресурсы
                rdr.Dispose();
                wtr.Dispose();
            }
        }
        public void Play(string filename)
        {
            // Создаем объект WaveOut для воспроизведения звука
            waveOut = new WaveOut();
            // Создаем объект WaveFileReader для чтения данных из файла
            reader = new WaveFileReader(filename);
            // Устанавливаем источник данных для WaveOut
            waveOut.Init(reader);
            // Начинаем воспроизведение
            waveOut.Play();
        }
        public void SaveSound(object sender, RoutedEventArgs e)
        {
            string folderPath = @"data\records\";
            string[] files = Directory.GetFiles(folderPath, "*.mp3");
            int id = 0;
            if (files.Length > 0)
            {
                Array.Sort(files);
                string lastFile = files[files.Length - 1];
                try
                {
                    int lastId = int.Parse(System.IO.Path.GetFileNameWithoutExtension(lastFile));
                    id = lastId + 1;
                }
                catch (Exception error) { System.Windows.MessageBox.Show("Error: " + error); return; }
            }
            string filename = folderPath + id.ToString() + ".mp3";
            try { Save(filename); }
            catch (Exception error) { System.Windows.MessageBox.Show("Error: " + error); return; }
        }
        public void PlaySound(object sender, RoutedEventArgs e)
        {
            if (waveIn != null) { System.Windows.MessageBox.Show("Остановите запись для проигрывания звука."); return; }
            if (!File.Exists("temp.wav")) { System.Windows.MessageBox.Show("Запись не начата."); return; }
            try { Play("temp.wav"); }
            catch (Exception error) { System.Windows.MessageBox.Show("Error: " + error); return; }
        }
        public void CreateSpectro(object sender, RoutedEventArgs e)
        {
            string filename = Microsoft.VisualBasic.Interaction.InputBox("Введите название файла mp3:");
            if (File.Exists(@"data\records\" + filename + ".mp3"))
            {
                SongForm song = new SongForm(filename);
                if (song.Filename == null)
                {
                    System.Windows.Forms.MessageBox.Show("Ошибка! Файл не найден.");
                }
                else
                {
                    if (song.CreateSpectrogramOne()) System.Windows.Forms.MessageBox.Show("Спектограмма успешно создана.");
                    else System.Windows.Forms.MessageBox.Show("Ошибка! Спектограмма не была создана.");
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Файл не найден.");
            }
        }

        public void CreateNeuralNetwork(object sender, RoutedEventArgs e)
        {
            try 
            {
                PropertyNetwork Property = new PropertyNetwork();
                Property.ShowDialog();
                bool Flag = Property.Flag;
                int CountEpochs = Property.CountEpochs;
                Genre_network network = new Genre_network();
                string result = network.CreateGenreNetwork(Flag, CountEpochs);
                FlexibleMessageBox.Show(TestFormatCreate(result), "Result of create network.");
            }
            catch (Exception error) { FlexibleMessageBox.Show("Error: " + error, "Error of create network."); return; }
        }
        public void TrainNetwork(object sender, RoutedEventArgs e)
        {
            try
            {
                PropertyNetwork Property = new PropertyNetwork();
                Property.ShowDialog();
                bool Flag = Property.Flag;
                int CountEpochs = Property.CountEpochs;
                Genre_network network = new Genre_network();
                network.OpenModel();
                string result = network.TrainGenreNetwork(Flag, CountEpochs);
                FlexibleMessageBox.Show(TestFormatTrain(result), "Result of train network.");
            }
            catch (Exception error) { FlexibleMessageBox.Show("Error: " + error, "Error of train network."); return; }
        }
        public void WorkNeuralNetwork(object sender, RoutedEventArgs e)
        {
            try
            {
                Genre_network network = new Genre_network();
                network.OpenModel();
                string filePathMp3 = network.OpenFileMp3();
                string result = network.WorkGenreNetwork(filePathMp3);
                FlexibleMessageBox.Show(TestFormatWork(result), "Result of searh genre.");
            }
            catch (Exception error) { FlexibleMessageBox.Show("Error: " + error, "Error of searh genre."); return; }
        }
        private string TestFormatCreate(string result)
        {
            try
            {
                //string filepath = "G:\\My\\Proga\\MusicSearch\\MusicSearch\\MusicSearch\\TestFormat.txt";
                //string result = File.ReadAllText(filepath);
                if (result.StartsWith("Error"))
                {
                    return result;
                }
                else
                {
                    string[] lines = result.Split("\n");
                    string formattedResult = "";
                    string modelName_format = "";
                    string accuracy = "";
                    string genres = "";
                    string epochs = "";

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("tensorflow_keras_spectrograms_genre")) 
                        {
                            int semicolonIndex = line.IndexOf(';');
                            if (semicolonIndex != -1) { modelName_format = line.Substring(0, semicolonIndex); }
                            else { modelName_format = line; }
                        }
                        else if (line.StartsWith("Calculating"))
                        {
                            if (!genres.Contains(line))
                            {
                                genres += line + "\n";
                            }
                        }
                        else if (line.StartsWith("Accuracy"))
                        {
                            accuracy = line;
                        }
                        else if (line.StartsWith("Epoch"))
                        {
                            string[] parts = line.Split();
                            string epochNumber = parts[1];
                            epochs += String.Format("Epoch {0}\n", epochNumber);
                        }
                        else if (line.Contains("sparse_categorical_accuracy:"))
                        {
                            string line_1 = line.Replace("[==============>...............]", "");
                            line_1 = line_1.Replace("[==============================]", "");
                            epochs += line_1 + "\n";
                        }
                    }

                    formattedResult += accuracy + "\n" + modelName_format + "\n\n";
                    formattedResult += genres + "\n";
                    formattedResult += epochs + "\n";
                    formattedResult += "OK";

                    if (formattedResult == "") return result;
                    return formattedResult;
                }
            }
            catch (Exception error) { return "Error: " + error + "\nResult: " + result; }
        }
        private string TestFormatTrain(string result)
        {
            try
            {
                if (result.StartsWith("Error"))
                {
                    return result;
                }
                else
                {
                    string[] lines = result.Split("\n");
                    string formattedResult = "";
                    string genres = "";
                    string epochs = "";

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Calculating"))
                        {
                            if (!genres.Contains(line))
                            {
                                genres += line + "\n";
                            }
                        }
                        else if (line.StartsWith("Epoch"))
                        {
                            string[] parts = line.Split();
                            string epochNumber = parts[1];
                            epochs += String.Format("Epoch {0}\n", epochNumber);
                        }
                        else if (line.Contains("sparse_categorical_accuracy:"))
                        {
                            string line_1 = line.Replace("[==============>...............]", "");
                            line_1 = line_1.Replace("[=========>....................]", "");
                            line_1 = line_1.Replace("[==============================]", "");
                            epochs += line_1 + "\n";
                        }
                    }
                    formattedResult += genres + "\n";
                    formattedResult += epochs + "\n";
                    formattedResult += "OK";

                    if (formattedResult == "") return result;
                    return formattedResult;
                }
            }
            catch (Exception error) { return "Error: " + error + "\nResult: " + result; }
        }
        private string TestFormatWork(string result)
        {
            try
            {
                if (result.StartsWith("Error"))
                {
                    return result;
                }
                else
                {
                    string formattedResult = result;
                    int pos = formattedResult.IndexOf("Общий результат");
                    if (pos >= 0)
                    {
                        formattedResult = formattedResult.Substring(pos, formattedResult.Length - pos);
                    }
                    else { return result; }
                    formattedResult += "OK";

                    if (formattedResult == "") return result;
                    return formattedResult;
                }
            }
            catch (Exception error) { return "Error: " + error + "\nResult: " + result; }
        }
        public void TestFormat(object sender, RoutedEventArgs e)
        {

        }
    }
}
/*
 * Исправить ошибку с занятостью процессами мелодии Запись-запись, проиграть-запись, проиграть-сохранить
 * Добавить отчиску папки buf
 */