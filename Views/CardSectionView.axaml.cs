using Avalonia.Controls;
using Noteflow.ViewModels; 
namespace Noteflow.Views
{
    public partial class CardSectionView : UserControl
    {
        public CardSectionView()
        {
            InitializeComponent();
                DataContext = new CardSectionViewModel(); // Setze den DataContext
        }
    }
}