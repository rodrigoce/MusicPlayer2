using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Web.UI.WebControls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;

namespace MusicPlayer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region private fields     
        
        private string labelTextMusicName;
        private DispatcherTimer timerMoveSlider;
        private PlayList playList;

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            labelTextMusicName = textMusicName.Text;
            timerMoveSlider = new DispatcherTimer();
            playList = new PlayList(mediaElement, listBoxMusics, btnPauseContinue, btnRunBack, btnRunForward, timerMoveSlider, sliderPositionOfMusic, textMusicName);

            timerMoveSlider.Interval = TimeSpan.FromSeconds(1);
            timerMoveSlider.Tick += new EventHandler(timer_Tick);

            playList.Stop2();
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
            playList.Play2(true);
        }               

        private void BtnPauseContinue_Click(object sender, RoutedEventArgs e)
        {
            playList.PauseContinue2();
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
            playList.PlayNextMusic2();
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            playList.PlayPreviousMusic2();
        }

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
        }

        private void ListMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                playList.Play2(true);
        }

        private void ListMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                playList.Play2(true);
        }

        private void CleanPlaylist_Click(object sender, RoutedEventArgs e)
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

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            var find = new FindMusic(playList);
            find.ShowDialog();
            /*if (find.ReturnKind == ReturnKind.rkMusic)
                playList.PlayCurrentMusic();
            else if (find.ReturnKind == ReturnKind.rkFilter)
            {
                playList.PlayCurrentMusic();
            }*/
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

        private async void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.X:
                    PressButton(btnPlay, e);
                    break;
                case Key.C:
                    PressButton(btnPauseContinue, e);
                    break;
                case Key.Space:
                    PressButton(btnPauseContinue, e);
                    break;
                case Key.Z:
                    PressButton(btnPrevious, e);
                    break;
                case Key.V:
                    PressButton(btnForward, e);
                    break;
                case Key.F:
                    PressButton(btnAddFolder, e);
                    break;
                case Key.A:
                    PressButton(btnAddFiles, e);
                    break;
                case Key.Q:
                    PressButton(btnFind, e);
                    break;
                case Key.Delete:
                    await AskDeletePermanently();
                    break;
                default:
                    break;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S:
                    PressButton(btnRunBack, e);
                    break;
                case Key.Left:
                    PressButton(btnRunBack, e);
                    break;
                case Key.D:
                    PressButton(btnRunForward, e);
                    break;
                case Key.Right:
                    PressButton(btnRunForward, e);
                    break;
                default:
                    break;
            }
        }

        private async void DeletePermanently_Click(object sender, RoutedEventArgs e)
        {
            await AskDeletePermanently();
        }

        private async Task AskDeletePermanently()
        {
            /*var music = playList.SetCurrentSelectedMusic();
            if (music != null)
            {
                var result = await this.ShowMessageAsync(
                                "Confirmação",
                                $"Confirma a exclusão permanente do arquivo {music.Path}?",
                                MessageDialogStyle.AffirmativeAndNegative,
                                new MetroDialogSettings
                                {
                                    AffirmativeButtonText = "Sim",
                                    NegativeButtonText = "Não",
                                    AnimateShow = false,
                                    AnimateHide = false,
                                    DefaultButtonFocus = MessageDialogResult.Affirmative
                                });

                if (result == MessageDialogResult.Affirmative)
                    playList.DeletePermanently(music);
            }*/
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
            playList.PlayNextMusic2();
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            timerMoveSlider.Stop();
            playList.PlayNextMusic2();
        }

        #endregion

        private void PressButton(System.Windows.Controls.Button btn, KeyEventArgs e)
        {
            e.Handled = true;
            btn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }


    }
}
