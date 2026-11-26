namespace WebApp.DatabaseLibrary.DTOs
{
    public class AnnotationDTO
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int LineStart { get; set; }
        public int LineEnd { get; set; }
        public int CharStart { get; set; }
        public int CharEnd { get; set; }
        public string? VerificationTypeName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int CommentsCount { get; set; }
        public List<AnnotationCommentDTO> Comments { get; set; } = new();
    }
}
