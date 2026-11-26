namespace WebApp.DatabaseLibrary.DTOs
{
    public class TrackStatsCsvRowDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public string Artists { get; set; } = string.Empty;
        public long ViewsCount { get; set; }
        public int AnnotationsCount { get; set; }
        public DateOnly ReleaseDate { get; set; }
    }
}
