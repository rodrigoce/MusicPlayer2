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
                LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, last, false, false);
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
                LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, last, false, false);
                playListFileStorage.Save();
            }
        }

        public void SetCurrentMusicFound(object selectedItem)
        {
            if (selectedItem != null)
                currentMusic = playListFileStorage.PlayList.Single(c => c.ItemOnFindListBox == selectedItem);
        }

        private void LoadToListBoxMusics(System.Windows.Controls.ListBox listBox, List<Music> listSource, int startFrom, bool InFindScreen, bool onlyFiltered)
        {
            for (int i = startFrom; i < listSource.Count; i++)
            {
                var music = listSource[i];

                // adiciona apenas as músicas filtradas
                bool added = false;
                if (onlyFiltered && music.IsInFilter)
                {
                    listBox.Items.Add(music.Nro + " - " + music.Name);
                    added = true;
                }
                else if (!onlyFiltered)
                {
                    listBox.Items.Add(music.Nro + " - " + music.Name);
                    added = true;
                }

                // apenas se a musica foi adicionada liga o listbox com a List<Music>
                if (added)
                {
                    if (!InFindScreen)
                        music.ItemOnListBox = listBox.Items[listBox.Items.Count - 1];
                    else
                        music.ItemOnFindListBox = listBox.Items[listBox.Items.Count - 1];
                }
            }

            if (!InFindScreen)
            {
                if ((listBoxMain.Items.Count > 0) && (currentMusic == null))
                {
                    SetCurrentMusic(listBoxMain.Items[0]);
                    listBoxMain.SelectedIndex = 0;
                }
            }
        }

        public void LoadListFromStorage()
        {
            playListFileStorage.Load();
            LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, 0, false, false);
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

        public void Filter(System.Windows.Controls.ListBox listBox, string filterText)
        {
            if (filterText.Trim().Length > 0)
            {
                var keys = filterText.ForSearch().Split(' ');
                keys = keys.Where(c => c.Trim() != "").ToArray();
                string chaves = keys.Aggregate((i, j) => i + "|" + j);

                foundPlayList = playListFileStorage.PlayList.Where(c => Regex.Match(c.NameForSearch, chaves).Success).ToList();
                listBox.Items.Clear();
                LoadToListBoxMusics(listBox, foundPlayList, 0, true, false);
            }
            else
            {
                listBox.Items.Clear();
            }
        }

        public void PlayFiltered()
        {
            playListFileStorage.PlayList.ForEach(c => c.IsInFilter = false);
            foreach (var music in foundPlayList)
            {
                playListFileStorage.PlayList.Single(c => c.Nro == music.Nro).IsInFilter = true;
            }
            currentMusic = null;
            listBoxMain.Items.Clear();
            LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, 0, false, true);
        }
    }
}
