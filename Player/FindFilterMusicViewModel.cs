using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

namespace MusicPlayer2
{
    public class FindFilterMusicViewModel : INotifyPropertyChanged
    {
        public FindFilterMusicViewModel()
        {
            RemoveFilterCommand = new RelayCommand(RemoveFilter);
        }

        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion

        #region props

        public PlayerViewModel PlayerViewModel { get; set; }
        
        public ObservableCollection<Music> MusicsList2 { get; set; } = new ObservableCollection<Music>();
       
        //
        private ICollectionView _filteredMusicsList2;
        public ICollectionView FilteredMusicsList2
        {
            get => _filteredMusicsList2;
            set
            {
                _filteredMusicsList2 = value;
                OnPropertyChanged();
            }
        }
        //
        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value ?? "";
                DoSearch();
                OnPropertyChanged();
            }
        }
        //
        private bool _hasFilter;
        public bool HasFilter
        {
            get => _hasFilter;
            set
            {
                _hasFilter = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand RemoveFilterCommand { get; }
        
        public MetroWindow Window { get; set; }

        #endregion

        #region methods

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DoSearch()
        {
            if (FilterText.Trim().Length > 0)
            {
                PlayerViewModel.PlayListFileStorage.MusicFiles.ForEach(item => item.Visible = false);
                var keys = FilterText.ForSearch().Split(' ');
                keys = keys.Where(c => c.Trim() != "").ToArray();
                string tokens = keys.Aggregate((i, j) => i + "|" + j);

                PlayerViewModel.PlayListFileStorage.MusicFiles.Where(c => Regex.Match(c.NameForSearch, tokens).Success).ToList()
                    .ForEach(item => item.Visible = true);
                FilteredMusicsList2.Filter = item => ((Music)item).Visible;
            }
            else
            {
                PlayerViewModel.PlayListFileStorage.MusicFiles.ForEach(item => item.Visible = true);
                FilteredMusicsList2.Filter = null;
            }
            FilteredMusicsList2.Refresh();
        }

        private void RemoveFilter()
        {
            FilterText = "";           
            PlayerViewModel.FilteredMusicsList.Filter = null;
            PlayerViewModel.FilteredMusicsList.Refresh();
            Window.Close();
        }

        #endregion
    }
}
