using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Noteflow.ViewModels
{
    public partial class NewCardFormularViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        public ICommand SaveCommand { get; }

        public NewCardFormularViewModel()
        {
            SaveCommand = new RelayCommand(OnSave);
        }

        private void OnSave()
        {
            // Hier kannst du die Logik zum Speichern der neuen Karte in der JSON-Datei implementieren
            // Zum Beispiel: Speichere `Title` und `Content` in der JSON-Datei
        }
    }
}