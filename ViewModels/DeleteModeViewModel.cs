using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Noteflow.ViewModels
{
    public partial class DeleteModeViewModel : ViewModelBase
    {
        public class CardWrapper
        {
            public required IndexCard Card { get; set; }
            public required ICommand DeleteCommand { get; set; }
        }

        private readonly CardBankManagement _cardBankManagement;

        private ObservableCollection<CardWrapper> _cards;
        public ObservableCollection<CardWrapper> Cards
        {
            get => Cards2;
            set
            {
                Cards2 = value;
                OnPropertyChanged();
                // Automatisch speichern bei Änderungen
                SaveAndReindex();
            }
        }

        public ObservableCollection<CardWrapper> Cards1 { get => Cards2; set => Cards2 = value; }
        public ObservableCollection<CardWrapper> Cards2 { get => _cards; set => _cards = value; }
        public ObservableCollection<CardWrapper> Cards3 { get => _cards; set => _cards = value; }

        public DeleteModeViewModel(CardBankManagement cardBankManagement)
        {
             _cardBankManagement = cardBankManagement;
            _cards = new ObservableCollection<CardWrapper>();  // Leere Liste initialisieren
            LoadCards();
        }

        private void LoadCards()
        {
            var loadedCards = _cardBankManagement.LoadCards();
            Cards = new ObservableCollection<CardWrapper>(
                loadedCards.Select(card => new CardWrapper 
                { 
                    Card = card,
                    DeleteCommand = new RelayCommand(() => DeleteCard(card.Id)) 
                }));
        }

     private void DeleteCard(int cardId)
{
    // 1. Originale Liste laden
    var allCards = _cardBankManagement.LoadCards();
    
    // 2. Karte aus Original-Liste entfernen
    allCards.RemoveAll(c => c.Id == cardId);
    
    // 3. IDs neu sortieren und speichern
    _cardBankManagement.ReindexCards(allCards);
    _cardBankManagement.SaveCards(allCards);
    
    // 4. UI-Liste aktualisieren
    LoadCards(); // Diese Methode lädt die Daten neu aus der JSON
    
    Debug.WriteLine($"Karte {cardId} wurde dauerhaft gelöscht.");
}

        private void SaveAndReindex()
        {
            var currentCards = Cards.Select(w => w.Card).ToList();
            _cardBankManagement.ReindexCards(currentCards);
            _cardBankManagement.SaveCards(currentCards);
            Debug.WriteLine("Karten wurden automatisch gespeichert und reindiziert.");
        }
    }
}