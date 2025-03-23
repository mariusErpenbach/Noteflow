using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Noteflow.ViewModels
{
    public partial class MenuBarViewModel : ViewModelBase
    {
        [ObservableProperty]
        private MainWindowViewModel _mainWindowViewModel;

        public ICommand EditBankCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand NewCardCommand { get; }

        public MenuBarViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            EditBankCommand = new RelayCommand(OnEditBank);
            ShowHelpCommand = new RelayCommand(OnShowHelp);
            NewCardCommand = new RelayCommand(OnNewCard);
        }

        private void OnEditBank()
        {
            // Logik für "Edit Bank"
        }

        private void OnShowHelp()
        {
            // Logik für "Show Help"
        }

        private void OnNewCard()
        {
            MainWindowViewModel.ShowNewCardForm();
        }
    }
}