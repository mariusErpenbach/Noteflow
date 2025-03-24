using CommunityToolkit.Mvvm.ComponentModel;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;

namespace Noteflow.ViewModels
{
    public partial class CardSectionViewModel : ViewModelBase
    {
        [ObservableProperty]
        private List<IndexCard> _cards;

        private readonly CardBankManagement _cardBankManagement;

        public CardSectionViewModel(CardBankManagement cardBankManagement)
        {
            _cardBankManagement = cardBankManagement;
            _cards = _cardBankManagement.LoadCards();
        }
    }
}