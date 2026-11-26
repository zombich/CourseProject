namespace WebApp.DatabaseLibrary.DTOs
{
    public class CreateAnnotationDTO
    {
        public string Content { get; set; } = string.Empty;
        public int LineStart { get; set; }
        public int LineEnd { get; set; }
        public int CharStart { get; set; }
        public int CharEnd { get; set; }
    }
}
