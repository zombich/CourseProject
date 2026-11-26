namespace WebApp.DatabaseLibrary.DTOs
{
    public class ManageContextDTO
    {
        public List<ArtistDTO> Artists { get; set; } = new();
        public List<AlbumDTO> Albums { get; set; } = new();
        public List<TrackListItemDTO> Tracks { get; set; } = new();
        public List<GenreDTO> Genres { get; set; } = new();
        public List<CriticDTO> ArtistUsers { get; set; } = new();
        public List<CriticDTO> AllUsers { get; set; } = new();
        public List<RoleDTO> Roles { get; set; } = new();
    }
}
