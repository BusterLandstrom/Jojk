using Jojk.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Jojk.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ConnectionManager? CM { get; set; }
        public AudioManager? AM { get; set; }
        public PlayerViewModel? Player { get; private set; }

        private SongsViewModel? _songs;

        private ViewModelBase _contentPanel;
        public ViewModelBase ContentPanel
        {
            get => _contentPanel;
            set => this.RaiseAndSetIfChanged(ref _contentPanel, value);
        }

        public ICommand CloseApp { get; }

        public MainWindowViewModel()
        {
            ContentPanel = new ConnectViewModel(this);

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

        public void ShowSongs()
        {
            if (_songs != null) ContentPanel = _songs;
        }

        public void ShowPlayback() => ContentPanel = new PlaybackViewModel(this);
    }
}
