using Avalonia.Controls;
using Avalonia.Input;
using Noteflow.ViewModels;

namespace Noteflow.Views
{
    public partial class CardSetsView : UserControl
    {
        public CardSetsView()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            if (DataContext is CardSetsViewModel viewModel && viewModel.IsCardDetailOpen)
            {
                viewModel.CloseCardDetail();
                e.Handled = true;
            }
        }
    }
}
