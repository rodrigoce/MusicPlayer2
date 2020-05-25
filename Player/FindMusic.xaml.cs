using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer2
{
    public enum ReturnKind { rkNone, rkMusic, rkLilter }
    /// <summary>
    /// Interaction logic for FindMusic.xaml
    /// </summary>
    public partial class FindMusic : Window
    {
        private readonly PlayList playList;

        public ReturnKind ReturnKind { get; private set; }

        public Music MusicSelected { get; private set; }

        public FindMusic(PlayList playList)
        {
            InitializeComponent();
            this.playList = playList;
            textFind.Focus();
        }

        private void TextFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            playList.Find(listBoxMusics, textFind.Text);
        }

        private void ListMusics_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SetCurrentMusicFinded();
            }
        }

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
                    if (IsDown && (listBoxMusics.SelectedIndex < (listBoxMusics.Items.Count -1)))
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
        private void ListMusics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SetCurrentMusicFinded();
            } 
        }

        private void SetCurrentMusicFinded()
        {
            if ((listBoxMusics.Items.Count > 0) && (listBoxMusics.SelectedItem != null))
            {
                playList.SetCurrentMusicFinded(listBoxMusics.SelectedItem);
                ReturnKind = ReturnKind.rkMusic;
                Close();
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
                SetCurrentMusicFinded();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentMusicFinded();
        }
    }
}
