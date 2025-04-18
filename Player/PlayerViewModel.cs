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
            LoadToListBoxMusics(playListFileStorage.MusicFiles, 0, false, false);
            PlayCommand = new RelayCommand(Play);
            StopCommand = new RelayCommand(Stop);
            PauseContinueCommand = new RelayCommand(PauseContinue);
            RunBackCommand = new RelayCommand(RunBack);
            RunForwardCommand = new RelayCommand(RunForward);
            PreviousCommand = new RelayCommand(PlayPrevious);
            NextCommand = new RelayCommand(PlayNext);
            AddFolderCommand = new RelayCommand(AddFolder);
            AddFilesCommand = new RelayCommand(AddFiles);
            ShowFinderFilterWindowCommand = new RelayCommand(ShowFinderFilterWindow);
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
        public ICommand ShowFinderFilterWindowCommand { get; }
        public ICommand ClearPlayListCommand { get; }

        #endregion

        #region Props

        private PlayListFileStorage playListFileStorage { get; set; }
        private List<Music> foundPlayList { get; set; }        
        public MediaElement MediaElement { get; set; }
        public ListBox ListBoxMusics { get; set; }
        public Button BtnPauseContinue { get; set; }        
        public Slider SliderPositionOfMusic { get; set; }
        public ObservableCollection<Music> MusicsList { get; set; } = new ObservableCollection<Music>();

        //
        private bool _isPlaying { get; set; }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { _isPlaying = value; OnPropertyChanged(); }
        }
        //
        private Music _currentMusic;
        public Music CurrentMusic 
        { 
            get => _currentMusic; 
            set { _currentMusic = value; CurrentMusicName = (value == null) ? "" : value.Nro.ToString() + " - " + value.Name; } 
        }
        //
        private string _currentMusicName;
        public string CurrentMusicName
        {
            get { return string.IsNullOrEmpty(_currentMusicName) ? System.Windows.Application.Current.Resources["txtMusicNameDefaultText"] as string : _currentMusicName; }
            set { _currentMusicName = value; OnPropertyChanged(); }
        }
        //
        private bool _isMuted = false;
        public bool IsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; MediaElement.IsMuted = value; OnPropertyChanged(); }
        }
        //
        private double _volumeSlider = 1;
        public double VolumeSlider
        {
            get { return _volumeSlider; }
            set { _volumeSlider = value; MediaElement.Volume = value; OnPropertyChanged(); }
        }
        //
        private double _balanceSlider = 0;
        public double BalanceSlider
        {
            get { return _balanceSlider; }
            set { _balanceSlider = value; MediaElement.Balance = value; OnPropertyChanged(); }
        }
        //

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
                int last = playListFileStorage.MusicFiles.Count;

                playListFileStorage.GetMusicRecursive(fd.SelectedPath);
                LoadToListBoxMusics(playListFileStorage.MusicFiles, last, false, false);
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
                int last = playListFileStorage.MusicFiles.Count;
                playListFileStorage.AddFiles(od.FileNames);
                LoadToListBoxMusics(playListFileStorage.MusicFiles, last, false, false);
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

        public void ShowFinderFilterWindow()
        {
            var find = new FindMusic(this);
            find.ShowDialog();
            /*if (find.ReturnKind == ReturnKind.rkMusic)
                playList.PlayCurrentMusic();
            else if (find.ReturnKind == ReturnKind.rkFilter)
            {
                playList.PlayCurrentMusic();
            }*/
        }


        public void Filter(System.Windows.Controls.ListBox listBox, string filterText)
        {
            if (filterText.Trim().Length > 0)
            {
                var keys = filterText.ForSearch().Split(' ');
                keys = keys.Where(c => c.Trim() != "").ToArray();
                string chaves = keys.Aggregate((i, j) => i + "|" + j);

                foundPlayList = playListFileStorage.MusicFiles.Where(c => Regex.Match(c.NameForSearch, chaves).Success).ToList();
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
            if ((music == CurrentMusic) && (IsPlaying))
                PlayNext();

            string error = string.Empty;
            try
            {
                File.Delete(music.Path);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (error == string.Empty)
            {
                playListFileStorage.MusicFiles.Remove(music);
                MusicsList.Remove(music);
                playListFileStorage.Save();
            }

        }

        //
        //

        public void Play()
        {
            if ((ListBoxMusics.SelectedItem == null) && (ListBoxMusics.Items.Count > 0))
                ListBoxMusics.SelectedIndex = 0;

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
                IsPlaying = true;
                ListBoxMusics.SelectedIndex = ListBoxMusics.Items.IndexOf(CurrentMusic);
                ListBoxMusics.ScrollIntoView(ListBoxMusics.SelectedItem);

            }
        }

        public void PauseContinue()
        {
            if (IsPlaying)
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

        public void Stop()
        {
            MediaElement.Stop();
            CurrentMusic = null;
            IsPlaying = false;
            SliderPositionOfMusic.Value = 0;
        }
    }
}
