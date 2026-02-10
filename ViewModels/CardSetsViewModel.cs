using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Noteflow.Models;
using Noteflow.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Noteflow.ViewModels
{
    public partial class CardSetsViewModel : ViewModelBase, ICardDetailHost
    {
        private readonly CardBankManagement _cardBankManagement;
        private readonly CardSetManagement _cardSetManagement;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private CardSet? _previousSelectedSet;
        private bool _isRevertingSelection;
        private bool _pendingUnsavedChanges;
        private string _baselineSetName = string.Empty;
        private System.Collections.Generic.HashSet<int> _baselineSelectedIds = new System.Collections.Generic.HashSet<int>();

        [ObservableProperty]
        private ObservableCollection<CardSet> _sets;

        [ObservableProperty]
        private ObservableCollection<CardSelectionItem> _cardSelections;

        [ObservableProperty]
        private ObservableCollection<IndexCard> _selectedCards;

        [ObservableProperty]
        private CardSet? _selectedSet;

        [ObservableProperty]
        private string _newSetName = string.Empty;

        [ObservableProperty]
        private string _selectedSetName = string.Empty;

        public bool HasSelectedSet => SelectedSet != null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCardDetailOpen))]
        private CardDetailViewModel? _cardDetail;

        public bool IsCardDetailOpen => CardDetail != null;

        public CardSetsViewModel(CardBankManagement cardBankManagement, CardSetManagement cardSetManagement, MainWindowViewModel mainWindowViewModel, int? preselectedSetId = null)
        {
            _cardBankManagement = cardBankManagement;
            _cardSetManagement = cardSetManagement;
            _mainWindowViewModel = mainWindowViewModel;

            var sets = _cardSetManagement.LoadSets();
            foreach (var set in sets)
            {
                set.IsNew = false;
            }
            Sets = new ObservableCollection<CardSet>(sets);

            var cards = _cardBankManagement.LoadCards()
                .Where(card => !card.IsArchived)
                .ToList();
            CardSelections = new ObservableCollection<CardSelectionItem>(
                cards.Select(card => new CardSelectionItem(card)));

            SelectedCards = new ObservableCollection<IndexCard>();
            foreach (var item in CardSelections)
            {
                item.PropertyChanged += CardSelectionChanged;
            }

            if (Sets.Count > 0)
            {
                if (preselectedSetId.HasValue)
                {
                    var newSet = Sets.FirstOrDefault(s => s.Id == preselectedSetId.Value);
                    if (newSet != null)
                    {
                        newSet.IsNew = true;
                    }
                    SelectedSet = newSet ?? Sets.First();
                }
                else
                {
                    SelectedSet = Sets.First();
                }
            }
        }

        [RelayCommand]
        private async Task NewSetAsync()
        {
            await _mainWindowViewModel.TryNavigateAsync(_mainWindowViewModel.CreateNewCardSetViewModel);
        }

        partial void OnSelectedSetChanging(CardSet? value)
        {
            if (_isRevertingSelection)
            {
                return;
            }

            _previousSelectedSet = SelectedSet;
            _pendingUnsavedChanges = HasUnsavedChanges();
        }

        partial void OnSelectedSetChanged(CardSet? value)
        {
            _ = HandleSelectedSetChangedAsync(value);
        }

        [RelayCommand]
        private void CreateSet()
        {
            var name = NewSetName.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var newSet = new CardSet
            {
                Id = Sets.Count + 1,
                Name = name,
                CardIds = new System.Collections.Generic.List<int>()
            };
            Sets.Add(newSet);
            SelectedSet = newSet;
            NewSetName = string.Empty;
            SaveAllSets();
        }

        [RelayCommand]
        private async Task DeleteSetAsync()
        {
            if (SelectedSet == null)
            {
                return;
            }

            var confirmed = await ShowDeleteSetConfirmDialogAsync();
            if (!confirmed)
            {
                return;
            }

            Sets.Remove(SelectedSet);
            SelectedSet = Sets.FirstOrDefault();
            SaveAllSets();
        }

        [RelayCommand]
        private void SaveSet()
        {
            if (SelectedSet == null)
            {
                return;
            }

            SelectedSet.Name = SelectedSetName.Trim();
            SelectedSet.CardIds = CardSelections
                .Where(c => c.IsSelected)
                .Select(c => c.Card.Id)
                .ToList();

            SaveAllSets();
            UpdateBaselineFromCurrent();
        }

        private void UpdateSelections()
        {
            var selectedIds = SelectedSet?.CardIds ?? new System.Collections.Generic.List<int>();
            foreach (var item in CardSelections)
            {
                item.IsSelected = selectedIds.Contains(item.Card.Id);
            }
            UpdateSelectionVisualStates();
        }

        private void CardSelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CardSelectionItem.IsSelected))
            {
                return;
            }

            if (sender is not CardSelectionItem item)
            {
                return;
            }

            if (item.IsSelected)
            {
                if (!SelectedCards.Any(c => c.Id == item.Card.Id))
                {
                    SelectedCards.Add(item.Card);
                }
            }
            else
            {
                var existing = SelectedCards.FirstOrDefault(c => c.Id == item.Card.Id);
                if (existing != null)
                {
                    SelectedCards.Remove(existing);
                }
            }

            UpdateSelectionVisualState(item);
        }

        private void RefreshSelectedCards()
        {
            SelectedCards.Clear();
            foreach (var item in CardSelections.Where(c => c.IsSelected))
            {
                SelectedCards.Add(item.Card);
            }
            UpdateSelectionVisualStates();
        }

        public void RefreshCardSection()
        {
            var cards = _cardBankManagement.LoadCards()
                .Where(card => !card.IsArchived)
                .ToList();

            var selectedIds = SelectedSet?.CardIds?.ToHashSet() ?? new System.Collections.Generic.HashSet<int>();
            CardSelections = new ObservableCollection<CardSelectionItem>(
                cards.Select(card =>
                {
                    var item = new CardSelectionItem(card)
                    {
                        IsSelected = selectedIds.Contains(card.Id)
                    };
                    item.PropertyChanged += CardSelectionChanged;
                    return item;
                }));

            RefreshSelectedCards();
            UpdateSelectionVisualStates();
        }

        private void SaveAllSets()
        {
            var sets = Sets.ToList();
            _cardSetManagement.ReindexSets(sets);
            _cardSetManagement.SaveSets(sets);
        }

        private async Task HandleSelectedSetChangedAsync(CardSet? value)
        {
            if (_isRevertingSelection)
            {
                return;
            }

            if (_previousSelectedSet != null && _pendingUnsavedChanges)
            {
                var confirmed = await ShowDiscardChangesDialogAsync();
                if (!confirmed)
                {
                    _isRevertingSelection = true;
                    SelectedSet = _previousSelectedSet;
                    _isRevertingSelection = false;
                    return;
                }
            }

            _pendingUnsavedChanges = false;
            SelectedSetName = value?.Name ?? string.Empty;
            UpdateSelections();
            RefreshSelectedCards();
            UpdateBaselineFromCurrent();
            OnPropertyChanged(nameof(HasSelectedSet));
        }

        private bool HasUnsavedChanges()
        {
            var currentName = SelectedSetName.Trim();
            if (!string.Equals(currentName, _baselineSetName, System.StringComparison.Ordinal))
            {
                return true;
            }

            var currentIds = CardSelections.Where(c => c.IsSelected).Select(c => c.Card.Id).ToHashSet();
            return !_baselineSelectedIds.SetEquals(currentIds);
        }

        public bool HasUnsavedChangesForCurrentSet()
        {
            if (SelectedSet == null)
            {
                return false;
            }

            return HasUnsavedChanges();
        }

        public Task<bool> ConfirmDiscardChangesAsync()
        {
            return ShowDiscardChangesDialogAsync();
        }

        private void UpdateBaselineFromCurrent()
        {
            _baselineSetName = SelectedSetName.Trim();
            _baselineSelectedIds = CardSelections
                .Where(c => c.IsSelected)
                .Select(c => c.Card.Id)
                .ToHashSet();
            UpdateSelectionVisualStates();
        }

        private void UpdateSelectionVisualStates()
        {
            foreach (var item in CardSelections)
            {
                UpdateSelectionVisualState(item);
            }
        }

        private void UpdateSelectionVisualState(CardSelectionItem item)
        {
            var isSaved = item.IsSelected && _baselineSelectedIds.Contains(item.Card.Id);
            item.IsSavedSelection = isSaved;
            item.IsDirtySelection = item.IsSelected && !isSaved;
        }

        private async Task<bool> ShowDiscardChangesDialogAsync()
        {
            var window = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            var dialog = new Window
            {
                Title = "Ungespeicherte Aenderungen",
                Width = 420,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Du hast ungespeicherte Aenderungen. Trotzdem wechseln?",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Spacing = 8,
                            Children =
                            {
                                new Button { Content = "Abbrechen" },
                                new Button { Content = "Wechseln" }
                            }
                        }
                    }
                }
            };

            var tcs = new TaskCompletionSource<bool>();
            if (dialog.Content is StackPanel root &&
                root.Children[1] is StackPanel buttons &&
                buttons.Children[0] is Button cancelButton &&
                buttons.Children[1] is Button okButton)
            {
                cancelButton.Click += (_, __) =>
                {
                    tcs.TrySetResult(false);
                    dialog.Close();
                };
                okButton.Click += (_, __) =>
                {
                    tcs.TrySetResult(true);
                    dialog.Close();
                };
            }

            if (window != null)
            {
                await dialog.ShowDialog(window);
            }
            else
            {
                dialog.Show();
            }

            return await tcs.Task;
        }

        private async Task<bool> ShowDeleteSetConfirmDialogAsync()
        {
            var window = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            var dialog = new Window
            {
                Title = "Set löschen",
                Width = 420,
                Height = 190,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Möchtest du dieses Set wirklich löschen? Dieser Vorgang ist endgültig.",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Spacing = 8,
                            Children =
                            {
                                new Button { Content = "Abbrechen" },
                                new Button { Content = "Löschen" }
                            }
                        }
                    }
                }
            };

            var tcs = new TaskCompletionSource<bool>();
            if (dialog.Content is StackPanel root &&
                root.Children[1] is StackPanel buttons &&
                buttons.Children[0] is Button cancelButton &&
                buttons.Children[1] is Button okButton)
            {
                cancelButton.Click += (_, __) =>
                {
                    tcs.TrySetResult(false);
                    dialog.Close();
                };
                okButton.Click += (_, __) =>
                {
                    tcs.TrySetResult(true);
                    dialog.Close();
                };
            }

            if (window != null)
            {
                await dialog.ShowDialog(window);
            }
            else
            {
                dialog.Show();
            }

            return await tcs.Task;
        }

        [RelayCommand]
        private void OpenCardDetail(IndexCard card)
        {
            CardDetail = new CardDetailViewModel(card, _cardBankManagement, this);
        }

        public void CloseCardDetail()
        {
            CardDetail = null;
        }
    }

    public partial class CardSelectionItem : ObservableObject
    {
        public IndexCard Card { get; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isSavedSelection;

        [ObservableProperty]
        private bool _isDirtySelection;

        public CardSelectionItem(IndexCard card)
        {
            Card = card;
        }
    }
}
