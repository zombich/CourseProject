namespace WebApp.DatabaseLibrary.DTOs
{
    public class TrackDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Lyrics { get; set; } = string.Empty;
        public int TrackNumber { get; set; }
        public int DurationSeconds { get; set; }
        public long ViewsCount { get; set; }
        public string? About { get; set; }
        public bool IsLyricsVerified { get; set; }
        public string? LyricsVerificationLabel { get; set; }
        public bool CanManageTrack { get; set; }
        public int AlbumId { get; set; }
        public string AlbumImage { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public List<ArtistDTO> Artists { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<AnnotationDTO> Annotations { get; set; } = new();
    }
}
