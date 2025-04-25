using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicPlayer2
{
    /// <summary>
    /// Interaction logic for FindMusic.xaml
    /// </summary>
    public partial class FindMusic : MetroWindow
    {
        public FindMusic(PlayerViewModel playerViewModel)
        {
            InitializeComponent();
            this.PlayerViewModel = playerViewModel;
            FindFilterMusicViewModel.PlayerViewModel = playerViewModel;
            FindFilterMusicViewModel.HasFilter = playerViewModel.FilteredMusicsList.Filter != null;
            FindFilterMusicViewModel.Window = this;
            //
            PlayerViewModel.LoadToObservableCollection(PlayerViewModel.PlayListFileStorage.MusicFiles, FindFilterMusicViewModel.MusicsList2, 0);
            FindFilterMusicViewModel.FilteredMusicsList2 = CollectionViewSource.GetDefaultView(FindFilterMusicViewModel.MusicsList2);
            TxtFilter.Focus();
        }

        #region Props

        private PlayerViewModel PlayerViewModel { get; }
        
        public FindFilterMusicViewModel FindFilterMusicViewModel => (DataContext as FindFilterMusicViewModel);
        
        #endregion

        #region clicks

        private void ListBoxFilterMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PlayCurrentMusic();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            PlayCurrentMusic();
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            PlayerViewModel.FilteredMusicsList.Filter = item => ((Music)item).Visible;
            PlayerViewModel.FilteredMusicsList.Refresh();
            Close();
        }

        #endregion

        #region keyborad events        

        private void ListBoxFilterMusics_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                PlayCurrentMusic();
            } 
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void TxtFilter_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                MoveSelectedItem(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                MoveSelectedItem(false);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (ListBoxFilterMusics.SelectedItem == null)
                    MoveSelectedItem(true);
                PlayCurrentMusic();
            }
        }

        #endregion

        #region methods

        private void MoveSelectedItem(bool IsDown)
        {
            if (ListBoxFilterMusics.Items.Count > 0)
            {
                if (ListBoxFilterMusics.SelectedItem == null)
                {
                    ListBoxFilterMusics.SelectedIndex = 0;
                }
                else
                {
                    if (IsDown && (ListBoxFilterMusics.SelectedIndex < (ListBoxFilterMusics.Items.Count - 1)))
                    {
                        ListBoxFilterMusics.SelectedIndex++;
                    }
                    else if ((!IsDown) && (ListBoxFilterMusics.SelectedIndex > 0))
                    {
                        ListBoxFilterMusics.SelectedIndex--;
                    }
                    ListBoxFilterMusics.ScrollIntoView(ListBoxFilterMusics.SelectedItem);
                }
            }
        }               

        private void PlayCurrentMusic()
        {
            if ((ListBoxFilterMusics.Items.Count > 0) && (ListBoxFilterMusics.SelectedItem != null))
            {
                PlayerViewModel.CurrentMusic = ListBoxFilterMusics.SelectedItem as Music;
                PlayerViewModel.PlayCurrentMusic();
                Close();
            }
        }

        #endregion
    }
}
