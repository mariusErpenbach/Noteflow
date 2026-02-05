using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Noteflow.ViewModels
{
    public partial class LearningModeViewModel : ViewModelBase
    {
        private readonly CardBankManagement _cardBankManagement;
        private readonly CardSetManagement _cardSetManagement;
        private readonly IAnswerEvaluator _answerEvaluator;
        private List<IndexCard> _currentCards = new List<IndexCard>();
        private int _currentIndex;
        private const int BatchSize = 10;
        private List<AnswerSubmission> _submissions = new List<AnswerSubmission>();
        private List<Task> _evaluationTasks = new List<Task>();

        [ObservableProperty]
        private ObservableCollection<CardSet> _sets;

        [ObservableProperty]
        private CardSet? _selectedSet;

        [ObservableProperty]
        private IndexCard? _currentCard;

        [ObservableProperty]
        private string _userAnswer = string.Empty;

        [ObservableProperty]
        private string _evaluationMessage = string.Empty;

        [ObservableProperty]
        private string _progressText = string.Empty;

        [ObservableProperty]
        private bool _isBatchComplete;

        [ObservableProperty]
        private string _batchResultText = string.Empty;

        [ObservableProperty]
        private bool _showBack;

        [ObservableProperty]
        private bool _isEvaluating;

        public bool HasSelectedSet => SelectedSet != null;
        public bool HasCurrentCard => CurrentCard != null;

        public LearningModeViewModel(
            CardBankManagement cardBankManagement,
            CardSetManagement cardSetManagement,
            IAnswerEvaluator answerEvaluator,
            int? preselectedSetId = null,
            bool autoStart = false)
        {
            _cardBankManagement = cardBankManagement;
            _cardSetManagement = cardSetManagement;
            _answerEvaluator = answerEvaluator;

            Sets = new ObservableCollection<CardSet>(_cardSetManagement.LoadSets());
            if (preselectedSetId != null)
            {
                SelectedSet = Sets.FirstOrDefault(set => set.Id == preselectedSetId) ?? Sets.FirstOrDefault();
            }
            else if (Sets.Count > 0)
            {
                SelectedSet = Sets.First();
            }

            if (autoStart && SelectedSet != null)
            {
                Start();
            }
        }

        partial void OnSelectedSetChanged(CardSet? value)
        {
            OnPropertyChanged(nameof(HasSelectedSet));
            ResetSession();
        }

        [RelayCommand]
        private void Start()
        {
            if (SelectedSet == null)
            {
                return;
            }

            var allCards = _cardBankManagement.LoadCards();
            var ordered = allCards
                .Where(c => !c.IsArchived)
                .Where(c => SelectedSet.CardIds.Contains(c.Id))
                .ToList();

            _currentCards = ordered;
            _currentIndex = 0;
            _submissions = new List<AnswerSubmission>();
            _evaluationTasks = new List<Task>();
            LoadCurrentCard();
        }

        [RelayCommand]
        private async Task SubmitAndNextAsync()
        {
            if (CurrentCard == null || _currentCards.Count == 0)
            {
                return;
            }

            var submission = new AnswerSubmission
            {
                Card = CurrentCard,
                UserAnswer = UserAnswer
            };
            _submissions.Add(submission);
            if (string.IsNullOrWhiteSpace(submission.UserAnswer))
            {
                submission.Result = new AnswerEvaluationResult
                {
                    Verdict = AnswerVerdict.Incorrect,
                    Message = "Falsch. Keine Antwort eingegeben."
                };
            }
            else
            {
                _evaluationTasks.Add(EvaluateSubmissionAsync(submission));
            }

            _currentIndex++;
            if (_currentIndex >= _currentCards.Count || _submissions.Count >= BatchSize)
            {
                CurrentCard = null;
                OnPropertyChanged(nameof(HasCurrentCard));
                IsBatchComplete = true;
                ProgressText = $"{_submissions.Count}/{_submissions.Count}";
                await FinishBatchAsync();
                return;
            }

            LoadCurrentCard();
        }

        private void LoadCurrentCard()
        {
            CurrentCard = _currentCards.Count > 0 ? _currentCards[_currentIndex] : null;
            UserAnswer = string.Empty;
            EvaluationMessage = string.Empty;
            ShowBack = false;
            IsBatchComplete = false;
            BatchResultText = string.Empty;
            UpdateProgress();
            OnPropertyChanged(nameof(HasCurrentCard));
        }

        private void UpdateProgress()
        {
            if (_currentCards.Count == 0)
            {
                ProgressText = "0/0";
                return;
            }

            var total = System.Math.Min(BatchSize, _currentCards.Count);
            ProgressText = $"{_currentIndex + 1}/{total}";
        }

        private void ResetSession()
        {
            _currentCards = new List<IndexCard>();
            _currentIndex = 0;
            _submissions = new List<AnswerSubmission>();
            _evaluationTasks = new List<Task>();
            CurrentCard = null;
            UserAnswer = string.Empty;
            EvaluationMessage = string.Empty;
            ShowBack = false;
            ProgressText = "0/0";
            IsBatchComplete = false;
            BatchResultText = string.Empty;
            OnPropertyChanged(nameof(HasCurrentCard));
        }

        private async Task EvaluateSubmissionAsync(AnswerSubmission submission)
        {
            var result = await _answerEvaluator.EvaluateAsync(
                submission.Card.Front,
                submission.Card.Back,
                submission.UserAnswer);
            submission.Result = result;
        }

        private async Task FinishBatchAsync()
        {
            IsEvaluating = true;
            await Task.WhenAll(_evaluationTasks);
            IsEvaluating = false;

            var correct = _submissions.Count(s => s.Result?.Verdict == AnswerVerdict.Correct);
            var incorrect = _submissions.Count(s => s.Result?.Verdict == AnswerVerdict.Incorrect);
            var errors = _submissions.Count(s => s.Result?.Verdict == AnswerVerdict.Error || s.Result?.Verdict == AnswerVerdict.NotConfigured);

            var lines = _submissions.Select((s, i) =>
            {
                var verdict = s.Result?.Verdict.ToString() ?? "Unknown";
                var message = s.Result?.Message ?? "";
                return $"{i + 1}. {verdict}: {message}";
            });

            BatchResultText = $"Ergebnis: {correct} korrekt, {incorrect} falsch, {errors} offen.\n" +
                              string.Join("\n", lines);
        }

        private class AnswerSubmission
        {
            public required IndexCard Card { get; set; }
            public string UserAnswer { get; set; } = string.Empty;
            public AnswerEvaluationResult? Result { get; set; }
        }
    }
}
