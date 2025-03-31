using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Services;
using Noteflow.ViewModels;

namespace Noteflow.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;
             [ObservableProperty]
        private bool _showBackButton;

        [ObservableProperty]
        private MenuBarViewModel _menuBarViewModel;

        private readonly CardBankManagement _cardBankManagement;

        // Öffentliche Property für CardBankManagement
        public CardBankManagement CardBankManagement => _cardBankManagement;

        public MainWindowViewModel()
        {
            // Erstelle eine Instanz von CardBankManagement
            _cardBankManagement = new CardBankManagement("Data/card_bank.json");

            // Standardansicht: Zeige die Karten an
            CurrentView = new CardSectionViewModel(_cardBankManagement);

            // Erstelle eine Instanz der Menüleiste
            MenuBarViewModel = new MenuBarViewModel(this);
            
            UpdateBackButtonVisibility();
        }
          partial void OnCurrentViewChanged(ViewModelBase value)
        {
            UpdateBackButtonVisibility();
        }
         private void UpdateBackButtonVisibility()
        {
            ShowBackButton = CurrentView is not CardSectionViewModel;
        }
        [RelayCommand]
        public void GoBack()
        {
            ShowCardSection();
        }
        public void ShowNewCardForm()
        {
            // Wechsle zur NewCardFormularView und übergebe CardBankManagement
            CurrentView = new NewCardFormularViewModel(_cardBankManagement, this);
        }

        public void ShowCardSection()
        {
            // Wechsle zurück zur CardSectionView
            CurrentView = new CardSectionViewModel(_cardBankManagement);
        }
        
        public void ShowDeleteMode()
        {
            // Wechsle zurück zur CardSectionView
            CurrentView = new DeleteModeViewModel(_cardBankManagement);
        }
    }
}