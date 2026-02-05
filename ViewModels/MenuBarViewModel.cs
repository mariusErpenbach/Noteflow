using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using Avalonia.Controls.Notifications;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Noteflow.Models;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Noteflow.ViewModels
{
    public partial class MenuBarViewModel : ViewModelBase
    {
        [ObservableProperty]
        private MainWindowViewModel _mainWindowViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBankActive))]
        [NotifyPropertyChangedFor(nameof(IsSetsActive))]
        [NotifyPropertyChangedFor(nameof(IsLearningActive))]
        private MenuSection _activeSection = MenuSection.Bank;

        public bool IsBankActive => ActiveSection == MenuSection.Bank;
        public bool IsSetsActive => ActiveSection == MenuSection.Sets;
        public bool IsLearningActive => ActiveSection == MenuSection.Lernen;

        public ICommand EditBankCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand NewCardCommand { get; }
        public ICommand DeleteCardCommand { get; }
        public ICommand OpenArchiveCommand { get; }
        public ICommand ImportBankCommand { get; }
        public ICommand OpenSetsCommand { get; }
        public ICommand OpenLearningModeCommand { get; }

        public MenuBarViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            EditBankCommand = new AsyncRelayCommand(OnEditBankAsync);
            ShowHelpCommand = new RelayCommand(OnShowHelp);
            NewCardCommand = new AsyncRelayCommand(OnNewCardAsync);
            DeleteCardCommand = new AsyncRelayCommand(OnDeleteCardAsync);
            OpenArchiveCommand = new AsyncRelayCommand(OnOpenArchiveAsync);
            ImportBankCommand = new RelayCommand(OnImportBank);
            OpenSetsCommand = new AsyncRelayCommand(OnOpenSetsAsync);
            OpenLearningModeCommand = new AsyncRelayCommand(OnOpenLearningModeAsync);

            UpdateActiveSectionFromCurrentView();
            _mainWindowViewModel.PropertyChanged += MainWindowViewModelOnPropertyChanged;
        }

        private void MainWindowViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.CurrentView))
            {
                UpdateActiveSectionFromCurrentView();
            }
        }

        private void UpdateActiveSectionFromCurrentView()
        {
            ActiveSection = MainWindowViewModel.CurrentView switch
            {
                CardSetsViewModel or NewCardSetViewModel => MenuSection.Sets,
                LearningSetupViewModel or LearningModeViewModel => MenuSection.Lernen,
                _ => MenuSection.Bank
            };
        }
        private async void OnImportBank()
        {
            // Versuche das aktuelle Fenster zu finden
            var window = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window == null)
                return;

            var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Kartenbank importieren (JSON)",
                AllowMultiple = false,
                FileTypeFilter = new List<Avalonia.Platform.Storage.FilePickerFileType>
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("JSON-Dateien") { Patterns = new[] { "*.json" } }
                }
            });

            if (files == null || files.Count == 0)
                return;

            var file = files[0];
            var filePath = file.Path.LocalPath;
            try
            {
                var json = File.ReadAllText(filePath);
                var importedCards = JsonSerializer.Deserialize<List<IndexCard>>(json);
                if (importedCards != null && importedCards.Count > 0)
                {
                    MainWindowViewModel.CardBankManagement.SaveCards(importedCards);
                    MainWindowViewModel.ShowCardSection();
                    await ShowInfoDialog(window, $"Import erfolgreich! {importedCards.Count} Karten wurden übernommen.");
                }
                else
                {
                    await ShowErrorDialog(window, "Die Datei enthält keine gültigen Karten.");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog(window, $"Fehler beim Import: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowInfoDialog(Window? window, string message)
        {
            if (window != null)
            {
                var dialog = new Window
                {
                    Title = "Import abgeschlossen",
                    Width = 400,
                    Height = 160,
                    Content = new StackPanel
                    {
                        Margin = new Avalonia.Thickness(20),
                        Children =
                        {
                            new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                            new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Margin = new Avalonia.Thickness(0,20,0,0) }
                        }
                    }
                };
                if (dialog.Content is StackPanel sp && sp.Children[1] is Button btn)
                {
                    btn.Click += (_, __) => dialog.Close();
                }
                await dialog.ShowDialog(window);
            }
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(Window? window, string message)
        {
            if (window != null)
            {
                var dialog = new Window
                {
                    Title = "Import-Fehler",
                    Width = 400,
                    Height = 180,
                    Content = new StackPanel
                    {
                        Margin = new Avalonia.Thickness(20),
                        Children =
                        {
                            new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                            new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Margin = new Avalonia.Thickness(0,20,0,0) }
                        }
                    }
                };
                if (dialog.Content is StackPanel sp && sp.Children[1] is Button btn)
                {
                    btn.Click += (_, __) => dialog.Close();
                }
                await dialog.ShowDialog(window);
            }
        }

        private async Task OnEditBankAsync()
        {
            await MainWindowViewModel.TryNavigateAsync(MainWindowViewModel.CreateCardSectionViewModel);
            MainWindowViewModel.CloseCardDetail();
        }

        private void OnShowHelp()
        {
            // Logik für "Show Help"
        }

        private async Task OnNewCardAsync()
        {
            await MainWindowViewModel.TryNavigateAsync(MainWindowViewModel.CreateNewCardFormularViewModel);
        }
        private async Task OnDeleteCardAsync()
        {
            await MainWindowViewModel.TryNavigateAsync(MainWindowViewModel.CreateDeleteModeViewModel);
        }
        private async Task OnOpenArchiveAsync()
        {
            await MainWindowViewModel.TryNavigateAsync(MainWindowViewModel.CreateCardArchiveViewModel);
        }
        private async Task OnOpenSetsAsync()
        {
            await MainWindowViewModel.TryNavigateAsync(() => MainWindowViewModel.CreateCardSetsViewModel());
        }

        private async Task OnOpenLearningModeAsync()
        {
            await MainWindowViewModel.TryNavigateAsync(MainWindowViewModel.CreateLearningSetupViewModel);
        }
    }

    public enum MenuSection
    {
        Bank,
        Sets,
        Lernen
    }
}
