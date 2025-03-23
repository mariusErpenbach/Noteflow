using Avalonia.Controls;
using Noteflow.ViewModels;

namespace Noteflow.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel(); // Erstelle eine Instanz des ViewModels
            DataContext = ViewModel; // Setze den DataContext auf das ViewModel
        }

        // Definiere die ViewModel-Eigenschaft
        public MainWindowViewModel ViewModel { get; }
    }
}