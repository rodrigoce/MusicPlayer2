using Id3;
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
 
        //public MusicList PlayList = new MusicList();
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
                    var music = new Music() { Nro = PlayList.Count + 1, Name = Path.GetFileNameWithoutExtension(file), NameForSearch = Path.GetFileNameWithoutExtension(file).ForSearch(), Path = file };
                    PlayList.Add(music);
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
                    item.Path + Constantes.RECORD_SEPARATOR + 
                    item.IsInFilter.ToString());
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
                        Path = separated[2],
                        IsInFilter = bool.Parse(separated[3])
                    });
                }
            }
        }

        /*
        private void GetTags(Music music)
        {
            bool tryAgain = false;
            using (var mp3 = new Mp3(music.Path, Mp3Permissions.ReadWrite))
            {
                //mp3.DeleteAllTags();
                //var tag = new Id3Tag();
                //var propinfo = typeof(Id3Tag).GetProperty("Version", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                //propinfo.SetValue(tag, Id3Version.V23);
                //tag.Artists.Value.Clear();
                //tag.Artists.Value.Add(dados.Substring(0, dados.IndexOf("-") - 1).Trim());
                //tag.Title.Value = dados.Substring(dados.IndexOf("-") + 1, dados.Length - (dados.IndexOf("-") + 1)).Trim();
                //mp3.WriteTag(tag, WriteConflictAction.Replace);

                if (mp3.HasTagOfVersion(Id3Version.V23))
                {
                    var tag = mp3.GetTag(Id3Version.V23);
                    music.Name = tag.Artists.Value[0] + " - " + tag.Title.Value;
                } 
                else if (mp3.HasTagOfVersion(Id3Version.V1X))
                {
                    var tag = mp3.GetTag(Id3Version.V1X);
                    music.Name = tag.Artists.Value[0] + " - " + tag.Title.Value;
                }
                else
                {
                    tryAgain = true;
                    
                }
            }
            
            if (tryAgain) GetByAlgorithm(music);
        }

        private void GetByAlgorithm(Music music)
        {
            byte[] b = new byte[128];
            string sTitle;
            string sSinger;
            string sAlbum;
            string sYear;
            string sComm;

            FileStream fs = new FileStream(music.Path , FileMode.Open);
            fs.Seek(-128, SeekOrigin.End);
            fs.Read(b, 0, 128);
            bool isSet = false;
            string sFlag = System.Text.Encoding.Default.GetString(b, 0, 3);
            if (sFlag.CompareTo("TAG") == 0)
            {
                isSet = true;
            }

            if (isSet)
            {
                //get   title   of   song;
                sTitle = System.Text.Encoding.Default.GetString(b, 3, 30);
                //get   singer;
                sSinger = System.Text.Encoding.Default.GetString(b, 33, 30);
                //get   album;
                sAlbum = System.Text.Encoding.Default.GetString(b, 63, 30);
                //get   Year   of   publish;
                sYear = System.Text.Encoding.Default.GetString(b, 93, 4);
                //get   Comment;
                sComm = System.Text.Encoding.Default.GetString(b, 97, 30);
                music.Name = sTitle;
            }
        }
        */

        public void Clear()
        {
            PlayList.Clear();
        }
    }
}
