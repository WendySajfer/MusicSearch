using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Python.Runtime;
using System.Diagnostics;
using System.Windows.Shapes;
using Path = System.IO.Path;


namespace MusicSearch
{
    class Genre_network
    {
        private static readonly string basePath = Path.GetFullPath(Path.Combine("..", "..", "..", ".."));
        private static readonly Env env = new Env();
        private string pythonPath = env.PythonPath;
        private string ModelName = "tensorflow_keras_spectrograms_genre_0";
        private string DirModel = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "models"), basePath);
        private string PathModel;
        //private string DirRecords = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "records"), basePath);
        private string DirCreateSongs = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "songs_for_create_network"), basePath);
        private string DirCreateSongsWav = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "songs_for_create_network_wav"), basePath);
        private string DirLearningSongs = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "songs_for_learning"), basePath);
        private string DirLearningSongsWav = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "songs_for_learning_wav"), basePath);
        private string DirTestSongs = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "songs_for_test"), basePath);
        private string DirSpectroView = Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "spectro_view"), basePath);
        private string[] Genres = new string[] { };

        public Genre_network()
        {
            Genres = File.ReadAllLines(Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "genres.txt"), basePath));
            PathModel = Path.Combine(DirModel, ModelName);
        }
        public Genre_network(string dirМodel)
        {
            Genres = File.ReadAllLines(Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "genres.txt"), basePath));
            bool dirExists = Directory.Exists(dirМodel);
            if(!dirExists) throw new Exception("Error! Invalid model path.");
            ModelName = new DirectoryInfo(dirМodel).Name;
            DirModel = Path.GetDirectoryName(dirМodel)!;
            PathModel = dirМodel;
        }

        public string CreateGenreNetwork(bool Flag, int CountEpochs)
        {
            try
            {
                string scriptPath = Path.GetFullPath(Path.Combine("MusicSearch", "PythonNetwork", "create_network_def.py"), basePath);

                string flag = Flag == true ? "True" : "";

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonPath;
                // Format the arguments string with the script path and the arguments array
                start.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\"", scriptPath, flag, DirCreateSongs, DirCreateSongsWav, string.Join(",", Genres), CountEpochs);
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;

                // Start the process and read the output
                using (Process process = Process.Start(start)!)
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string TrainGenreNetwork(bool Flag, int CountEpochs)
        {
            try
            {
                string scriptPath = Path.GetFullPath(Path.Combine("MusicSearch", "PythonNetwork", "train_network_def.py"), basePath);

                string flag = Flag == true ? "True" : "";

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonPath;
                // Format the arguments string with the script path and the arguments array
                start.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\" \"{6}\"", scriptPath, flag, PathModel, DirLearningSongs, DirLearningSongsWav, string.Join(",", Genres), CountEpochs);
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;

                // Start the process and read the output
                using (Process process = Process.Start(start)!)
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string WorkGenreNetwork(string PathWorkSong)
        {
            try
            {
                string scriptPath = Path.GetFullPath(Path.Combine("MusicSearch", "PythonNetwork", "work_network_def.py"), basePath);

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonPath;
                // Format the arguments string with the script path and the arguments array
                start.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", scriptPath, PathModel, PathWorkSong, string.Join(",", Genres));
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;

                // Start the process and read the output
                using (Process process = Process.Start(start)!)
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string OpenFileMp3()
        {
            string filePathMp3;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            openFileDialog.InitialDirectory = DirTestSongs;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) { filePathMp3 = openFileDialog.FileName; }
            else throw new Exception("Error! OpenFileDialog not DialogResult.OK.");
            
            return filePathMp3;
        }
        public string OpenModel()
        {
            //проверим существование модели по .h5
            string filePathModel;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "H5 files (*.h5)|*.h5";
            openFileDialog.InitialDirectory = DirModel;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) { filePathModel = openFileDialog.FileName; }
            else throw new Exception("Error! OpenFileDialog not DialogResult.OK.");

            filePathModel = Path.ChangeExtension(filePathModel, null);

            if (!Directory.Exists(filePathModel))
            {
                throw new Exception("Error! Model directory not exists.");
            }
            if (!File.Exists(Path.Combine(filePathModel, "saved_model.pb")))
            {
                throw new Exception("Error! Model pb not exists.");
            }
            dirModel = filePathModel;

            return filePathModel;
        }
        public void GenresUpdate()
        {
            Genres = File.ReadAllLines(Path.GetFullPath(Path.Combine("MusicSearch", "bin", "Debug", "net6.0-windows", "data", "genres.txt")));
        }
        public string dirModel
        {
            get => PathModel;
            set
            {
                bool dirExists = Directory.Exists(value);
                if (!dirExists) throw new Exception("Error! Invalid model path.");
                ModelName = new DirectoryInfo(value).Name;
                DirModel = Path.GetDirectoryName(value)!;
                PathModel = value;
            }
        }
        public string[] GenresList {
            get => Genres;
            set
            {
                Genres = value;
            }
        }
    }
}
