using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Annotation> AnnotationAuthors { get; set; } = new List<Annotation>();

    public virtual ICollection<AnnotationComment> AnnotationComments { get; set; } = new List<AnnotationComment>();

    public virtual ICollection<AnnotationLike> AnnotationLikes { get; set; } = new List<AnnotationLike>();

    public virtual ICollection<Annotation> AnnotationVerifiers { get; set; } = new List<Annotation>();

    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserManagedArtist> UserManagedArtists { get; set; } = new List<UserManagedArtist>();
}
