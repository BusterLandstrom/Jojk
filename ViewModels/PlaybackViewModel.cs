using ReactiveUI;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Jojk.ViewModels
{
    public class PlaybackViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWVM;

        public PlayerViewModel Player => _mainWVM.Player;

        public ICommand BackCommand { get; }

        public PlaybackViewModel(MainWindowViewModel mwvm)
        {
            _mainWVM = mwvm;

            var back = ReactiveCommand.Create(() => _mainWVM.ShowSongs());
            back.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"back failed: {ex.Message}"));
            BackCommand = back;
        }
    }
}
