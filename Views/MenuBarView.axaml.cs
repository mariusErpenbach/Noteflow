using Avalonia.Controls;
using Noteflow.ViewModels;

namespace Noteflow.Views
{
    public partial class MenuBarView : UserControl
    {
        public MenuBarView()
        {
            InitializeComponent();
            AttachedToVisualTree += (_, __) => TrySetDataContext();
        }

        private void TrySetDataContext()
        {
            if (DataContext is MenuBarViewModel)
            {
                return;
            }

            if (VisualRoot is MainWindow mainWindow && mainWindow.DataContext is MainWindowViewModel vm)
            {
                DataContext = new MenuBarViewModel(vm);
            }
        }
    }
}
