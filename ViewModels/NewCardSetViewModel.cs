using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Linq;

namespace Noteflow.ViewModels
{
    public partial class NewCardSetViewModel : ViewModelBase
    {
        private readonly CardSetManagement _cardSetManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;

        [ObservableProperty]
        private string _name = string.Empty;

        public NewCardSetViewModel(CardSetManagement cardSetManagement, MainWindowViewModel mainWindowViewModel)
        {
            _cardSetManagement = cardSetManagement;
            _mainWindowViewModel = mainWindowViewModel;
        }

        [RelayCommand]
        private void Save()
        {
            var trimmed = Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return;
            }

            var sets = _cardSetManagement.LoadSets();
            var newSet = new CardSet
            {
                Id = sets.Count + 1,
                Name = trimmed
            };
            sets.Add(newSet);
            _cardSetManagement.ReindexSets(sets);
            _cardSetManagement.SaveSets(sets);
            _mainWindowViewModel.ShowCardSets(newSet.Id);
        }

        [RelayCommand]
        private void Cancel()
        {
            _mainWindowViewModel.ShowCardSets();
        }
    }
}
