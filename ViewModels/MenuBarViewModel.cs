using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
namespace Noteflow.ViewModels
{
    public partial class MenuBarViewModel : ViewModelBase
    {
        [ObservableProperty]
        private MainWindowViewModel _mainWindowViewModel;

        public ICommand EditBankCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand NewCardCommand { get; }
        public ICommand DeleteCardCommand { get; }
        public ICommand FilterCardCommand {get; }

        public MenuBarViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            EditBankCommand = new RelayCommand(OnEditBank);
            ShowHelpCommand = new RelayCommand(OnShowHelp);
            NewCardCommand = new RelayCommand(OnNewCard);
            DeleteCardCommand = new RelayCommand(OnDeleteCard);
            FilterCardCommand = new RelayCommand(OnFilterCard);
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
        private void OnDeleteCard()
        {
            MainWindowViewModel.ShowDeleteMode();
                  
        }
        private void OnFilterCard()
        {
            Console.WriteLine("Test Nachricht");
        }
    }
}