using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Jojk.Models
{

    /*
      
        Uhh this one is HORRIBLE, it is so slow and I cannot make it better... I HOPE that it's just jellyfin that is slow. Also add windows control support for pause etc
      
     */

    public class AudioManager : IDisposable
    {
        private readonly ConnectionManager _connectionManager;
        private readonly LibVLC _libvlc;
        private readonly MediaPlayer _mediaPlayer;

        private Media? _currentMedia;

        public ConnectionManager ConnectionManager => _connectionManager;

        public string? CurrentSongId { get; private set; }

        public bool IsPlaying => _mediaPlayer.IsPlaying;
        public long CurrentPositionMs => _mediaPlayer.Time;

        public bool IsStoppedOrEnded =>
            _mediaPlayer.State is VLCState.Ended
                or VLCState.Stopped
                or VLCState.NothingSpecial
                or VLCState.Error;

        public event Action<float>? PositionChanged;
        public event Action<long, long>? TimeChanged; // currentMs, totalMs
        public event Action? PlaybackEnded;
        public event Action? NextRequested;
        public event Action? PreviousRequested;

        public AudioManager(ConnectionManager cM)
        {
            _connectionManager = cM;
            _libvlc = new LibVLC("--network-caching=500"); // Helps a bit, the network delays are still kinda rough tho (slow as hell)
            _mediaPlayer = new MediaPlayer(_libvlc);

            _mediaPlayer.EndReached += (_, _) => PlaybackEnded?.Invoke();
            _mediaPlayer.PositionChanged += (_, e) => PositionChanged?.Invoke(e.Position);
            _mediaPlayer.TimeChanged += (_, e) => TimeChanged?.Invoke(e.Time, _mediaPlayer.Length);
        }

        public async Task<string> GetLibraryIDAsync()
        {
            var jellyfinViews = await _connectionManager.HTTPClient.GetFromJsonAsync<JsonElement>(
                $"/Users/{_connectionManager.UserID}/Views"
            );

            return jellyfinViews.GetProperty("Items")
                .EnumerateArray()
                .First(x => x.GetProperty("CollectionType").GetString() == "music")
                .GetProperty("Id")
                .GetString()!;
        }

        public async Task<List<Song>> GetSongsAsync()
        {
            var libraryID = await GetLibraryIDAsync();

            var query = $"/Users/{_connectionManager.UserID}/Items?ParentId={libraryID}&IncludeItemTypes=Audio&Recursive=true&Fields=MediaSources%2CPath%2CPrimaryImageTag&StartIndex=0&SortBy=SortName&SortOrder=Ascending";

            Debug.WriteLine($"Request URL: {query}");

            var json = await _connectionManager.HTTPClient.GetFromJsonAsync<JsonElement>(query);

            return json.GetProperty("Items")
                .EnumerateArray()
                .Select(x => new Song(
                    ID: x.GetProperty("Id").GetString()!,
                    Name: x.GetProperty("Name").GetString()!,
                    Artist: x.TryGetProperty("AlbumArtist", out var a) ? a.GetString() ?? "Unknown" : "Unknown",
                    Album: x.TryGetProperty("Album", out var al) ? al.GetString() ?? "Unknown" : "Unknown",
                    AlbumArtPath: _connectionManager.AlbumArtPath(x.GetProperty("Id").GetString()!, 300, 300)
                )).ToList();
        }
        public void RequestNext() => NextRequested?.Invoke();
        public void RequestPrevious() => PreviousRequested?.Invoke();

        public async Task PlayAsync(string songId, string streamPath, bool forceReload = false)
        {
            if (!forceReload && CurrentSongId == songId && _currentMedia != null && !IsStoppedOrEnded)
            {
                _mediaPlayer.SetPause(false);
                return;
            }

            await LoadAndPlayAsync(songId, streamPath);
        }

        private async Task LoadAndPlayAsync(string songId, string streamPath)
        {
            try
            {
                _mediaPlayer.Stop();

                var old = _currentMedia;
                _currentMedia = null;
                old?.Dispose();

                var media = new Media(_libvlc, new Uri(streamPath));
                await media.Parse(MediaParseOptions.ParseNetwork);

                _currentMedia = media;
                CurrentSongId = songId;

                _mediaPlayer.Play(media);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting song: {ex.Message}");
                throw;
            }
        }

        public void Pause()
        {
            if (_mediaPlayer.CanPause)
                _mediaPlayer.SetPause(true);
        }

        public void Resume()
        {
            if (_currentMedia != null && !IsStoppedOrEnded)
                _mediaPlayer.SetPause(false);
        }

        public bool TogglePause()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.SetPause(true);
                return false;
            }

            _mediaPlayer.SetPause(false);
            return true;
        }

        public void Seek(float position)
        {
            _mediaPlayer.Position = Math.Clamp(position, 0f, 1f);
        }

        public void StopSong()
        {
            _mediaPlayer.Stop();
            CurrentSongId = null;

            var old = _currentMedia;
            _currentMedia = null;
            old?.Dispose();
        }

        public void Dispose()
        {
            _mediaPlayer.Stop();
            _currentMedia?.Dispose();
            _mediaPlayer.Dispose();
            _libvlc.Dispose();
        }
    }

    public record Song(string ID, string Name, string Artist, string Album, string AlbumArtPath); // Placed at the botton beacuse it annoyed me when writing the comment at the top
}
