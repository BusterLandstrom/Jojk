using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Jojk.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Jojk.ViewModels
{

    /*
        
        This one is chaotic I have to clean these variables up but it's almost 12 and I have to be up at 6am
    
    */

    public class PlayerViewModel : ViewModelBase
    {
        private readonly AudioManager _audioManager;
        private SongItemViewModel? _current;
        private IReadOnlyList<SongItemViewModel> _queue = Array.Empty<SongItemViewModel>();
        private bool _updatingFromPlayer;

        public PlayerViewModel(AudioManager audioManager)
        {
            _audioManager = audioManager;

            _audioManager.PositionChanged += OnPositionChanged;
            _audioManager.TimeChanged += OnTimeChanged;
            _audioManager.PlaybackEnded += OnPlaybackEnded;

            var toggle = ReactiveCommand.CreateFromTask(ToggleCurrent);
            var next = ReactiveCommand.CreateFromTask(Next);
            var prev = ReactiveCommand.CreateFromTask(Previous);
            var stop = ReactiveCommand.Create(Stop);


            // Handle these better, they are just debug outputs now, print to text or file for error management
            toggle.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"toggle failed: {ex.Message}"));
            next.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"next failed: {ex.Message}"));
            prev.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"prev failed: {ex.Message}"));
            stop.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"stop failed: {ex.Message}"));

            ToggleCurrentCommand = toggle;
            NextCommand = next;
            PreviousCommand = prev;
            StopCommand = stop;
        }

        public ICommand ToggleCurrentCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand StopCommand { get; }

        private bool _hasSelection;
        public bool HasSelection { get => _hasSelection; private set => this.RaiseAndSetIfChanged(ref _hasSelection, value); }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                this.RaiseAndSetIfChanged(ref _isPlaying, value);
                this.RaisePropertyChanged(nameof(PlayPauseIcon));
            }
        }

        public Geometry PlayPauseIcon => IsPlaying ? Icons.Pause : Icons.Play;

        public Geometry AlbumIcon { get; } = Icons.Album;

        public Geometry NextIcon { get; } = Icons.Next;
        public Geometry PrevIcon { get; } = Icons.Prev;
        public Geometry DownIcon { get; } = Icons.DownArrow;

        private string _songName = "";
        public string SongName { get => _songName; private set => this.RaiseAndSetIfChanged(ref _songName, value); }

        private string _artistAndAlbum = "";
        public string ArtistAndAlbum { get => _artistAndAlbum; private set => this.RaiseAndSetIfChanged(ref _artistAndAlbum, value); }

        private Bitmap? _art;
        public Bitmap? Art { get => _art; private set => this.RaiseAndSetIfChanged(ref _art, value); }

        private double _position;
        public double Position
        {
            get => _position;
            set
            {
                this.RaiseAndSetIfChanged(ref _position, value);
                if (!_updatingFromPlayer && HasSelection)
                    _audioManager.Seek((float)value);
            }
        }

        private string _timeText = "0:00";
        public string TimeText { get => _timeText; private set => this.RaiseAndSetIfChanged(ref _timeText, value); }

        private string _durationText = "0:00";
        public string DurationText { get => _durationText; private set => this.RaiseAndSetIfChanged(ref _durationText, value); }

        public async Task PlayOrToggle(SongItemViewModel? song, IReadOnlyList<SongItemViewModel> queue)
        {
            if (song is null) return;
            _queue = queue;

            if (_current == song)
            {
                if (_audioManager.IsStoppedOrEnded)
                    await StartTrack(song);
                else
                    SetPlaying(song, _audioManager.TogglePause());
                return;
            }

            await StartTrack(song);
        }

        private async Task ToggleCurrent()
        {
            if (_current is null) return;

            if (_audioManager.IsStoppedOrEnded)
                await StartTrack(_current);
            else
                SetPlaying(_current, _audioManager.TogglePause());
        }

        private async Task StartTrack(SongItemViewModel song)
        {
            if (_current != null && _current != song)
            {
                _current.IsPlaying = false;
                _current.IsCurrent = false;
            }

            var streamPath = _audioManager.ConnectionManager.FileStreamPath(song.ID);
            await _audioManager.PlayAsync(song.ID, streamPath, forceReload: true);

            _current = song;
            song.IsCurrent = true;
            SetPlaying(song, true);

            SongName = song.Name;
            ArtistAndAlbum = song.ArtistAndAlbum;
            HasSelection = true;
            SetPositionFromPlayer(0);

            _ = LoadArtAsync(song.ArtPath);
        }

        private async Task Next()
        {
            if (_current is null || _queue.Count == 0) return;
            var i = IndexOf(_current);
            if (i < 0) return;
            await StartTrack(_queue[(i + 1) % _queue.Count]);
        }

        private async Task Previous()
        {
            if (_current is null || _queue.Count == 0) return;

            if (_audioManager.CurrentPositionMs > 3000)
            {
                _audioManager.Seek(0f);
                return;
            }

            var i = IndexOf(_current);
            if (i < 0) return;
            await StartTrack(_queue[(i - 1 + _queue.Count) % _queue.Count]);
        }

        private int IndexOf(SongItemViewModel song)
        {
            for (int i = 0; i < _queue.Count; i++)
                if (_queue[i] == song) return i;
            return -1;
        }

        private void Stop()
        {
            _audioManager.StopSong();
            if (_current != null)
            {
                _current.IsPlaying = false;
                _current.IsCurrent = false;
            }
            _current = null;
            IsPlaying = false;
            HasSelection = false;
            SetPositionFromPlayer(0);
        }

        private void SetPlaying(SongItemViewModel song, bool playing)
        {
            song.IsPlaying = playing;
            IsPlaying = playing;
        }

        private void SetPositionFromPlayer(double p)
        {
            _updatingFromPlayer = true;
            Position = p;
            _updatingFromPlayer = false;
        }

        private async Task LoadArtAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Dispatcher.UIThread.Post(() => Art = null);
                return;
            }

            try
            {
                var bytes = await _audioManager.ConnectionManager.HTTPClient.GetByteArrayAsync(url);
                using var ms = new MemoryStream(bytes);
                var bmp = new Bitmap(ms);
                Dispatcher.UIThread.Post(() => Art = bmp);
            }
            catch
            {
                Dispatcher.UIThread.Post(() => Art = null);
            }
        }

        private void OnPositionChanged(float p) => Dispatcher.UIThread.Post(() => SetPositionFromPlayer(p));

        private void OnTimeChanged(long currentMs, long totalMs) => Dispatcher.UIThread.Post(() =>
        {
            TimeText = Format(currentMs);
            DurationText = Format(totalMs);
        });

        private void OnPlaybackEnded() => Dispatcher.UIThread.Post(async () =>
        {
            IsPlaying = false;
            if (_current != null) _current.IsPlaying = false;
            SetPositionFromPlayer(0);
            await Next();
        });

        private static string Format(long ms)
        {
            if (ms <= 0) return "0:00";
            var t = TimeSpan.FromMilliseconds(ms);
            return $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
        }
    }
}
