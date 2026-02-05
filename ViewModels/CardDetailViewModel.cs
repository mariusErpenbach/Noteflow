using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Linq;

namespace Noteflow.ViewModels
{
    public partial class CardDetailViewModel : ViewModelBase
    {
        private readonly CardBankManagement _cardBankManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly int _cardId;
        private string _originalFront;
        private string _originalBack;

        [ObservableProperty]
        private string _front;

        [ObservableProperty]
        private string _back;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isArchived;

        public bool IsReadOnly => !IsEditing;
        public bool IsNotEditing => !IsEditing;
        public bool CanArchive => !IsArchived;

        public CardDetailViewModel(IndexCard card, CardBankManagement cardBankManagement, MainWindowViewModel mainWindowViewModel)
        {
            _cardBankManagement = cardBankManagement;
            _mainWindowViewModel = mainWindowViewModel;
            _cardId = card.Id;
            _front = card.Front;
            _back = card.Back;
            _originalFront = card.Front;
            _originalBack = card.Back;
            _isEditing = false;
            _isArchived = card.IsArchived;
        }

        partial void OnIsEditingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsReadOnly));
            OnPropertyChanged(nameof(IsNotEditing));
        }

        partial void OnIsArchivedChanged(bool value)
        {
            OnPropertyChanged(nameof(CanArchive));
        }

        [RelayCommand]
        private void StartEdit()
        {
            IsEditing = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            Front = _originalFront;
            Back = _originalBack;
            IsEditing = false;
        }

        [RelayCommand]
        private void SaveEdit()
        {
            var cards = _cardBankManagement.LoadCards();
            var card = cards.FirstOrDefault(c => c.Id == _cardId);
            if (card == null)
            {
                IsEditing = false;
                return;
            }

            card.Front = Front;
            card.Back = Back;
            _cardBankManagement.SaveCards(cards);

            _originalFront = Front;
            _originalBack = Back;
            IsEditing = false;
            _mainWindowViewModel.RefreshCardSection();
        }

        [RelayCommand]
        private void ArchiveCard()
        {
            var cards = _cardBankManagement.LoadCards();
            var card = cards.FirstOrDefault(c => c.Id == _cardId);
            if (card == null)
            {
                IsEditing = false;
                return;
            }

            card.IsArchived = true;
            _cardBankManagement.SaveCards(cards);
            IsArchived = true;
            IsEditing = false;
            _mainWindowViewModel.RefreshCardSection();
            _mainWindowViewModel.CloseCardDetail();
        }

        [RelayCommand]
        private void DeleteCard()
        {
            var cards = _cardBankManagement.LoadCards();
            var removed = cards.RemoveAll(c => c.Id == _cardId);
            if (removed == 0)
            {
                IsEditing = false;
                return;
            }

            _cardBankManagement.ReindexCards(cards);
            _cardBankManagement.SaveCards(cards);
            IsEditing = false;
            _mainWindowViewModel.RefreshCardSection();
            _mainWindowViewModel.CloseCardDetail();
        }

        [RelayCommand]
        private void Close()
        {
            if (IsEditing)
            {
                CancelEdit();
            }

            _mainWindowViewModel.CloseCardDetail();
        }
    }
}
