using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MusicPlayer2
{
    public class PlayList
    {
        public PlayList(MediaElement mediaElement, System.Windows.Controls.ListBox listBox)
        {
            this.mediaElement = mediaElement;
            this.listBoxMain = listBox;
            playListFileStorage = new PlayListFileStorage();
        }

        private readonly PlayListFileStorage playListFileStorage;
        private readonly MediaElement mediaElement;
        private readonly System.Windows.Controls.ListBox listBoxMain;
        private List<Music> foundPlayList; 
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
                LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, last, false);
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
                LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, last, false);
                playListFileStorage.Save();
            }
        }

        public void SetCurrentMusicFinded(object selectedItem)
        {
            if (selectedItem != null)
                currentMusic = playListFileStorage.PlayList.Single(c => c.ItemOnFindListBox == selectedItem);
        }

        private void LoadToListBoxMusics(System.Windows.Controls.ListBox listBox, List<Music> listSource, int startFrom, bool AtFinder)
        {
            for (int i = startFrom; i < listSource.Count; i++)
            {
                var music = listSource[i];
                listBox.Items.Add(music.Nro + " - " + music.Name);
                if (!AtFinder)
                    music.ItemOnListBox = listBox.Items[listBox.Items.Count - 1];
                else
                    music.ItemOnFindListBox = listBox.Items[listBox.Items.Count - 1];
            }

            if ((listSource.Count > 0) && (currentMusic == null))
            {
                currentMusic = listSource[0];
                listBox.SelectedIndex = 0;
            }

        }

        public void LoadListFromStorage()
        {
            playListFileStorage.Load();
            LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, 0, false);
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
            listBoxMain.Items.Clear();
        }

        public void Find(System.Windows.Controls.ListBox listBox, string text)
        {
            if (text.Trim().Length > 0)
            {
                var keys = text.ForSearch().Split(' ');
                keys = keys.Where(c => c.Trim() != "").ToArray();
                string chaves = keys.Aggregate((i, j) => i + "|" + j);

                foundPlayList = playListFileStorage.PlayList.Where(c => Regex.Match(c.NameForSearch, chaves).Success).ToList();
                listBox.Items.Clear();
                LoadToListBoxMusics(listBox, foundPlayList, 0, true);
            }
            else
            {
                listBox.Items.Clear();
            }
        }
    }
}
