namespace WebApp.DatabaseLibrary.DTOs
{
    public class CriticDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int AnnotationsCount { get; set; }
        public int LikesCount { get; set; }
    }
}
