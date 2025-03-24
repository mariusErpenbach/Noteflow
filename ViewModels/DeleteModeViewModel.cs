using CommunityToolkit.Mvvm.ComponentModel;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;

namespace Noteflow.ViewModels
{
    public partial class DeleteModeViewModel : ViewModelBase
    {
        
        [ObservableProperty]
        private List<IndexCard> _cards;


        private readonly CardBankManagement _cardBankManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public DeleteModeViewModel(CardBankManagement cardBankManagement)
        {
            _cardBankManagement = cardBankManagement;
            _cards = _cardBankManagement.LoadCards();
        }

        private void OnSave()
        {
            // Navigiere zurück zur CardSectionView
            _mainWindowViewModel.ShowCardSection();
        }
    }
}