using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;
using System.Linq;

using System.ComponentModel;

namespace Noteflow.ViewModels
{
    public partial class CardSectionViewModel : ViewModelBase
    {
                public List<string> GetAvailableCategories()
                {
                    return AllCards.Select(c => c.Category)
                        .Where(cat => !string.IsNullOrWhiteSpace(cat))
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                }
        public CardFilterViewModel FilterViewModel { get; }
        [ObservableProperty]
        private List<IndexCard> _cards;

        [ObservableProperty]
        private List<IndexCard> _allCards; // Originale, ungefilterte Liste

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCardView))]
        private bool _isListView;

        public bool IsCardView => !IsListView;

        private readonly CardBankManagement _cardBankManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public CardSectionViewModel(CardBankManagement cardBankManagement, MainWindowViewModel mainWindowViewModel)
        {
            _cardBankManagement = cardBankManagement;
            _mainWindowViewModel = mainWindowViewModel;
            _allCards = _cardBankManagement.LoadCards()
                .Where(card => !card.IsArchived)
                .ToList();
            _cards = new List<IndexCard>(_allCards); // Kopie der originalen Liste

            // FilterViewModel initialisieren und Kategorien setzen
            FilterViewModel = new CardFilterViewModel();
            UpdateCategories();
            FilterViewModel.PropertyChanged += FilterViewModel_PropertyChanged;
        }

        [RelayCommand]
        private void OpenCard(IndexCard? card)
        {
            if (card == null)
            {
                return;
            }

            _mainWindowViewModel.ShowCardDetail(card);
        }

        [RelayCommand]
        private void ToggleCardView()
        {
            IsListView = false;
        }

        [RelayCommand]
        private void ToggleListView()
        {
            IsListView = true;
        }

        public void ReloadCards()
        {
            AllCards = _cardBankManagement.LoadCards()
                .Where(card => !card.IsArchived)
                .ToList();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        private void FilterViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterViewModel.SearchText) ||
                e.PropertyName == nameof(FilterViewModel.SelectedCategory))
            {
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            var filtered = AllCards.AsEnumerable();
            var search = FilterViewModel.SearchText?.Trim().ToLower() ?? string.Empty;
            if (search.Length >= 3)
            {
                filtered = filtered.Where(card =>
                    card.Front.ToLower().Contains(search) ||
                    card.Category.ToLower().Contains(search));
            }
            if (!string.IsNullOrWhiteSpace(FilterViewModel.SelectedCategory) && FilterViewModel.SelectedCategory != "Alle Kategorien")
            {
                filtered = filtered.Where(card => card.Category == FilterViewModel.SelectedCategory);
            }
            Cards = filtered.ToList();
        }

        partial void OnAllCardsChanged(List<IndexCard> value)
        {
            FilterViewModel.TotalCards = value?.Count ?? 0;
            UpdateCategories();
            ApplyFilter();
        }

        private void UpdateCategories()
        {
            var categories = AllCards.Select(c => c.Category)
                .Where(cat => !string.IsNullOrWhiteSpace(cat))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            categories.Insert(0, "Alle Kategorien");
            FilterViewModel.Categories = categories;

            if (string.IsNullOrWhiteSpace(FilterViewModel.SelectedCategory) ||
                !categories.Contains(FilterViewModel.SelectedCategory))
            {
                FilterViewModel.SelectedCategory = "Alle Kategorien";
            }

            FilterViewModel.TotalCards = AllCards?.Count ?? 0;
        }
    }
}
