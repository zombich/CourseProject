using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class Annotation
{
    public int Id { get; set; }

    public int TrackId { get; set; }

    public int AuthorId { get; set; }

    public string Content { get; set; } = null!;

    public int LineStart { get; set; }

    public int LineEnd { get; set; }

    public int CharStart { get; set; }

    public int CharEnd { get; set; }

    public int? VerificationTypeId { get; set; }

    public int? VerifierId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AnnotationComment> AnnotationComments { get; set; } = new List<AnnotationComment>();

    public virtual ICollection<AnnotationLike> AnnotationLikes { get; set; } = new List<AnnotationLike>();

    public virtual User Author { get; set; } = null!;

    public virtual Track Track { get; set; } = null!;

    public virtual VerificationType? VerificationType { get; set; }

    public virtual User? Verifier { get; set; }
}
