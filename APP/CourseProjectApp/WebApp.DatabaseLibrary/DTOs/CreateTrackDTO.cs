namespace WebApp.DatabaseLibrary.DTOs
{
    public class CreateTrackDTO
    {
        public int AlbumId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Lyrics { get; set; } = string.Empty;
        public int TrackNumber { get; set; } = 1;
        public int DurationSeconds { get; set; } = 180;
        public string? About { get; set; }
        public List<int> ArtistIds { get; set; } = new();
        public List<int> GenreIds { get; set; } = new();
    }
}
