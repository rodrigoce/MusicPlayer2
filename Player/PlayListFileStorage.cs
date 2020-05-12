using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Music
    {
        public int Nro { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string NameForSearch { get; set; }
    }
    public class PlayListFileStorage
    {
        private const char RECORD_SEPARATOR = (char)30;

        private const string STORE_FILENAME = "PlayList.txt";

        public List<Music> PlayList = new List<Music>();

        public void GetMusicRecursive(string rootFolder)
        {
            var files = Directory.GetFiles(rootFolder); //await rootFolder.GetFilesAsync();
            foreach (var file in files)
            {
                if (file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    PlayList.Add(new Music() { Nro = PlayList.Count + 1, Name = Path.GetFileNameWithoutExtension(file), NameForSearch = Path.GetFileNameWithoutExtension(file).ForSearch(), Path = file });
                }
            }

            var folders = Directory.GetDirectories(rootFolder); //await rootFolder.GetFoldersAsync();

            foreach (var folder in folders)
            {
                GetMusicRecursive(folder);
            }
        }

        private List<string> ListToLines()
        {
            var sl = new List<string>();
            foreach (var item in PlayList)
            {
                sl.Add(item.Name + RECORD_SEPARATOR +
                    item.NameForSearch + RECORD_SEPARATOR +
                    item.Path);
            } 

            return sl;
        }

        public void Save()
        {
            File.WriteAllLines(Path.GetTempPath() + STORE_FILENAME, ListToLines());
        }

        public void Load()
        {

            string[] f = null;
            bool flag = true;

            try
            {
                f = File.ReadAllLines(Path.GetTempPath() + STORE_FILENAME);
            }
            catch 
            {
                flag = false;
            }
            
            if (flag)
            {
                foreach (var item in f)
                {
                    var separated = item.Split(RECORD_SEPARATOR);
                    PlayList.Add(new Music()
                    {
                        Nro = PlayList.Count + 1,
                        Name = separated[0],
                        NameForSearch = separated[1],
                        Path = separated[2]
                    });
                }
            }
        }
    }
}
