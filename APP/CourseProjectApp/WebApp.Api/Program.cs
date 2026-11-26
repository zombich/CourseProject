using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApp.DatabaseLibrary.Contexts;

var builder = WebApplication.CreateBuilder(args);

var key = new SymmetricSecurityKey(
    Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            RequireExpirationTime = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

try
{
    using var db = new AppDbContext();
    db.Database.ExecuteSqlRaw(@"
        UPDATE roles SET name = 'Пользователь' WHERE id = 1 OR name = 'пользователь';
        UPDATE roles SET name = 'Модератор' WHERE id = 2 OR name = 'модератор';
        UPDATE roles SET name = 'Администратор' WHERE id = 3 OR name = 'администратор';
        UPDATE roles SET name = 'Критик' WHERE id = 4 OR name = 'критик';
        UPDATE roles SET name = 'Артист' WHERE id = 5 OR name = 'артист';
        ALTER TABLE users ADD COLUMN IF NOT EXISTS bio TEXT;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_url VARCHAR(255) NULL;
        ALTER TABLE tracks ADD COLUMN IF NOT EXISTS is_lyrics_verified BOOLEAN NOT NULL DEFAULT FALSE;
        ALTER TABLE tracks ADD COLUMN IF NOT EXISTS lyrics_verifier_id INT NULL REFERENCES users(id) ON DELETE SET NULL;
        SELECT setval(pg_get_serial_sequence('roles', 'id'), COALESCE((SELECT MAX(id) FROM roles), 1));
        SELECT setval(pg_get_serial_sequence('users', 'id'), COALESCE((SELECT MAX(id) FROM users), 1));
        SELECT setval(pg_get_serial_sequence('artists', 'id'), COALESCE((SELECT MAX(id) FROM artists), 1));
        SELECT setval(pg_get_serial_sequence('genres', 'id'), COALESCE((SELECT MAX(id) FROM genres), 1));
        SELECT setval(pg_get_serial_sequence('verification_types', 'id'), COALESCE((SELECT MAX(id) FROM verification_types), 1));
        SELECT setval(pg_get_serial_sequence('albums', 'id'), COALESCE((SELECT MAX(id) FROM albums), 1));
        SELECT setval(pg_get_serial_sequence('tracks', 'id'), COALESCE((SELECT MAX(id) FROM tracks), 1));
        SELECT setval(pg_get_serial_sequence('annotations', 'id'), COALESCE((SELECT MAX(id) FROM annotations), 1));
        SELECT setval(pg_get_serial_sequence('annotation_comments', 'id'), COALESCE((SELECT MAX(id) FROM annotation_comments), 1));
    ");
}
catch
{
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();