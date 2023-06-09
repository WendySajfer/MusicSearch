
namespace MusicSearch
{
    class Env
    {
        private string pythonPath = @"C:\Users\kseni\AppData\Local\Programs\Python\Python310\python.exe";
        public string PythonPath
        {
            get => pythonPath;
            set
            {
                pythonPath = value;
            }
        }
    }
}
