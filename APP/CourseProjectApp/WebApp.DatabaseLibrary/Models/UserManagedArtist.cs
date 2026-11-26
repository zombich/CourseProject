using System;
using System.Collections.Generic;

namespace WebApp.DatabaseLibrary.Models;

public partial class UserManagedArtist
{
    public int UserId { get; set; }

    public int ArtistId { get; set; }

    public DateTime? GrantedAt { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
