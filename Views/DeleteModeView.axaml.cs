using Avalonia.Controls;
using Noteflow.ViewModels;
using Noteflow.Services;

namespace Noteflow.Views
{
    public partial class DeleteModeView : UserControl
    {
        public DeleteModeView()
        {
            InitializeComponent();

            // Setze den DataContext manuell und übergebe CardBankManagement
            if (VisualRoot is MainWindow mainWindow)
            {
                DataContext = new DeleteModeViewModel(mainWindow.ViewModel.CardBankManagement);
            }
        }
    }
}