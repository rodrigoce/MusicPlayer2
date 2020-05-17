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

        public void AddFolder()
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                playListFileStorage.GetMusicRecursive(fd.SelectedPath);
            }
            LoadToListBoxMusics();
            playListFileStorage.Save();
        }

        private void LoadToListBoxMusics()
        {
            foreach (var music in playListFileStorage.PlayList)
            {
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
            LoadToListBoxMusics();
        }

        public void SetCurrentMusic(object selectedItem)
        {
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
        }

        public void ClearPlayList()
        {
            playListFileStorage.Clear();
            playListFileStorage.Save();
            listBoxMusics.Items.Clear();
        }
    }
}
