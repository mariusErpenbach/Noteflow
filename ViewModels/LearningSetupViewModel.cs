using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Noteflow.ViewModels
{
    public partial class LearningSetupViewModel : ViewModelBase
    {
        private readonly CardSetManagement _cardSetManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;

        [ObservableProperty]
        private ObservableCollection<CardSet> _sets;

        [ObservableProperty]
        private CardSet? _selectedSet;

        [ObservableProperty]
        private int _selectedSetCardCount;

        public bool HasSelectedSet => SelectedSet != null;

        public LearningSetupViewModel(
            CardSetManagement cardSetManagement,
            MainWindowViewModel mainWindowViewModel)
        {
            _cardSetManagement = cardSetManagement;
            _mainWindowViewModel = mainWindowViewModel;

            Sets = new ObservableCollection<CardSet>(_cardSetManagement.LoadSets());
            if (Sets.Count > 0)
            {
                SelectedSet = Sets.First();
            }
        }

        partial void OnSelectedSetChanged(CardSet? value)
        {
            OnPropertyChanged(nameof(HasSelectedSet));
            UpdateSelectedSetCardCount();
        }

        private void UpdateSelectedSetCardCount()
        {
            if (SelectedSet == null)
            {
                SelectedSetCardCount = 0;
                return;
            }

            SelectedSetCardCount = SelectedSet.CardIds?.Count ?? 0;
        }

        [RelayCommand]
        private void StartLearning()
        {
            if (SelectedSet == null)
            {
                return;
            }

            _mainWindowViewModel.ShowLearningMode(SelectedSet.Id, autoStart: true);
        }
    }
}
