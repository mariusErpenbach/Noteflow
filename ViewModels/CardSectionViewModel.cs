using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;
using System.Linq;

namespace Noteflow.ViewModels
{
    public partial class CardSectionViewModel : ViewModelBase
    {
        [ObservableProperty]
        private List<IndexCard> _cards;

        [ObservableProperty]
        private List<IndexCard> _allCards; // Originale, ungefilterte Liste

        [ObservableProperty]
        private string _searchText = string.Empty;

        private readonly CardBankManagement _cardBankManagement;

        public CardSectionViewModel(CardBankManagement cardBankManagement)
        {
            _cardBankManagement = cardBankManagement;
            _allCards = _cardBankManagement.LoadCards();
            _cards = new List<IndexCard>(_allCards); // Kopie der originalen Liste
        }

        partial void OnSearchTextChanged(string value)
        {
            if (value.Length < 3)
            {
                // Bei weniger als 3 Zeichen alle Karten anzeigen
                Cards = new List<IndexCard>(AllCards);
                return;
            }

            // Filter anwenden (case-insensitive)
            var lowerValue = value.ToLower();
            Cards = AllCards.Where(card =>
                card.Front.ToLower().Contains(lowerValue) ||
                card.Category.ToLower().Contains(lowerValue) ||
                card.Back.ToLower().Contains(lowerValue)
            ).ToList();
        }
    }
}