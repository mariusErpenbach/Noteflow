namespace Noteflow.ViewModels
{
    public class CardListViewModel : ViewModelBase
    {
        public CardBrowserViewModel Browser { get; }

        public CardListViewModel(CardBrowserViewModel browser)
        {
            Browser = browser;
        }
    }
}
