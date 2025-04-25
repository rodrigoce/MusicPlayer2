using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
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

        private DispatcherTimer timerMoveSlider;

        #endregion

        #region Props
        public PlayerViewModel PlayerViewModel => (DataContext as PlayerViewModel);
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            timerMoveSlider = new DispatcherTimer();

            timerMoveSlider.Interval = TimeSpan.FromSeconds(1);
            timerMoveSlider.Tick += new EventHandler(timer_Tick);

            PlayerViewModel.MediaElement = mediaElement;
            PlayerViewModel.ListBoxMusics = listBoxMusics;
            PlayerViewModel.BtnPauseContinue = btnPauseContinue;
            PlayerViewModel.SliderPositionOfMusic = sliderPositionOfMusic;

            PlayerViewModel.LoadControlsValuesFromFile();
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

        private void ListMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                PlayerViewModel.Play();
        }

        private void ListMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PlayerViewModel.Play();
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

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.SpeedRatio = SpeedSlider.Value;
        }

        private async void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    PlayerViewModel.PlayCommand.Execute(null);
                    break;
                case Key.D2:
                    PlayerViewModel.StopCommand.Execute(null);
                    break;
                case Key.D3:
                    PlayerViewModel.PauseContinueCommand.Execute(null);
                    break;
                case Key.Space:
                    PlayerViewModel.PauseContinueCommand.Execute(null);
                    break;
                case Key.D6:
                    PlayerViewModel.PreviousCommand.Execute(null);
                    break;
                case Key.D7:
                    PlayerViewModel.NextCommand.Execute(null);
                    break;
                case Key.D8:
                    PlayerViewModel.AddFolderCommand.Execute(null);
                    break;
                case Key.D9:
                    PlayerViewModel.AddFilesCommand.Execute(null);
                    break;
                case Key.D0:
                    PlayerViewModel.ShowFinderFilterWindowCommand.Execute(null);
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
                case Key.D4:
                    PlayerViewModel.RunBackCommand.Execute(null);
                    break;
                case Key.Left:
                    PlayerViewModel.RunBackCommand.Execute(null);
                    break;
                case Key.D5:
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
            if (listBoxMusics.SelectedItem == null) return;

            var selectedMusic = listBoxMusics.SelectedItem as Music;

            var result = await this.ShowMessageAsync(
                "Confirmação",
                $"Confirma a exclusão permanente do arquivo {selectedMusic.Path}?",
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
                PlayerViewModel.DeletePermanently(selectedMusic);
        }


        #endregion

        #region media events

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
    }
}
