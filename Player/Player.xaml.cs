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
    public partial class Player : MetroWindow
    {
        public Player()
        {
            InitializeComponent();
        }

        #region private fields     
        
        private string labelTextMusicName;
        private DispatcherTimer timerMoveSlider;

        #endregion

        #region Props
        public PlayerViewModel PlayerViewModel => (DataContext as PlayerViewModel);
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            labelTextMusicName = textMusicName.Text;
            timerMoveSlider = new DispatcherTimer();
            
            timerMoveSlider.Interval = TimeSpan.FromSeconds(1);
            timerMoveSlider.Tick += new EventHandler(timer_Tick);

            PlayerViewModel.MediaElement = mediaElement;
            PlayerViewModel.ListBoxMusics = listBoxMusics;
            PlayerViewModel.TxtCurrentMusicName = textMusicName;
            PlayerViewModel.BtnPauseContinue = btnPauseContinue;
            PlayerViewModel.BtnRunBack = btnRunBack;
            PlayerViewModel.BtnRunForward = btnRunForward;
            PlayerViewModel.TimerMoveSlider = timerMoveSlider;
            PlayerViewModel.SliderPositionOfMusic = sliderPositionOfMusic;

            if (listBoxMusics.SelectedIndex == -1)
                listBoxMusics.SelectedIndex = 0;

            PlayerViewModel.Stop();
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
           

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
        }

        private void ListMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                PlayerViewModel.Play(/*true*/);
        }

        private void ListMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PlayerViewModel.Play(/*true*/);
        }  

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            //var find = new FindMusic(PlayerViewModel);
            //find.ShowDialog();
            /*if (find.ReturnKind == ReturnKind.rkMusic)
                playList.PlayCurrentMusic();
            else if (find.ReturnKind == ReturnKind.rkFilter)
            {
                playList.PlayCurrentMusic();
            }*/
        }


        private void btnTests_Click(object sender, RoutedEventArgs e)
        {
            new WindowTests().ShowDialog();
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
                    PlayerViewModel.PlayCommand.Execute(null);
                    break;
                case Key.C:
                    PlayerViewModel.PauseContinueCommand.Execute(null);
                    break;
                case Key.Space:
                    PlayerViewModel.PauseContinueCommand.Execute(null);
                    break;
                case Key.Z:
                    PlayerViewModel.PreviousCommand.Execute(null);
                    break;
                case Key.V:
                    PlayerViewModel.NextCommand.Execute(null);
                    break;
                case Key.F:
                    PlayerViewModel.AddFolderCommand.Execute(null);
                    break;
                case Key.A:
                    PlayerViewModel.AddFilesCommand.Execute(null);
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
                    PlayerViewModel.RunBackCommand.Execute(null);
                    break;
                case Key.Left:
                    PlayerViewModel.RunBackCommand.Execute(null);
                    break;
                case Key.D:
                    PlayerViewModel.RunForwardCommand.Execute(null);
                    break;
                case Key.Right:
                    PlayerViewModel.RunForwardCommand.Execute(null);
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
            PlayerViewModel.PlayNext();
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            timerMoveSlider.Stop();
            PlayerViewModel.PlayNext();
        }

        #endregion

        private void PressButton(System.Windows.Controls.Button btn, KeyEventArgs e)
        {
            e.Handled = true;
            btn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }

    }
}
