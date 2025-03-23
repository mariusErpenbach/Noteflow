using Avalonia.Controls;
using Noteflow.ViewModels;

namespace Noteflow.Views
{
    public partial class MenuBarView : UserControl
    {
        public MenuBarView()
        {
            InitializeComponent();

            // Überprüfe, ob VisualRoot nicht null ist und cast es zu MainWindow
            if (VisualRoot is MainWindow mainWindow)
            {
                // Verwende die ViewModel-Eigenschaft von MainWindow
                DataContext = new MenuBarViewModel(mainWindow.ViewModel);
            }
        }
    }
}