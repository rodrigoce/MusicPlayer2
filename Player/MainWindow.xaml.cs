using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;

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
        }

        #region private fields
        
        bool _isPlaying;
        
        private string labelTextMusicName;

        private DispatcherTimer timerMoveSlider;

        #endregion

        private PlayList playList;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labelTextMusicName = textMusicName.Text;
            playList = new PlayList(mediaElement, listBoxMusics);
            playList.LoadListFromStorage();
            timerMoveSlider = new DispatcherTimer();
            timerMoveSlider.Interval = TimeSpan.FromSeconds(1);
            timerMoveSlider.Tick += new EventHandler(timer_Tick);
            IsPlaying(false);
        }

        #region timmer       

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
                SetAsPlaying();
            }
            else
            {
                SetAsPaused();
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
            if (IsPlaying())
            {
                mediaElement.Stop();
                IsPlaying(false);
            }

            sliderPositionOfMusic.Value = 0;
            textMusicName.Text = labelTextMusicName;
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

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            var find = new FindMusic(playList);
            find.ShowDialog();
            if (find.ReturnKind == ReturnKind.rkMusic)
                PlayNew();
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

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.X:
                    PressButton(btnPlay);
                    break;
                case Key.C:
                    PressButton(btnPauseContinue);
                    break;
                case Key.S:
                    PressButton(btnRunBack);
                    break;
                case Key.D:
                    PressButton(btnRunForward);
                    break;
                case Key.Z:
                    PressButton(btnPrevious);
                    break;
                case Key.V:
                    PressButton(btnForward);
                    break;
                case Key.F:
                    PressButton(btnAddFolder);
                    break;
                case Key.A:
                    PressButton(btnAddFiles);
                    break;
                case Key.Q:
                    PressButton(btnFind);
                    break;
                default:
                    break;
            }
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
            IsPlaying(false);
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
                SetAsPaused();
                listBoxMusics.SelectedIndex = listBoxMusics.Items.IndexOf(music.ItemOnListBox);
                listBoxMusics.ScrollIntoView(listBoxMusics.SelectedItem);
                IsPlaying(true);
                textMusicName.Text = music.Nro.ToString() + " -  " + music.Name;
            }
            else
                IsPlaying(false);
        }

        private void IsPlaying(bool value)
        {
            btnPauseContinue.IsEnabled = (value || btnPauseContinue.Content.ToString().Equals("Continue"));
            btnRunBack.IsEnabled = value;
            btnRunForward.IsEnabled = value;
            timerMoveSlider.IsEnabled = value;
            _isPlaying = value;
        }

        private bool IsPlaying()
        {
            return _isPlaying;
        }

        private void SetAsPlaying()
        {
            mediaElement.Pause();
            btnPauseContinue.Content = "Continue";
            IsPlaying(false);
        }

        private void SetAsPaused()
        {
            mediaElement.Play();
            btnPauseContinue.Content = "Pause";
            IsPlaying(true);
        }

        private void PressButton(Button btn)
        {
            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        #endregion

    }
}
