using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Services;
using Noteflow.ViewModels;
using System;
using System.Threading.Tasks;

namespace Noteflow.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;
             [ObservableProperty]
        private bool _showBackButton;

        [ObservableProperty]
        private MenuBarViewModel _menuBarViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCardDetailOpen))]
        private CardDetailViewModel? _cardDetail;

        public bool IsCardDetailOpen => CardDetail != null;

        private readonly CardBankManagement _cardBankManagement;
        private readonly CardSetManagement _cardSetManagement;
        private readonly IAnswerEvaluator _answerEvaluator;

        // Öffentliche Property für CardBankManagement
        public CardBankManagement CardBankManagement => _cardBankManagement;
        public CardSetManagement CardSetManagement => _cardSetManagement;

        public MainWindowViewModel()
        {
            // Erstelle eine Instanz von CardBankManagement
            _cardBankManagement = new CardBankManagement("Data/card_bank.json");
            _cardSetManagement = new CardSetManagement("Data/card_sets.json");
            _answerEvaluator = new LlamaSharpAnswerEvaluator("Assets/Models/mistral-7b-instruct-v0.2.Q4_K_M.gguf");

            // Standardansicht: Zeige die Karten an
            CurrentView = CreateCardSectionViewModel();

            // Erstelle eine Instanz der Menüleiste
            MenuBarViewModel = new MenuBarViewModel(this);
            
            UpdateBackButtonVisibility();
        }
          partial void OnCurrentViewChanged(ViewModelBase value)
        {
            UpdateBackButtonVisibility();
        }
         private void UpdateBackButtonVisibility()
        {
            ShowBackButton = CurrentView is not CardSectionViewModel;
        }
        [RelayCommand]
        public async Task GoBackAsync()
        {
            await TryNavigateAsync(CreateCardSectionViewModel);
        }
        public void ShowNewCardForm()
        {
            // Wechsle zur NewCardFormularView und übergebe CardBankManagement
            CurrentView = CreateNewCardFormularViewModel();
        }

        public void ShowCardSection()
        {
            // Wechsle zurück zur CardSectionView
            CurrentView = CreateCardSectionViewModel();
        }
        
        public void ShowDeleteMode()
        {
            // Wechsle zurück zur CardSectionView
            CurrentView = CreateDeleteModeViewModel();
        }
        public void ShowCardArchive()
        {
            CurrentView = CreateCardArchiveViewModel();
        }
        public void ShowCardDetail(Models.IndexCard card)
        {
            CardDetail = new CardDetailViewModel(card, _cardBankManagement, this);
        }

        public void CloseCardDetail()
        {
            CardDetail = null;
        }

        public void RefreshCardSection()
        {
            if (CurrentView is CardSectionViewModel cardSection)
            {
                cardSection.ReloadCards();
            }
        }
        public void ShowCardSets(int? preselectedSetId = null)
        {
            CurrentView = CreateCardSetsViewModel(preselectedSetId);
        }

        public void ShowNewSetForm()
        {
            CurrentView = CreateNewCardSetViewModel();
        }

        public void ShowLearningMode()
        {
            ShowLearningMode(null, autoStart: false);
        }

        public void ShowLearningMode(int? preselectedSetId, bool autoStart)
        {
            CurrentView = CreateLearningModeViewModel(preselectedSetId, autoStart);
        }

        public void ShowLearningSetup()
        {
            CurrentView = CreateLearningSetupViewModel();
        }

        public ViewModelBase CreateCardSectionViewModel()
        {
            return new CardSectionViewModel(_cardBankManagement, this);
        }

        public ViewModelBase CreateNewCardFormularViewModel()
        {
            return new NewCardFormularViewModel(_cardBankManagement, this);
        }

        public ViewModelBase CreateDeleteModeViewModel()
        {
            return new DeleteModeViewModel(_cardBankManagement);
        }

        public ViewModelBase CreateCardArchiveViewModel()
        {
            return new CardArchiveViewModel(_cardBankManagement);
        }

        public ViewModelBase CreateCardSetsViewModel(int? preselectedSetId = null)
        {
            return new CardSetsViewModel(_cardBankManagement, _cardSetManagement, this, preselectedSetId);
        }

        public ViewModelBase CreateNewCardSetViewModel()
        {
            return new NewCardSetViewModel(_cardSetManagement, this);
        }

        public ViewModelBase CreateLearningModeViewModel(int? preselectedSetId = null, bool autoStart = false)
        {
            return new LearningModeViewModel(_cardBankManagement, _cardSetManagement, _answerEvaluator, preselectedSetId, autoStart);
        }

        public ViewModelBase CreateLearningSetupViewModel()
        {
            return new LearningSetupViewModel(_cardSetManagement, this);
        }

        private async Task<bool> ConfirmLeaveCardSetsAsync()
        {
            if (CurrentView is CardSetsViewModel cardSets && cardSets.HasUnsavedChangesForCurrentSet())
            {
                return await cardSets.ConfirmDiscardChangesAsync();
            }

            return true;
        }

        public async Task<bool> TryNavigateAsync(Func<ViewModelBase> createView)
        {
            if (!await ConfirmLeaveCardSetsAsync())
            {
                return false;
            }

            CurrentView = createView();
            return true;
        }
    }
}

