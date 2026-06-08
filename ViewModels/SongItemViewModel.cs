using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Jojk.Models;
using ReactiveUI;
using System.IO;
using System.Threading.Tasks;

namespace Jojk.ViewModels
{
    public class SongItemViewModel : ReactiveObject
	{
		private readonly AudioManager _audioManager;
		private bool _isPlaying;
        private bool _isCurrent;

        public SongItemViewModel(string id, string name, string artist, string album, AudioManager audioManager, string artPath = "")
		{
			_audioManager = audioManager;
			ID = id;
            Name = name;
            ArtistAndAlbum = $"{artist} - {album}";
            ArtPath = artPath;

			_ = LoadArtAsync(ArtPath);
		}

        public string ID { get; }
        public string Name { get; set; }
        public string ArtistAndAlbum { get; set; }
        public string ArtPath { get; }

		private Bitmap? _art;
		public Bitmap? Art { get => _art; private set => this.RaiseAndSetIfChanged(ref _art, value); }

		public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                this.RaiseAndSetIfChanged(ref _isPlaying, value);
                this.RaisePropertyChanged(nameof(PlayIcon));
            }
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
				// Missing or unreachable art just falls back to the placeholder, not worth surfacing
				Dispatcher.UIThread.Post(() => Art = null);
			}
		}

		// Whether this row is the one loaded in the player, drives the highlight
		public bool IsCurrent
        {
            get => _isCurrent;
            set => this.RaiseAndSetIfChanged(ref _isCurrent, value);
        }

        public Geometry PlayIcon => IsPlaying ? Icons.Pause : Icons.Play;
		public Geometry AlbumIcon { get; } = Icons.Album; // This is only a placeholder try to put the album in where it can be
    }
}
