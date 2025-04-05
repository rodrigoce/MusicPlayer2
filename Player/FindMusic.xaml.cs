using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayer2
{
    public enum ReturnKind { rkNone, rkMusic, rkFilter }
    /// <summary>
    /// Interaction logic for FindMusic.xaml
    /// </summary>
    public partial class FindMusic : MetroWindow
    {
        public FindMusic(PlayerViewModel playList)
        {
            InitializeComponent();
            this.playList = playList;
            ReturnKind = ReturnKind.rkNone;
            textFind.Focus();
        }

        #region Props

        private PlayerViewModel playList { get; set; }

        public ReturnKind ReturnKind { get; private set; }

        public Music MusicSelected { get; private set; }

        #endregion

        #region clicks

        private void ListMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SetCurrentMusicFound();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentMusicFound();
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            //playList.PlayFiltered();
            ReturnKind = ReturnKind.rkFilter;
            Close();
        }

        #endregion

        #region keyborad events

        private void TextFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            playList.Filter(listBoxMusics, textFind.Text);
        }
                
        private void ListMusics_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SetCurrentMusicFound();
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

        private void TextFind_PreviewKeyDown(object sender, KeyEventArgs e)
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
                if (listBoxMusics.SelectedItem == null)
                    MoveSelectedItem(true);
                SetCurrentMusicFound();
            }
        }

        #endregion

        #region methods

        private void MoveSelectedItem(bool IsDown)
        {
            if (listBoxMusics.Items.Count > 0)
            {
                if (listBoxMusics.SelectedItem == null)
                {
                    listBoxMusics.SelectedIndex = 0;
                }
                else
                {
                    if (IsDown && (listBoxMusics.SelectedIndex < (listBoxMusics.Items.Count - 1)))
                    {
                        listBoxMusics.SelectedIndex++;
                    }
                    else if ((!IsDown) && (listBoxMusics.SelectedIndex > 0))
                    {
                        listBoxMusics.SelectedIndex--;
                    }
                    listBoxMusics.ScrollIntoView(listBoxMusics.SelectedItem);
                }
            }
        }

        

        private void SetCurrentMusicFound()
        {
            if ((listBoxMusics.Items.Count > 0) && (listBoxMusics.SelectedItem != null))
            {
                //playList.SetCurrentMusicFound(listBoxMusics.SelectedItem);
                ReturnKind = ReturnKind.rkMusic;
                Close();
            }
        }

        #endregion
    }
}
