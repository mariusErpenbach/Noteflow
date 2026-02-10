namespace Noteflow.ViewModels
{
    public class CardGridViewModel : ViewModelBase
    {
        public CardBrowserViewModel Browser { get; }

        public CardGridViewModel(CardBrowserViewModel browser)
        {
            Browser = browser;
        }
    }
}
