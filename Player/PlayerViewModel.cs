using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicPlayer2
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        public PlayerViewModel() 
        {
            PlayerControlsFileStorage = new PlayerControlsFileStorage();            
            PlayListFileStorage = new PlayListFileStorage();
            PlayListFileStorage.Load();
            LoadToObservableCollection(PlayListFileStorage.MusicFiles, MusicsList, 0);
            FilteredMusicsList = CollectionViewSource.GetDefaultView(MusicsList) as ListCollectionView;
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

        public PlayerControlsFileStorage PlayerControlsFileStorage { get; private set; }
        public PlayListFileStorage PlayListFileStorage { get; set; }
        public MediaElement MediaElement { get; set; }
        public ListBox ListBoxMusics { get; set; }
        public Button BtnPauseContinue { get; set; }        
        public Slider SliderPositionOfMusic { get; set; }
        public ObservableCollection<Music> MusicsList { get; set; } = new ObservableCollection<Music>();
        public ListCollectionView FilteredMusicsList { get; set; }

        //
        private bool _isPlaying { get; set; }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set 
            { 
                _isPlaying = value; 

                OnPropertyChanged();
            }
        }
        //
        private bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set 
            { 
                _isPaused = value;
                if (value)
                    BtnPauseContinue.Content = "Continue";
                else
                    BtnPauseContinue.Content = "Pause";
            }
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
            set 
            { 
                if (_volumeSlider == value) return;

                _volumeSlider = value; 
                MediaElement.Volume = value; 
                PlayerControlsFileStorage.SaveValue(Constants.VOLUME_SLIDER, value.ToString()); 
                OnPropertyChanged(); 
            }
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
        
        public void LoadControlsValuesFromFile()
        {
            VolumeSlider = PlayerControlsFileStorage.LoadDouble(Constants.VOLUME_SLIDER) ?? 1;
        }

        #endregion

        public void AddFolder()
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int last = PlayListFileStorage.MusicFiles.Count;

                PlayListFileStorage.GetMusicRecursive(fd.SelectedPath);
                LoadToObservableCollection(PlayListFileStorage.MusicFiles, MusicsList, last);
                PlayListFileStorage.Save();
            }
        }

        public void AddFiles()
        {
            var od = new System.Windows.Forms.OpenFileDialog();
            od.Filter = "MP3 files (*.MP3)|*.MP3";
            od.Multiselect = true;

            if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int last = PlayListFileStorage.MusicFiles.Count;
                PlayListFileStorage.AddFiles(od.FileNames);
                LoadToObservableCollection(PlayListFileStorage.MusicFiles, MusicsList, last);
                PlayListFileStorage.Save();
            }
        }

        public void LoadToObservableCollection(List<Music> listSource, ObservableCollection<Music> listDest, int startFrom)
        {
            for (int i = startFrom; i < listSource.Count; i++)
            {
                listDest.Add(listSource[i]);
            }          
        }

        public void ClearPlayList()
        {
            Stop();

            SliderPositionOfMusic.Value = 0;
            PlayListFileStorage.Clear();
            MusicsList.Clear();
            PlayListFileStorage.Save();
            
        }

        public void ShowFinderFilterWindow()
        {
            var find = new FindMusic(this);
            find.ShowDialog();
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
                PlayListFileStorage.MusicFiles.Remove(music);
                MusicsList.Remove(music);
                PlayListFileStorage.Save();
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
                IsPaused = false;
                ListBoxMusics.SelectedIndex = ListBoxMusics.Items.IndexOf(CurrentMusic);
                ListBoxMusics.ScrollIntoView(ListBoxMusics.SelectedItem);

            }
        }

        public void PauseContinue()
        {
            if (IsPlaying)
            {
                if (IsPaused)
                {
                    MediaElement.Play();
                    IsPaused = false;
                }
                else
                {
                    MediaElement.Pause();
                    IsPaused = true;
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
            if (FilteredMusicsList.Count > 0)
            {
                var index = FilteredMusicsList.IndexOf(CurrentMusic);

                if (index == 0)
                {
                    CurrentMusic = FilteredMusicsList.GetItemAt(FilteredMusicsList.Count - 1) as Music;
                }
                else
                {
                    CurrentMusic = FilteredMusicsList.GetItemAt(--index) as Music;
                }
            }
            else
                CurrentMusic = null;

            PlayCurrentMusic();
        }

        public void PlayNext()
        {
            if (FilteredMusicsList.Count > 0)
            {
                var index = FilteredMusicsList.IndexOf(CurrentMusic);

                if (index < FilteredMusicsList.Count - 1)
                {
                    CurrentMusic = FilteredMusicsList.GetItemAt(++index) as Music;
                }
                else
                {
                    CurrentMusic = FilteredMusicsList.GetItemAt(0) as Music;
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
            IsPaused = false;
            SliderPositionOfMusic.Value = 0;
        }
    }
}
