using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MusicPlayer2
{
    public class PlayList
    {
        public PlayList(MediaElement mediaElement, ListBox listBox, Button btnPauseContinue, Button btnRunBack, Button btnRunForward, DispatcherTimer timerMoveSlider,
            TextBlock txtMusicName)
        {
            this.mediaElement = mediaElement;
            this.listBoxMain = listBox;
            this.playListFileStorage = new PlayListFileStorage();
            this.btnPauseContinue = btnPauseContinue;
            this.btnRunBack = btnRunBack;
            this.btnRunForward = btnRunForward;
            this.timerMoveSlider = timerMoveSlider;
            this.txtMusicName = txtMusicName;
        }

        #region Fiels
        
        private readonly PlayListFileStorage playListFileStorage;
        private readonly MediaElement mediaElement;
        private readonly System.Windows.Controls.ListBox listBoxMain;
        private List<Music> foundPlayList; 
        private Music currentMusic;
        private bool _isPlaying;
        private Button btnPauseContinue;
        private Button btnRunBack;
        private Button btnRunForward;
        private DispatcherTimer timerMoveSlider;
        private TextBlock txtMusicName;
        
        #endregion

        #region Props
        
        #endregion

        private int GetLastPlayListIndex()
        {
            int last = playListFileStorage.PlayList.Count;
           
            return last;
        }
        
        public void AddFolder()
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
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
            var od = new System.Windows.Forms.OpenFileDialog();
            od.Filter = "MP3 files (*.MP3)|*.MP3";
            od.Multiselect = true;

            if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int last = GetLastPlayListIndex();
                playListFileStorage.AddFiles(od.FileNames);
                LoadToListBoxMusics(listBoxMain, playListFileStorage.PlayList, last, false, false);
                playListFileStorage.Save();
            }
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

        public void DeletePermanently(Music music)
        {
            if (music == GetCurrrentMusic())
                File.Delete(music.Path);
        }

        public void SetCurrentMusicFound(object selectedItem)
        {
            if (selectedItem != null)
                currentMusic = playListFileStorage.PlayList.Single(c => c.ItemOnFindListBox == selectedItem);
        }

        public Music SetCurrentSelectedMusic()
        {
            if (listBoxMain.SelectedItem != null)
                return playListFileStorage.PlayList.Single(c => c.ItemOnListBox == listBoxMain.SelectedItem);
            return null;
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


        public void IsPlaying(bool value)
        {
            if (!value)
                mediaElement.Stop();

            btnPauseContinue.IsEnabled = (value || btnPauseContinue.Content.ToString().Equals("Continue"));
            btnRunBack.IsEnabled = value;
            btnRunForward.IsEnabled = value;
            timerMoveSlider.IsEnabled = value;
            _isPlaying = value;
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public void PlayCurrentMusic()
        {
            var music = GetCurrrentMusic();
            if (music != null)
            {
                mediaElement.Source = new Uri(music.Path);
                mediaElement.Play();
                listBoxMain.SelectedIndex = listBoxMain.Items.IndexOf(music.ItemOnListBox);
                listBoxMain.ScrollIntoView(listBoxMain.SelectedItem);
                IsPlaying(true);
                txtMusicName.Text = music.Nro.ToString() + " -  " + music.Name;
            }
            else
                IsPlaying(false);
        }

        public void PauseContinue()
        {
            if (IsPlaying())
            {
                if (btnPauseContinue.Content.ToString() == "Pause")
                {
                    mediaElement.Pause();
                    btnPauseContinue.Content = "Continue";
                }
                else
                {
                    mediaElement.Play();
                    btnPauseContinue.Content = "Pause";
                }
            }
        }

    }
}
