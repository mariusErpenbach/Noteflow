using CommunityToolkit.Mvvm.ComponentModel;
using Noteflow.ViewModels;

namespace Noteflow.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private MenuBarViewModel _menuBarViewModel;

        public MainWindowViewModel()
        {
            // Standardansicht: Zeige die Karten an
            CurrentView = new CardSectionViewModel(); // Ersetze dies durch dein Standard-ViewModel

            // Erstelle eine Instanz der Menüleiste
            MenuBarViewModel = new MenuBarViewModel(this);
        }

        public void ShowNewCardForm()
        {
            // Wechsle zur NewCardFormularView
            CurrentView = new NewCardFormularViewModel();
        }
    }
}