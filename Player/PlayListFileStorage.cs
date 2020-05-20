using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer2
{
    public class PlayListFileStorage
    {
 
        public List<Music> PlayList = new List<Music>();

        public void GetMusicRecursive(string rootFolder)
        {
            var files = Directory.GetFiles(rootFolder);

            AddFiles(files);

            var folders = Directory.GetDirectories(rootFolder); 

            foreach (var folder in folders)
            {
                GetMusicRecursive(folder);
            }
        }

        public void AddFiles(string [] files)
        {
            foreach (var file in files)
            {
                if (file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    PlayList.Add(new Music() { Nro = PlayList.Count + 1, Name = Path.GetFileNameWithoutExtension(file), NameForSearch = Path.GetFileNameWithoutExtension(file).ForSearch(), Path = file });
                }
            }
        }

        private List<string> ListToLines()
        {
            var sl = new List<string>();
            foreach (var item in PlayList)
            {
                sl.Add(item.Name + Constantes.RECORD_SEPARATOR +
                    item.NameForSearch + Constantes.RECORD_SEPARATOR +
                    item.Path);
            } 

            return sl;
        }

        public void Save()
        {
            File.WriteAllLines(Path.GetTempPath() + Constantes.STORE_FILENAME, ListToLines());
        }

        public void Load()
        {

            string[] f = null;
            bool flag = true;

            try
            {
                f = File.ReadAllLines(Path.GetTempPath() + Constantes.STORE_FILENAME);
            }
            catch 
            {
                flag = false;
            }
            
            if (flag)
            {
                foreach (var item in f)
                {
                    var separated = item.Split(Constantes.RECORD_SEPARATOR);
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

        public void Clear()
        {
            PlayList.Clear();
        }
    }
}
