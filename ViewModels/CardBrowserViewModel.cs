using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Noteflow.ViewModels
{
    public partial class CardBrowserViewModel : ViewModelBase
    {
        public CardFilterViewModel FilterViewModel { get; }

        [ObservableProperty]
        private List<IndexCard> _cards;

        [ObservableProperty]
        private List<IndexCard> _allCards; // Originale, ungefilterte Liste

        private readonly CardBankManagement _cardBankManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public CardBrowserViewModel(CardBankManagement cardBankManagement, MainWindowViewModel mainWindowViewModel)
        {
            _cardBankManagement = cardBankManagement;
            _mainWindowViewModel = mainWindowViewModel;

            _cardBankManagement.CardsChanged += OnCardsChanged;

            _allCards = _cardBankManagement.LoadCards()
                .Where(card => !card.IsArchived)
                .ToList();
            _cards = new List<IndexCard>(_allCards);

            FilterViewModel = new CardFilterViewModel();
            UpdateCategories();
            FilterViewModel.PropertyChanged += FilterViewModel_PropertyChanged;
        }

        private void OnCardsChanged()
        {
            ReloadCards();
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

        public void ReloadCards()
        {
            AllCards = _cardBankManagement.LoadCards()
                .Where(card => !card.IsArchived)
                .ToList();
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
            if (!string.IsNullOrWhiteSpace(FilterViewModel.SelectedCategory) &&
                FilterViewModel.SelectedCategory != "Alle Kategorien")
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
