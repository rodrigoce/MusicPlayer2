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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playList = new PlayList(mediaElement, listBoxMusics);
            playList.LoadListFromStorage();
            timerMoveSlider = new DispatcherTimer();
            timerMoveSlider.Interval = TimeSpan.FromSeconds(1);
            timerMoveSlider.Tick += new EventHandler(timer_Tick);
            IsPlaying(false);
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

        #endregion
   
        #region click and keypress

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            playList.SetCurrentMusic(listBoxMusics.SelectedItem);
            PlayNew();
        }

        private void BtnPauseContinue_Click(object sender, RoutedEventArgs e)
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

        private void BtnRunBack_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(5);
        }

        private void BtnRunForward_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(5);
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            MoveNext();
        }
        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            MovePrevious();
        }

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
        }

        private void ListMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                playList.SetCurrentMusic(listBoxMusics.SelectedItem);
                PlayNew();
            }
        }

        private void ListMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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

        private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            playList.AddFolder();
        }

        private void BtnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            playList.AddFiles();
        }

        private void SliderPositionOfMusic_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timerMoveSlider.Tag = 0;
        }

        private void SliderPositionOfMusic_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(sliderPositionOfMusic.Value);
            timerMoveSlider.Tag = 1;
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

        #endregion

        #region media

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            sliderPositionOfMusic.Minimum = 0;
            sliderPositionOfMusic.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            timerMoveSlider.Start();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            timerMoveSlider.Stop();
            MoveNext();
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            timerMoveSlider.Stop();
            MoveNext();
        }

        #endregion

        #region reusable methods   

        private void MoveNext()
        {
            playList.MoveToNextMusic();
            PlayNew();
        }

        private void MovePrevious()
        {
            playList.MoveToPreviousMusic();
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
                textMusicName.Text = music.Nro.ToString() + " -  " + music.Name;
            }
        }

        private void IsPlaying(bool value)
        {
            //if (value) btnPauseContinue.Content = "Pause";
            btnPauseContinue.IsEnabled = (value || btnPauseContinue.Content.ToString().Equals("Continue"));
            btnRunBack.IsEnabled = value;
            btnRunForward.IsEnabled = value;
        }

        #endregion

        
    }
}
