using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusicPlayer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            IsPlaying(false);
        }

        private PlayList playList;

        private void IsPlaying(bool value)
        {
            if (value) btnPauseContinue.Content = "Pause";
            btnRunBack.IsEnabled = value;
            btnRunForward.IsEnabled = value;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            playList.SetCurrentMusic(listBoxMusics.SelectedItem);
            PlayNew();
        }

        #region timmer

        private DispatcherTimer timerMoveSlider;

        private void timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && (mediaElement.NaturalDuration.TimeSpan.TotalSeconds > 0))
            {
                currentSecond.Text = mediaElement.Position.TotalSeconds.ToString("#s");
                totalSeconds.Text = mediaElement.NaturalDuration.TimeSpan.TotalSeconds.ToString("#s");
                if ((int)(timerMoveSlider.Tag ?? 1) == 1)
                    sliderPositionOfMusic.Value = mediaElement.Position.TotalSeconds;
            }
        }

        private void sliderPositionOfMusic_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timerMoveSlider.Tag = 0;
        }

        private void sliderPositionOfMusic_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(sliderPositionOfMusic.Value);
            timerMoveSlider.Tag = 1;
        }

        #endregion

        private void btnPauseContinue_Click(object sender, RoutedEventArgs e)
        {
            if (btnPauseContinue.Content.ToString() == "Pause")
            {
                mediaElement.Pause();
                btnPauseContinue.Content = "Continue";
                IsPlaying(false);
            }
            else
            {
                mediaElement.Play();
                btnPauseContinue.Content = "Pause";
                IsPlaying(true);
            }
        }

        private void btnRunBack_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(5);
        }

        private void btnRunForward_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(5);
        }

        private void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            playList.AddFolder();    
        }

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = VolumeSlider.Value;
        }

        private void BalanceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Balance = BalanceSlider.Value;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.SpeedRatio = SpeedSlider.Value;
        }

        #region media

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            sliderPositionOfMusic.Minimum = 0;
            sliderPositionOfMusic.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            timerMoveSlider.Start();
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            timerMoveSlider.Stop();
            MoveNext();
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            timerMoveSlider.Stop();
            MoveNext();
        }

        #endregion


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playList = new PlayList(mediaElement, listBoxMusics);
            playList.LoadListFromStorage();
            timerMoveSlider = new DispatcherTimer();
            timerMoveSlider.Interval = TimeSpan.FromSeconds(1);
            timerMoveSlider.Tick += new EventHandler(timer_Tick);
        }

        private void listMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                playList.SetCurrentMusic(listBoxMusics.SelectedItem);
                PlayNew();
            }
        }

        private void MoveNext()
        {
            playList.MoveToNextMusic();
            PlayNew();
        }

        private void PlayNew()
        {
            var music = playList.GetCurrrentMusic();
            if (music != null)
            {
                mediaElement.Source = new Uri(music.Path);
                mediaElement.LoadedBehavior = MediaState.Manual;
                mediaElement.UnloadedBehavior = MediaState.Manual;
                mediaElement.Play();
                listBoxMusics.SelectedIndex = listBoxMusics.Items.IndexOf(music.ItemOnListBox);
                IsPlaying(true);
                textMusicName.Text = music.Name;
            }
        }

        private void listMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                playList.SetCurrentMusic(listBoxMusics.SelectedItem);
                PlayNew();
            }
        }

        private void LimparLista_Click(object sender, RoutedEventArgs e)
        {
            playList.ClearPlayList();
        }

        private void btnAddFiles_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
