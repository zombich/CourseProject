using System;
using System.Collections.Generic;
using WebApp.DatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.DatabaseLibrary.Contexts;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Album> Albums { get; set; }

    public virtual DbSet<Annotation> Annotations { get; set; }

    public virtual DbSet<AnnotationComment> AnnotationComments { get; set; }

    public virtual DbSet<AnnotationLike> AnnotationLikes { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<CommentLike> CommentLikes { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserManagedArtist> UserManagedArtists { get; set; }

    public virtual DbSet<VerificationType> VerificationTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=test;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("albums_pkey");

            entity.ToTable("albums");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_image_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");

            entity.HasMany(d => d.Artists).WithMany(p => p.Albums)
                .UsingEntity<Dictionary<string, object>>(
                    "AlbumArtist",
                    r => r.HasOne<Artist>().WithMany()
                        .HasForeignKey("ArtistId")
                        .HasConstraintName("fk_artist"),
                    l => l.HasOne<Album>().WithMany()
                        .HasForeignKey("AlbumId")
                        .HasConstraintName("fk_album"),
                    j =>
                    {
                        j.HasKey("AlbumId", "ArtistId").HasName("album_artists_pkey");
                        j.ToTable("album_artists");
                        j.IndexerProperty<int>("AlbumId").HasColumnName("album_id");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("artist_id");
                    });
        });

        modelBuilder.Entity<Annotation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("annotations_pkey");

            entity.ToTable("annotations");

            entity.HasIndex(e => e.TrackId, "idx_annotations_track");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CharEnd).HasColumnName("char_end");
            entity.Property(e => e.CharStart).HasColumnName("char_start");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.LineEnd).HasColumnName("line_end");
            entity.Property(e => e.LineStart).HasColumnName("line_start");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.VerificationTypeId).HasColumnName("verification_type_id");
            entity.Property(e => e.VerifierId).HasColumnName("verifier_id");

            entity.HasOne(d => d.Author).WithMany(p => p.AnnotationAuthors)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("fk_annotation_author");

            entity.HasOne(d => d.Track).WithMany(p => p.Annotations)
                .HasForeignKey(d => d.TrackId)
                .HasConstraintName("fk_annotation_track");

            entity.HasOne(d => d.VerificationType).WithMany(p => p.Annotations)
                .HasForeignKey(d => d.VerificationTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_annotation_ver_type");

            entity.HasOne(d => d.Verifier).WithMany(p => p.AnnotationVerifiers)
                .HasForeignKey(d => d.VerifierId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_annotation_verifier");
        });

        modelBuilder.Entity<AnnotationComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("annotation_comments_pkey");

            entity.ToTable("annotation_comments");

            entity.HasIndex(e => e.AnnotationId, "idx_comments_annotation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnnotationId).HasColumnName("annotation_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Annotation).WithMany(p => p.AnnotationComments)
                .HasForeignKey(d => d.AnnotationId)
                .HasConstraintName("fk_comment_annotation");

            entity.HasOne(d => d.Author).WithMany(p => p.AnnotationComments)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("fk_comment_author");
        });

        modelBuilder.Entity<AnnotationLike>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.AnnotationId }).HasName("annotation_likes_pkey");

            entity.ToTable("annotation_likes");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AnnotationId).HasColumnName("annotation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Annotation).WithMany(p => p.AnnotationLikes)
                .HasForeignKey(d => d.AnnotationId)
                .HasConstraintName("fk_like_annotation");

            entity.HasOne(d => d.User).WithMany(p => p.AnnotationLikes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_like_user");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("artists_pkey");

            entity.ToTable("artists");

            entity.HasIndex(e => e.Name, "artists_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CommentId }).HasName("comment_likes_pkey");

            entity.ToTable("comment_likes");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentLikes)
                .HasForeignKey(d => d.CommentId)
                .HasConstraintName("fk_like_comment");

            entity.HasOne(d => d.User).WithMany(p => p.CommentLikes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_like_c_user");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genres_pkey");

            entity.ToTable("genres");

            entity.HasIndex(e => e.Name, "genres_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tracks_pkey");

            entity.ToTable("tracks");

            entity.HasIndex(e => e.AlbumId, "idx_tracks_album");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.About).HasColumnName("about");
            entity.Property(e => e.AlbumId).HasColumnName("album_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.Lyrics).HasColumnName("lyrics");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.TrackNumber).HasColumnName("track_number");
            entity.Property(e => e.ViewsCount).HasColumnName("views_count");
            entity.Property(e => e.IsLyricsVerified).HasColumnName("is_lyrics_verified").HasDefaultValue(false);
            entity.Property(e => e.LyricsVerifierId).HasColumnName("lyrics_verifier_id");

            entity.HasOne(d => d.Album).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.AlbumId)
                .HasConstraintName("fk_track_album");

            entity.HasMany(d => d.Artists).WithMany(p => p.Tracks)
                .UsingEntity<Dictionary<string, object>>(
                    "TrackArtist",
                    r => r.HasOne<Artist>().WithMany()
                        .HasForeignKey("ArtistId")
                        .HasConstraintName("fk_artist"),
                    l => l.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .HasConstraintName("fk_track"),
                    j =>
                    {
                        j.HasKey("TrackId", "ArtistId").HasName("track_artists_pkey");
                        j.ToTable("track_artists");
                        j.HasIndex(new[] { "ArtistId" }, "idx_track_artists_artist");
                        j.IndexerProperty<int>("TrackId").HasColumnName("track_id");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("artist_id");
                    });

            entity.HasMany(d => d.Genres).WithMany(p => p.Tracks)
                .UsingEntity<Dictionary<string, object>>(
                    "TrackGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .HasConstraintName("fk_tg_genre"),
                    l => l.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .HasConstraintName("fk_tg_track"),
                    j =>
                    {
                        j.HasKey("TrackId", "GenreId").HasName("track_genres_pkey");
                        j.ToTable("track_genres");
                        j.HasIndex(new[] { "TrackId" }, "idx_track_genres_track");
                        j.IndexerProperty<int>("TrackId").HasColumnName("track_id");
                        j.IndexerProperty<int>("GenreId").HasColumnName("genre_id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.AvatarUrl).HasMaxLength(255).HasColumnName("avatar_url");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_user_role");
        });

        modelBuilder.Entity<UserManagedArtist>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArtistId }).HasName("user_managed_artists_pkey");

            entity.ToTable("user_managed_artists");

            entity.HasIndex(e => e.ArtistId, "idx_uma_artist");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.GrantedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("granted_at");

            entity.HasOne(d => d.Artist).WithMany(p => p.UserManagedArtists)
                .HasForeignKey(d => d.ArtistId)
                .HasConstraintName("fk_uma_artist");

            entity.HasOne(d => d.User).WithMany(p => p.UserManagedArtists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_uma_user");
        });

        modelBuilder.Entity<VerificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("verification_types_pkey");

            entity.ToTable("verification_types");

            entity.HasIndex(e => e.Name, "verification_types_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
