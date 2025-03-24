using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Windows.Input;

namespace Noteflow.ViewModels
{
    public partial class NewCardFormularViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _front = string.Empty;

        [ObservableProperty]
        private string _back = string.Empty;

        [ObservableProperty]
        private string _category = string.Empty;

        private readonly CardBankManagement _cardBankManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ICommand SaveCommand { get; }

        public NewCardFormularViewModel(CardBankManagement cardBankManagement, MainWindowViewModel mainWindowViewModel)
        {
            _cardBankManagement = cardBankManagement;
            _mainWindowViewModel = mainWindowViewModel;
            SaveCommand = new RelayCommand(OnSave);
        }

        private void OnSave()
        {
            // Erstelle eine neue Karte
            var newCard = new IndexCard
            {
                Id = 0, // Die ID wird später in ReindexCards aktualisiert
                Front = Front,
                Back = Back,
                Category = Category
            };

            // Lade die vorhandenen Karten
            var cards = _cardBankManagement.LoadCards();

            // Füge die neue Karte hinzu
            cards.Add(newCard);

            // Aktualisiere die IDs der Karten
            _cardBankManagement.ReindexCards(cards);

            // Speichere die aktualisierte Liste
            _cardBankManagement.SaveCards(cards);

            // Navigiere zurück zur CardSectionView
            _mainWindowViewModel.ShowCardSection();
        }
    }
}