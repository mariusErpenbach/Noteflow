using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Services; // Füge diese Zeile hinzu


namespace Noteflow.ViewModels
{
  public partial class CardSectionViewModel : ViewModelBase
{
    private CardBankManagement _cardBank;

    [ObservableProperty]
    private List<IndexCard> _cards = new List<IndexCard>(); // Initialisierung

    public CardSectionViewModel()
    {
        _cardBank = new CardBankManagement("Data/card_bank.json"); // Beispielpfad
        LoadCards();
    }

    private void LoadCards()
    {
        Cards = _cardBank.LoadCards();
    }

    [RelayCommand]
    private void SaveCards()
    {
        _cardBank.ReindexCards(Cards);
        _cardBank.SaveCards(Cards);
    }
}
}