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
        public PlayList(MediaElement mediaElement, ListBox listBoxMusic, Button btnPauseContinue, Button btnRunBack, Button btnRunForward, DispatcherTimer timerMoveSlider,
Slider sliderPositionOfMusic, TextBlock txtMusicName)
        {
            this.mediaElement = mediaElement;
            this.listBoxMusic = listBoxMusic;
            this.playListFileStorage = new PlayListFileStorage();
            this.btnPauseContinue = btnPauseContinue;
            this.btnRunBack = btnRunBack;
            this.btnRunForward = btnRunForward;
            this.timerMoveSlider = timerMoveSlider;
            this.sliderPositionOfMusic = sliderPositionOfMusic;
            this.txtMusicName = txtMusicName;
            playListFileStorage.Load();
            LoadToListBoxMusics(listBoxMusic, playListFileStorage.Files, 0, false, false);
        }

        #region Props
        
        private PlayListFileStorage playListFileStorage { get; set; }
        private MediaElement mediaElement { get; set; }
        private ListBox listBoxMusic { get; set; }
        private List<Music> foundPlayList { get; set; } 
        private Music CurrentMusic { get; set; }
        private bool _isPlaying { get; set; }
        private Button btnPauseContinue { get; set; }
        private Button btnRunBack { get; set; }
        private Button btnRunForward { get; set; }
        private DispatcherTimer timerMoveSlider { get; set; }
        private Slider sliderPositionOfMusic { get; set; }
        private TextBlock txtMusicName { get; set; }
        
        #endregion
      
        public void AddFolder()
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int last = playListFileStorage.Files.Count;

                playListFileStorage.GetMusicRecursive(fd.SelectedPath);
                LoadToListBoxMusics(listBoxMusic, playListFileStorage.Files, last, false, false);
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
                int last = playListFileStorage.Files.Count;
                playListFileStorage.AddFiles(od.FileNames);
                LoadToListBoxMusics(listBoxMusic, playListFileStorage.Files, last, false, false);
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
                if ((listBoxMusic.Items.Count > 0) && (CurrentMusic == null))
                {
                    //SetCurrentMusic(listBoxMusic.Items[0]);
                    listBoxMusic.SelectedIndex = 0;
                }
            }
        }              

        public void ClearPlayList()
        {
            Stop2();

            sliderPositionOfMusic.Value = 0;
            txtMusicName.Text = System.Windows.Application.Current.Resources["txtMusicNameDefaultText"] as string;
            playListFileStorage.Clear();
            playListFileStorage.Save();
            listBoxMusic.Items.Clear();
        }


        public void Filter(System.Windows.Controls.ListBox listBox, string filterText)
        {
            if (filterText.Trim().Length > 0)
            {
                var keys = filterText.ForSearch().Split(' ');
                keys = keys.Where(c => c.Trim() != "").ToArray();
                string chaves = keys.Aggregate((i, j) => i + "|" + j);

                foundPlayList = playListFileStorage.Files.Where(c => Regex.Match(c.NameForSearch, chaves).Success).ToList();
                listBox.Items.Clear();
                LoadToListBoxMusics(listBox, foundPlayList, 0, true, false);
            }
            else
            {
                listBox.Items.Clear();
            }
        }
        
        public void DeletePermanently(Music music)
        {
            if (music == CurrentMusic)
                //SetIsPlaying(false);

            File.Delete(music.Path);
            playListFileStorage.Files.Remove(music);
            listBoxMusic.Items.Remove(music.ItemOnListBox);
            playListFileStorage.Save();
            CurrentMusic = null;

        }



        

        

        


        ////////////////////////////////
        ////////////////////////////////

        public void Play2(bool playFromListBox)
        {
            if (playFromListBox) 
                if (listBoxMusic.SelectedItem != null)
                    CurrentMusic = playListFileStorage.Files.FirstOrDefault(c => c.ItemOnListBox == listBoxMusic.SelectedItem);
            
            if (CurrentMusic != null)
            {
                mediaElement.Source = new Uri(CurrentMusic.Path);
                mediaElement.Play();
                _isPlaying = true;
                listBoxMusic.SelectedIndex = listBoxMusic.Items.IndexOf(CurrentMusic.ItemOnListBox);
                listBoxMusic.ScrollIntoView(listBoxMusic.SelectedItem);
                RefreshControlsStatus2(false);
                
            }
            else
                RefreshControlsStatus2(true);
        

        }

        public void RefreshControlsStatus2(bool isStoped)
        {
            btnPauseContinue.IsEnabled = !isStoped;
            btnRunBack.IsEnabled = !isStoped;
            btnRunForward.IsEnabled = !isStoped;
            timerMoveSlider.IsEnabled = !isStoped;

            if (CurrentMusic != null)
                txtMusicName.Text = CurrentMusic.Nro.ToString() + " -  " + CurrentMusic.Name;

            if (isStoped)
            {
                sliderPositionOfMusic.Value = 0;
                txtMusicName.Text = System.Windows.Application.Current.Resources["txtMusicNameDefaultText"] as string;
            }
        }

        public void Stop2()
        {
            mediaElement.Stop();
            _isPlaying = false;
            RefreshControlsStatus2(true);
        }

        public void PlayNextMusic2()
        {
            Stop2();

            if (playListFileStorage.Files.Count > 0)
            {
                var index = playListFileStorage.Files.IndexOf(CurrentMusic);

                if (index < playListFileStorage.Files.Count - 1)
                {
                    CurrentMusic = playListFileStorage.Files[++index];
                }
                else
                {
                    CurrentMusic = playListFileStorage.Files[0];
                }
            }
            else
                CurrentMusic = null;

            Play2(false);
        }

        public void PlayPreviousMusic2()
        {
            Stop2();

            if (playListFileStorage.Files.Count > 0)
            {
                var index = playListFileStorage.Files.IndexOf(CurrentMusic);

                if (index == 0)
                {
                    CurrentMusic = playListFileStorage.Files[playListFileStorage.Files.Count - 1];
                }
                else
                {
                    CurrentMusic = playListFileStorage.Files[--index];
                }
            }
            else
                CurrentMusic = null;

            Play2(false);
        }

        public void PauseContinue2()
        {
            if (_isPlaying)
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
