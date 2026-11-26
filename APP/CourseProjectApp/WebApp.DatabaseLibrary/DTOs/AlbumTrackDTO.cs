namespace WebApp.DatabaseLibrary.DTOs
{
    public class AlbumTrackDTO
    {
        public int Id { get; set; }
        public int TrackNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public long ViewsCount { get; set; }
        public List<ArtistDTO> Artists { get; set; } = new();
    }
}
