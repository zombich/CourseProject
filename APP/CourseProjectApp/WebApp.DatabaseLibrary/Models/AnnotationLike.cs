using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class AnnotationLike
{
    public int UserId { get; set; }

    public int AnnotationId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Annotation Annotation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
