using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class CommentLike
{
    public int UserId { get; set; }

    public int CommentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AnnotationComment Comment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
