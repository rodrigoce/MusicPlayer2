using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MusicPlayer2
{
    public class PlayList
    {
        public PlayList(MediaElement mediaElement, System.Windows.Controls.ListBox listBoxMusics)
        {
            this.mediaElement = mediaElement;
            this.listBoxMusics = listBoxMusics;
            playListFileStorage = new PlayListFileStorage();
        }

        private readonly PlayListFileStorage playListFileStorage;
        private readonly MediaElement mediaElement;
        private readonly System.Windows.Controls.ListBox listBoxMusics;
        private Music currentMusic;

        private int GetLastPlayListIndex()
        {
            int last = playListFileStorage.PlayList.Count;
           
            return last;
        }
        
        public void AddFolder()
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int last = GetLastPlayListIndex();

                playListFileStorage.GetMusicRecursive(fd.SelectedPath);
                LoadToListBoxMusics(last);
                playListFileStorage.Save();
            }
        }
        public void AddFiles()
        {
            var od = new OpenFileDialog();
            od.Filter = "MP3 files (*.MP3)|*.MP3";
            od.Multiselect = true;

            if (od.ShowDialog() == DialogResult.OK)
            {
                int last = GetLastPlayListIndex();
                playListFileStorage.AddFiles(od.FileNames);
                LoadToListBoxMusics(last);
                playListFileStorage.Save();
            }
        }

        private void LoadToListBoxMusics(int startFrom)
        {
            for (int i = startFrom; i < playListFileStorage.PlayList.Count; i++)
            {
                var music = playListFileStorage.PlayList[i];
                listBoxMusics.Items.Add(music.Nro + " - " + music.Name);
                music.ItemOnListBox = listBoxMusics.Items[listBoxMusics.Items.Count - 1];
            }

            if ((playListFileStorage.PlayList.Count > 0) && (currentMusic == null))
            {
                currentMusic = playListFileStorage.PlayList[0];
                listBoxMusics.SelectedIndex = 0;
            }

        }

        public void LoadListFromStorage()
        {
            playListFileStorage.Load();
            LoadToListBoxMusics(0);
        }

        public void SetCurrentMusic(object selectedItem)
        {
            if (selectedItem != null)
                currentMusic = playListFileStorage.PlayList.Single(c => c.ItemOnListBox == selectedItem);
        }

        public Music GetCurrrentMusic()
        {
            return currentMusic;
        }

        public void MoveToNextMusic()
        {
            if (playListFileStorage.PlayList.Count > 0)
            {
                var index = playListFileStorage.PlayList.IndexOf(currentMusic);

                if (index < playListFileStorage.PlayList.Count - 1)
                {
                    currentMusic = playListFileStorage.PlayList[++index];
                }
                else
                {
                    currentMusic = playListFileStorage.PlayList[0];
                }
            }
            else
                currentMusic = null;
        }
        public void MoveToPreviousMusic()
        {
            if (playListFileStorage.PlayList.Count > 0)
            {
                var index = playListFileStorage.PlayList.IndexOf(currentMusic);

                if (index == 0)
                {
                    currentMusic = playListFileStorage.PlayList[playListFileStorage.PlayList.Count - 1];
                }
                else
                {
                    currentMusic = playListFileStorage.PlayList[--index];
                }
            }
            else
                currentMusic = null;
        }

        public void ClearPlayList()
        {
            playListFileStorage.Clear();
            playListFileStorage.Save();
            listBoxMusics.Items.Clear();
        }

    }
}
