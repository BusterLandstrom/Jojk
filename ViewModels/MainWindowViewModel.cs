using System;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using Jojk.Models;
using ReactiveUI;

namespace Jojk.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ConnectionManager? CM { get; set; }
        public AudioManager? AM { get; set; }
        public PlayerViewModel? Player { get; private set; }

        private SongsViewModel? _songs;

        private WindowState _currentWindowState = WindowState.Normal;

        public WindowState CurrentWindowState
        {
            get => _currentWindowState;
            set => this.RaiseAndSetIfChanged(ref _currentWindowState, value);
        }

        private ViewModelBase _contentPanel;
        public ViewModelBase ContentPanel
        {
            get => _contentPanel;
            set => this.RaiseAndSetIfChanged(ref _contentPanel, value);
        }

        public ICommand CloseApp { get; }
        public ICommand MinimizeApp { get; }

        public MainWindowViewModel()
        {
            ContentPanel = new ConnectViewModel(this);

            MinimizeApp = ReactiveCommand.Create(MinimizeFunc);

            // This close app thing looks like shit, also I need to handle the debugging in a logging system like I've said elsewhere
            var close = ReactiveCommand.Create(() => Environment.Exit(0));
            close.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"close failed: {ex.Message}"));
            CloseApp = close;
        }

        public void NavigateTo(ViewModelBase viewModel) => ContentPanel = viewModel;

        public void StartSession()
        {
            Player = new PlayerViewModel(AM);
            _songs = new SongsViewModel(this);
            ShowSongs();
        }

        private void MinimizeFunc() 
        {
            CurrentWindowState = WindowState.Minimized;
        }

        public void ShowSongs()
        {
            if (_songs != null) ContentPanel = _songs;
        }

        public void ShowPlayback() => ContentPanel = new PlaybackViewModel(this);
    }
}
