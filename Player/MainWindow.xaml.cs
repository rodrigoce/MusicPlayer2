using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
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

        private PlayListFileStorage playListFileStorage = new PlayListFileStorage();

        private void IsPlaying(bool value)
        {
            //btnPauseContinue.IsEnabled = value;
            btnBack.IsEnabled = value;
            btnForward.IsEnabled = value;
        }

        private TimeSpan totalTime;
        private DispatcherTimer timerVideoTime;
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            PlayFromList();
            IsPlaying(true);
        }
    private void timer_Tick(object sender, EventArgs e)
    {
        
        // Check if the movie finished calculate it's total time
        if ((mediaElement.NaturalDuration.HasTimeSpan) && (mediaElement.NaturalDuration.TimeSpan.TotalSeconds > 0))
        {
            if (totalTime.TotalSeconds > 0)
            {
                // Updating time slider
                timerSlider.Value = mediaElement.Position.TotalSeconds ;
            }
        }
    }

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

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(5);
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(5);
        }

        private void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                playListFileStorage.GetMusicRecursive(fd.SelectedPath);
            }
            LoadMusicList();
            playListFileStorage.Save();
        }

        private void LoadMusicList()
        {
            foreach (var music in playListFileStorage.PlayList)
            {
                listMusics.Items.Add(music.Nro + " - " + music.Name);
            }
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

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            totalTime = mediaElement.NaturalDuration.TimeSpan;
            timerSlider.Minimum = 0;
            timerSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            // Create a timer that will update the counters and the time slider
            timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            timerVideoTime.Tick += new EventHandler(timer_Tick);
            timerVideoTime.Start();
            ShowMediaInformation();
        }
        private void ShowMediaInformation()
        {
            var duration = mediaElement.NaturalDuration.HasTimeSpan
                ? mediaElement.NaturalDuration.TimeSpan.TotalSeconds.ToString("#s")
                : "No duration";
            MediaInformation.Text = duration;
        }

        private void timerSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (totalTime.TotalSeconds > 0)
            {
                mediaElement.Position = TimeSpan.FromSeconds(timerSlider.Value);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playListFileStorage.Load();
            LoadMusicList();
        }

        private void listMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PlayFromList();
            }
        }

        private void PlayFromList()
        {
            if (listMusics.SelectedIndex >= 0)
            {
                var music = playListFileStorage.PlayList.Single(c => c.Nro == listMusics.SelectedIndex + 1);
                PlayNew(music);
            }
        }

        private void MoveNext()
        {
            if (listMusics.SelectedIndex < listMusics.Items.Count -1)
            {
                listMusics.SelectedIndex++;
                PlayFromList();
            }
            else 
            {
                listMusics.SelectedIndex = 0;
                PlayFromList();
            }
        }

        private void PlayNew(Music file)
        {
            mediaElement.Source = new Uri(file.Path);
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            mediaElement.Play();
            textMusicName.Text = file.Name;
        }

        private void listMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PlayFromList();
            }
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MoveNext();
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MoveNext();
        }
    }
}
