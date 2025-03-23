using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Noteflow.ViewModels
{
    public partial class MenuBarViewModel : ViewModelBase
    {
        [RelayCommand]
        private void OpenSettings()
        {
            Console.WriteLine("OpenSettings called!");
        }

        [RelayCommand]
        private void EditBank()
        {
            Console.WriteLine("EditBank wurde ausgeführt.");
        }

        [RelayCommand]
        private void ShowHelp()
        {
            Console.WriteLine("ShowHelp called!");
        }
    }
}