using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class AnnotationComment
{
    public int Id { get; set; }

    public int AnnotationId { get; set; }

    public int AuthorId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Annotation Annotation { get; set; } = null!;

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
