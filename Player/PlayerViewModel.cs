using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MusicPlayer2
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        public PlayerViewModel() 
        {
            this.playListFileStorage = new PlayListFileStorage();
            playListFileStorage.Load();
            LoadToListBoxMusics(playListFileStorage.Files, 0, false, false);
            PlayCommand = new RelayCommand(Play);
            StopCommand = new RelayCommand(Stop);
            PauseContinueCommand = new RelayCommand(PauseContinue);
            RunBackCommand = new RelayCommand(RunBack);
            RunForwardCommand = new RelayCommand(RunForward);
            PreviousCommand = new RelayCommand(PlayPrevious);
            NextCommand = new RelayCommand(PlayNext);
            AddFolderCommand = new RelayCommand(AddFolder);
            AddFilesCommand = new RelayCommand(AddFiles);
            ClearPlayListCommand = new RelayCommand(ClearPlayList);
        }

        #region Commands
        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PauseContinueCommand { get; }
        public ICommand RunBackCommand { get; }
        public ICommand RunForwardCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand AddFilesCommand { get; }
        public ICommand ClearPlayListCommand { get; }

        #endregion

        #region Props

        private PlayListFileStorage playListFileStorage { get; set; }
        //
        private Music _currentMusic;
        private Music CurrentMusic { get => _currentMusic; set { _currentMusic = value; CurrentMusicName = value?.Name ?? ""; } }
        //
        private string _currentMusicName;
        public string CurrentMusicName
        {
            get { return string.IsNullOrEmpty(_currentMusicName) ? System.Windows.Application.Current.Resources["txtMusicNameDefaultText"] as string : _currentMusicName; }
            set { _currentMusicName = value; OnPropertyChanged(); }
        }
        //
        public TextBlock TxtCurrentMusicName { get; set; }
        private bool _isPlaying { get; set; }
        private List<Music> foundPlayList { get; set; }
        
        public MediaElement MediaElement { get; set; }
        public ListBox ListBoxMusics { get; set; }
        public Button BtnPauseContinue { get; set; }
        public Button BtnRunBack { get; set; }
        public Button BtnRunForward { get; set; }
        public DispatcherTimer TimerMoveSlider { get; set; }
        public Slider SliderPositionOfMusic { get; set; }
        public ObservableCollection<Music> MusicsList { get; set; } = new ObservableCollection<Music>();


        #endregion

        #region Others
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion

        public void AddFolder()
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int last = playListFileStorage.Files.Count;

                playListFileStorage.GetMusicRecursive(fd.SelectedPath);
                LoadToListBoxMusics(playListFileStorage.Files, last, false, false);
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
                LoadToListBoxMusics(playListFileStorage.Files, last, false, false);
                playListFileStorage.Save();
            }
        }

        private void LoadToListBoxMusics(List<Music> listSource, int startFrom, bool InFindScreen, bool onlyFiltered)
        {
            for (int i = startFrom; i < listSource.Count; i++)
            {
                MusicsList.Add(listSource[i]);
            }          
        }

        public void ClearPlayList()
        {
            Stop();

            SliderPositionOfMusic.Value = 0;
            playListFileStorage.Clear();
            MusicsList.Clear();
            playListFileStorage.Save();
            
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
                LoadToListBoxMusics(foundPlayList, 0, true, true);
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
            ListBoxMusics.Items.Remove(music.ItemOnListBox);
            playListFileStorage.Save();
            CurrentMusic = null;

        }

        ////////////////////////////////
        ////////////////////////////////

        public void Play()
        {
            if (ListBoxMusics.SelectedItem != null)
                CurrentMusic = ListBoxMusics.SelectedItem as Music;

            PlayCurrentMusic();
        }

        public void PlayCurrentMusic()
        {    
            if (CurrentMusic != null)
            {
                MediaElement.Source = new Uri(CurrentMusic.Path);
                MediaElement.Play();
                _isPlaying = true;
                ListBoxMusics.SelectedIndex = ListBoxMusics.Items.IndexOf(CurrentMusic.ItemOnListBox);
                ListBoxMusics.ScrollIntoView(ListBoxMusics.SelectedItem);
                RefreshControlsStatus(false);

            }
            else
                RefreshControlsStatus(true);
        }


        public void PauseContinue()
        {
            if (_isPlaying)
            {
                if (BtnPauseContinue.Content.ToString() == "Pause")
                {
                    MediaElement.Pause();
                    BtnPauseContinue.Content = "Continue";
                }
                else
                {
                    MediaElement.Play();
                    BtnPauseContinue.Content = "Pause";
                }
            }
        }

        public void RunBack()
        {
            MediaElement.Position = MediaElement.Position.Subtract(new TimeSpan(0, 0, 5));
        }

        public void RunForward()
        {
            MediaElement.Position = MediaElement.Position.Add(new TimeSpan(0, 0, 5));
        }

        public void PlayPrevious()
        {
            if (MusicsList.Count > 0)
            {
                var index = MusicsList.IndexOf(CurrentMusic);

                if (index == 0)
                {
                    CurrentMusic = MusicsList[MusicsList.Count - 1];
                }
                else
                {
                    CurrentMusic = MusicsList[--index];
                }
            }
            else
                CurrentMusic = null;

            PlayCurrentMusic();
        }

        public void PlayNext()
        {
            if (MusicsList.Count > 0)
            {
                var index = MusicsList.IndexOf(CurrentMusic);

                if (index < MusicsList.Count - 1)
                {
                    CurrentMusic = MusicsList[++index];
                }
                else
                {
                    CurrentMusic = MusicsList[0];
                }
            }
            else
                CurrentMusic = null;

            PlayCurrentMusic();
        }


        public void RefreshControlsStatus(bool isStoped)
        {
            BtnPauseContinue.IsEnabled = !isStoped;
            BtnRunBack.IsEnabled = !isStoped;
            BtnRunForward.IsEnabled = !isStoped;
            TimerMoveSlider.IsEnabled = !isStoped;

            if (isStoped)
                SliderPositionOfMusic.Value = 0;

        }

        public void Stop()
        {
            MediaElement.Stop();
            CurrentMusic = null;
            _isPlaying = false;
            RefreshControlsStatus(true);
        }

        

    }
}
