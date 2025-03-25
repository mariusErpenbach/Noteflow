using Avalonia.Controls;
using Noteflow.ViewModels;
using Noteflow.Services;
using System.Diagnostics;
namespace Noteflow.Views
{
    public partial class DeleteModeView : UserControl
    {
        public DeleteModeView()
        {
            InitializeComponent();
    this.Loaded += (s, e) => 
    {
        Debug.WriteLine($"Aktueller DataContext: {DataContext?.GetType().Name}");
    };
        }
    }
}