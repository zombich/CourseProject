using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class Artist
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<UserManagedArtist> UserManagedArtists { get; set; } = new List<UserManagedArtist>();

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
