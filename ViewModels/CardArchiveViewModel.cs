using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Noteflow.ViewModels
{
    public partial class CardArchiveViewModel : ViewModelBase
    {
        public class CardWrapper
        {
            public required IndexCard Card { get; set; }
            public required ICommand RestoreCommand { get; set; }
            public required ICommand DeleteCommand { get; set; }
        }

        private readonly CardBankManagement _cardBankManagement;
        private List<IndexCard> _allArchivedCards = new();

        private ObservableCollection<CardWrapper> _cards = new();

        public ObservableCollection<CardWrapper> Cards
        {
            get => _cards;
            private set => SetProperty(ref _cards, value);
        }

        public CardFilterViewModel FilterViewModel { get; }

        public CardArchiveViewModel(CardBankManagement cardBankManagement)
        {
            _cardBankManagement = cardBankManagement;
            FilterViewModel = new CardFilterViewModel();
            FilterViewModel.PropertyChanged += FilterViewModel_PropertyChanged;
            LoadCards();
        }

        private void LoadCards()
        {
            _allArchivedCards = _cardBankManagement.LoadCards()
                .Where(card => card.IsArchived)
                .ToList();

            UpdateCategories();
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
            var filtered = _allArchivedCards.AsEnumerable();
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

            Cards = new ObservableCollection<CardWrapper>(
                filtered.Select(card => new CardWrapper
                {
                    Card = card,
                    RestoreCommand = new RelayCommand(() => RestoreCard(card.Id)),
                    DeleteCommand = new RelayCommand(() => DeleteCard(card.Id))
                }));
        }

        private void UpdateCategories()
        {
            var categories = _allArchivedCards.Select(c => c.Category)
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
        }

        private void RestoreCard(int cardId)
        {
            var allCards = _cardBankManagement.LoadCards();
            var card = allCards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
            {
                return;
            }

            card.IsArchived = false;
            _cardBankManagement.SaveCards(allCards);
            LoadCards();
        }

        private void DeleteCard(int cardId)
        {
            var allCards = _cardBankManagement.LoadCards();
            var removed = allCards.RemoveAll(c => c.Id == cardId);
            if (removed == 0)
            {
                return;
            }

            _cardBankManagement.ReindexCards(allCards);
            _cardBankManagement.SaveCards(allCards);
            LoadCards();
        }
    }
}
