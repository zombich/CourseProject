namespace WebApp.DatabaseLibrary.DTOs
{
    public class AnnotationCommentDTO
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public int LikesCount { get; set; }
    }
}
