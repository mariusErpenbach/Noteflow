using Avalonia.Controls;
using Noteflow.ViewModels;
using Noteflow.Services;

namespace Noteflow.Views
{
    public partial class CardSectionView : UserControl
    {
        public CardSectionView()
        {
            InitializeComponent();

            // Setze den DataContext manuell und übergebe CardBankManagement
            if (VisualRoot is MainWindow mainWindow)
            {
                DataContext = new CardSectionViewModel(mainWindow.ViewModel.CardBankManagement);
            }
        }
    }
}