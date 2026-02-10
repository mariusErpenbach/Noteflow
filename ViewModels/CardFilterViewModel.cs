using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows.Input;

namespace Noteflow.ViewModels
{
    public class CardFilterViewModel : ObservableObject
    {
        private string _searchText = string.Empty;
        private List<string> _categories = new();
        private string _selectedCategory = string.Empty;
        private int _totalCards;
        private bool _isCardViewActive = true;
        private bool _isListViewActive;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public List<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public int TotalCards
        {
            get => _totalCards;
            set => SetProperty(ref _totalCards, value);
        }

        public bool IsCardViewActive
        {
            get => _isCardViewActive;
            set => SetProperty(ref _isCardViewActive, value);
        }

        public bool IsListViewActive
        {
            get => _isListViewActive;
            set => SetProperty(ref _isListViewActive, value);
        }

        public ICommand SortAZCommand { get; }
        public ICommand SortZACommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand? ShowCardViewCommand { get; set; }
        public ICommand? ShowListViewCommand { get; set; }

        public CardFilterViewModel()
        {
            SortAZCommand = new RelayCommand(OnSortAZ);
            SortZACommand = new RelayCommand(OnSortZA);
            ResetFilterCommand = new RelayCommand(OnResetFilter);
        }

        private void OnSortAZ() { /* Sortierlogik */ }
        private void OnSortZA() { /* Sortierlogik */ }
        private void OnResetFilter()
        {
            SearchText = string.Empty;
            SelectedCategory = string.Empty;
        }
    }
}
