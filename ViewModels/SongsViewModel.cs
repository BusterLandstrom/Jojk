using Jojk.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Jojk.ViewModels
{
    public class SongsViewModel : ViewModelBase
    {
        private readonly AudioManager _audioManager;
        private readonly MainWindowViewModel _mainWVM;

        public PlayerViewModel Player => _mainWVM.Player;

        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        public ObservableCollection<SongItemViewModel> Songs { get; }

        private ObservableCollection<SongItemViewModel> _filteredSongs;
        public ObservableCollection<SongItemViewModel> FilteredSongs
        {
            get => _filteredSongs;
            set => this.RaiseAndSetIfChanged(ref _filteredSongs, value);
        }

        public ICommand TogglePlayCommand { get; }
        public ICommand OpenPlaybackCommand { get; }

        public SongsViewModel(MainWindowViewModel mwvm)
        {
            _mainWVM = mwvm;
            _audioManager = _mainWVM.AM;

            Songs = new ObservableCollection<SongItemViewModel>();
            FilteredSongs = new ObservableCollection<SongItemViewModel>();

            _ = LoadSongsAsync();

            this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(FilterSongs);

            // The row button just hands off to the shared player, passing the visible list as the queue
            var toggle = ReactiveCommand.CreateFromTask<SongItemViewModel>(s => Player.PlayOrToggle(s, FilteredSongs));
            var open = ReactiveCommand.Create(() => _mainWVM.ShowPlayback());

            toggle.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Play toggle failed: {ex.Message}"));
            open.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Open playback failed: {ex.Message}"));

            TogglePlayCommand = toggle;
            OpenPlaybackCommand = open;
        }

        private async System.Threading.Tasks.Task LoadSongsAsync()
        {
            try
            {
                var songs = await _audioManager.GetSongsAsync();
                foreach (var song in songs)
                    Songs.Add(new SongItemViewModel(song.ID, song.Name, song.Artist, song.Album, _audioManager, song.AlbumArtPath));

                FilteredSongs.Clear();
                foreach (var song in Songs)
                    FilteredSongs.Add(song);
            }
            catch (Exception ex)
            {
                // Nothing fancier than a log for now, the list just stays empty if the fetch dies
                Debug.WriteLine($"loading songs failed: {ex.Message}");
            }
        }

        private void FilterSongs(string term)
        {
            IEnumerable<SongItemViewModel> source = string.IsNullOrEmpty(term)
                ? Songs
                : Songs.Where(s =>
                      s.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                      s.ArtistAndAlbum.Contains(term, StringComparison.OrdinalIgnoreCase));

            FilteredSongs.Clear();
            foreach (var s in source)
                FilteredSongs.Add(s);
        }
    }
}
