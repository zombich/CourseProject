using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Models;

namespace WebApp.Converters
{
    public static class SiteConverter
    {
        private static readonly Random _random = new Random();
        public static string FormatDuration(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}";
        }

        public static string FormatViews(long views)
        {
            if (views >= 1000)
                return $"{(views / 1000.0):0.#} K";
            return views.ToString();
        }

        public static string GetGradient()
        {
            int firstColor = _random.Next(1,360);
            int secondColor = _random.Next(1, 360);
            int thirdColor = _random.Next(1, 360);
            string gradient = $"background: linear-gradient(120deg, hsl({firstColor}, 78%, 89%) 0%, hsl({secondColor}, 72%, 83%) 45%, hsl({thirdColor}, 78%, 91%) 100%);";
            return gradient;
        }

        public static bool HasImage(string avatarUrl) 
            => !string.IsNullOrWhiteSpace(avatarUrl)
        && avatarUrl.StartsWith("/images/", StringComparison.OrdinalIgnoreCase);
    }
}
